// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class SvgImage : Image
   {
      TapGestureRecognizer _tapGestureRecognizer = new TapGestureRecognizer();

      #region Config

      public static string Resources_FullName { get; set; }
      private static double CacheTolerance { get; } = 0.1; // Ratio of size increase required to update the cached image
      private static BindableProperty[] PropertiesRequiringCacheUpdate { get; }
      private static Assembly ExecutingAssembly { get; } = Assembly.GetExecutingAssembly();
      private static Assembly CallingAssembly { get; } = (Device.RuntimePlatform != Device.UWP) ? Assembly.GetCallingAssembly() : Assembly.GetEntryAssembly();

      static SvgImage()
      {
         // Configure default resources suffix
         Assembly assembly = CallingAssembly;
         Type mainType = null;
         foreach (Type type in assembly.ExportedTypes)
         {
            if (type.Name == "MainActivity" || type.Name == "AppDelegate")
            {
               mainType = type;
               break;
            }
         }

         if (mainType != null)
            Resources_FullName = $"{mainType.Namespace}.Resources";

         // Must be executed after the ResourceNameProperty is created
         PropertiesRequiringCacheUpdate = new BindableProperty[]
         {
            AspectProperty, HeightProperty, ResourceNameProperty, WidthProperty
         };
      }

      #endregion

      public SvgImage()
      {
         Assembly executingAssembly = Assembly.GetExecutingAssembly();
         CheckAndAddAssemblyResources(executingAssembly);

         Assembly entryAssembly = Assembly.GetEntryAssembly();
         CheckAndAddAssemblyResources(entryAssembly);

         if (Device.RuntimePlatform != Device.UWP)
         {
            // Calling Assembly.GetCallingAssembly on UWP (Release config) will throw Operation not supported exception, so don't call it for UWP.
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            CheckAndAddAssemblyResources(callingAssembly);
         }

         PropertyChanged += SvgImage_PropertyChanged;
         Aspect = Aspect.AspectFit;

         // Custom tap event, instead of gestures
         _tapGestureRecognizer.Tapped += (sender, e) => Tapped?.Invoke(sender, e);
         GestureRecognizers.Add(_tapGestureRecognizer);
      }

      private void CheckAndAddAssemblyResources(Assembly assembly)
      {
         if (assembly == null) return;

         if (!SvgImageManager.ManifestResourceNames.Keys.Contains(assembly))
         {
            var resources = assembly.GetManifestResourceNames().Where(r => r.EndsWith(".svg"));
            if (resources != null && resources.Count() > 0)
            {
               SvgImageManager.ManifestResourceNames.Add(assembly, resources.ToList());
            }
         }
      }

      private void SvgImage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         // Fix a known issue in ListView where ItemTapped event doesn't get fired if the list view item template has a tap gesture while it also uses
         // ContextActions like long press action to show some menu items, in this case when we add the SvgImage inside list view item template the list 
         // view item will NOT receive the ItemTapped event and so the item will not be clickable, this workaround will fix it by checking for the 
         // SvgImage.IsEnabled property, you need to set it to FALSE when you use the SvgImage inside list view item template.
         if (e.PropertyName.Equals(IsEnabledProperty.PropertyName))
         {
            if (IsEnabled)
            {
               if (!GestureRecognizers.Contains(_tapGestureRecognizer))
                  GestureRecognizers.Add(_tapGestureRecognizer);
            }
            else
               GestureRecognizers.Remove(_tapGestureRecognizer);
         }
      }

      #region Bindable properties

      public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
         propertyName: nameof(ResourceName),
         returnType: typeof(string),
         declaringType: typeof(SvgImage),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay
      );
      public string ResourceName { get => GetValue(ResourceNameProperty) is string value ? value : string.Empty; set => SetValue(ResourceNameProperty, value); }

      #endregion

      #region Events

      public event EventHandler Tapped;

      #endregion

      #region Methods

      #region OnXXX

      protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
      {
         // If loaded, preserve aspect ratio
         if (!string.IsNullOrWhiteSpace(ResourceName))
         {
            SvgImageManager.CalculateBounds(ResourceName, Aspect, widthConstraint, heightConstraint, out SKSizeI pixelSize);
            if (pixelSize.Width != 0 && pixelSize.Height != 0)
               return new SizeRequest(pixelSize.ToFormsSize());
         }

         return base.OnMeasure(widthConstraint, heightConstraint);
      }

      protected override void OnPropertyChanged(string propertyName = null)
      {
         base.OnPropertyChanged(propertyName);

         // Clear source?
         if (propertyName == ResourceNameProperty.PropertyName && string.IsNullOrWhiteSpace(ResourceName))
         {
            if (Source != null)
               MainThread.BeginInvokeOnMainThread(() => Source = null);
            return;
         }

         // Need to update source?
         if (string.IsNullOrWhiteSpace(ResourceName) || !PropertiesRequiringCacheUpdate.Any(property => propertyName == property.PropertyName))
            return;
         if (double.IsInfinity(Width) || Width <= 0 || double.IsInfinity(Height) || Height <= 0)
            return;

         // Update the image source
         SKBitmapImageSource updated = SvgImageManager.Load(ResourceName, Aspect, Width, Height);
         MainThread.BeginInvokeOnMainThread(() =>
         {
            // Disposed?
            if (updated != null && updated.Bitmap != null && updated.Bitmap.Handle == IntPtr.Zero)
               return;

            // Try to update
            if (updated != Source)
               Source = updated;
         });
      }

      #endregion

      #endregion

      #region SvgImageManager

      internal static class SvgImageManager
      {
         #region Internal properties

         internal static Dictionary<Assembly, List<string>> ManifestResourceNames { get; } = new Dictionary<Assembly, List<string>>();
         private static Dictionary<string, Tuple<SKSizeI, SKBitmapImageSource>> ImageSourceCache { get; } = new Dictionary<string, Tuple<SKSizeI, SKBitmapImageSource>>();
         private static Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> SvgCache { get; } = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();

         #endregion

         #region Methods

         static SvgImageManager()
         {
            ManifestResourceNames.Add(ExecutingAssembly, ExecutingAssembly.GetManifestResourceNames().ToList());
            if(ExecutingAssembly != CallingAssembly)
               ManifestResourceNames.Add(CallingAssembly, CallingAssembly.GetManifestResourceNames().ToList());
         }

         public static void CalculateBounds(string source, Aspect aspect, double maxWidth, double maxHeight, out SKSizeI pixelSize)
         {
            if (string.IsNullOrWhiteSpace(source))
               pixelSize = SKSizeI.Empty;
            else
               CalculateBounds(false, LoadSVG(GetKey(source)), aspect, maxWidth, maxHeight, out SKMatrix _, out pixelSize);
         }

         private static void CalculateBounds(bool forDrawing, SkiaSharp.Extended.Svg.SKSvg svg, Aspect aspect, double maxWidth, double maxHeight, out SKMatrix drawMatrix, out SKSizeI pixelSize)
         {
            // Determine bounds of SVG
            SKRect svgBounds = svg.ViewBox;
            if (svgBounds.Width == 0 || svgBounds.Height == 0)
               svgBounds = new SKRect(0, 0, svg.CanvasSize.Width, svg.CanvasSize.Height);

            // Unable to scale?
            double scaleX, scaleY;
            if (svgBounds.Width == 0 || svgBounds.Height == 0 || double.IsInfinity(maxWidth) || double.IsInfinity(maxHeight))
               scaleX = scaleY = 1;
            else
            {
               // Scale properly
               double ratioX = maxWidth / svgBounds.Width, ratioY = maxHeight / svgBounds.Height;
               switch (aspect)
               {
                  case Aspect.AspectFill:
                     scaleX = scaleY = Math.Max(ratioX, ratioY);
                     break;
                  case Aspect.AspectFit:
                     scaleX = scaleY = Math.Min(ratioX, ratioY);
                     break;
                  case Aspect.Fill:
                  default:
                     scaleX = ratioX;
                     scaleY = ratioY;
                     break;
               }
            }

            // Increase sizing for drawing
            if (forDrawing)
            {
               scaleX *= DemoUtilities.DisplayDensity;
               scaleY *= DemoUtilities.DisplayDensity;
            }

            // Apply scaling
            if (svg.ViewBox.Width != 0 && svg.ViewBox.Height != 0)
               drawMatrix = SKMatrix.MakeTranslation(-svg.ViewBox.Left, -svg.ViewBox.Top);
            else
               drawMatrix = SKMatrix.MakeIdentity();
            SKMatrix.PreConcat(ref drawMatrix, SKMatrix.MakeScale((float)scaleX, (float)scaleY));
            pixelSize = new SKSizeI((int)Math.Ceiling(scaleX * svgBounds.Width), (int)Math.Ceiling(scaleY * svgBounds.Height));
         }

         private static string GetKey(string source)
         {
            string key = null;
            string resourceName = source.Replace('/', '.');
            foreach(KeyValuePair<Assembly, List<string>> entry in ManifestResourceNames)
            {
               if (entry.Value.Contains(resourceName))
               {
                  key = resourceName;
               }
               else if (entry.Value.Contains($"{Resources_FullName}.{resourceName}", StringComparer.InvariantCultureIgnoreCase))
               {
                  // Name within Images directory was given
                  key = $"{Resources_FullName}.{resourceName}";
               }
               else
               {
                  // Try to locate as suffix
                  key = entry.Value.FirstOrDefault(name => name.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase));
               }

               if (!string.IsNullOrWhiteSpace(key))
                  break;
            }

            return key;
         }

         public static SKBitmapImageSource Load(string source, Aspect aspect, double width, double height)
         {
            // No image needed?
            if (string.IsNullOrWhiteSpace(source) || width <= 0 || height <= 0)
               return null;

            // Read the image
            string key = GetKey(source);
            SkiaSharp.Extended.Svg.SKSvg svg = LoadSVG(key);
            CalculateBounds(true, svg, aspect, width, height, out SKMatrix drawMatrix, out SKSizeI pixelSize);

            // Check the cache
            if (ImageSourceCache.TryGetValue(key, out Tuple<SKSizeI, SKBitmapImageSource> cacheEntry))
               if (cacheEntry.Item1.Width >= pixelSize.Width * (1 - CacheTolerance) &&
                  cacheEntry.Item1.Height >= pixelSize.Height * (1 - CacheTolerance))
               {
                  // Already cached
                  return cacheEntry.Item2;
               }
               else
               {
                  // Dispose/remove current entry
                  cacheEntry.Item2.Bitmap?.Dispose();
                  ImageSourceCache.Remove(key);
               }

            // Convert to an SKBitmapImageSource
            using (SKImage image = SKImage.FromPicture(svg.Picture, pixelSize, drawMatrix))
            {
               SKBitmapImageSource imageSource = new SKBitmapImageSource()
               {
                  Bitmap = SKBitmap.FromImage(image)
               };
               ImageSourceCache.Add(key, Tuple.Create(pixelSize, imageSource));
               return imageSource;
            }
         }

         private static SkiaSharp.Extended.Svg.SKSvg LoadSVG(string key)
         {
            Assembly assembly = null;
            foreach (KeyValuePair<Assembly, List<string>> entry in ManifestResourceNames)
            {
               if (entry.Value.Contains(key))
               {
                  assembly = entry.Key;
                  break;
               }
            }

            if (assembly == null) throw new Exception($"Failed to locate assembly for resource {key}");

            // If not found in cache, load
            if (!SvgCache.TryGetValue(key, out SkiaSharp.Extended.Svg.SKSvg svg))
               using (Stream stream = assembly.GetManifestResourceStream(key))
               {
                  svg = new SkiaSharp.Extended.Svg.SKSvg();
                  svg.Load(stream);
                  SvgCache.Add(key, svg);
               }
            return svg;
         }

         #endregion
      }

      #endregion
   }
}