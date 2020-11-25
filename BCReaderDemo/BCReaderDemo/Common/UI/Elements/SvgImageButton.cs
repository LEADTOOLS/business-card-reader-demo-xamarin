using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class SvgImageButton : ImageButton
   {
      #region Config

      private static BindableProperty[] PropertiesRequiringCacheUpdate { get; }

      static SvgImageButton()
      {
         // Must be executed after the ResourceNameProperty is created
         PropertiesRequiringCacheUpdate = new BindableProperty[]
         {
            AspectProperty, HeightProperty, ResourceNameProperty, WidthProperty
         };
      }

      #endregion

      public SvgImageButton()
      {
         Aspect = Aspect.AspectFit;
         BackgroundColor = Color.Transparent;
      }

      #region Bindable properties

      #region ResourceName

      public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
         propertyName: nameof(ResourceName),
         returnType: typeof(string),
         declaringType: typeof(SvgImageButton),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay
      );
      public string ResourceName { get => GetValue(ResourceNameProperty) is string value ? value : string.Empty; set => SetValue(ResourceNameProperty, value); }

      #endregion

      #endregion

      #region Methods

      #region OnXXX

      protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
      {
         // If loaded, preserve aspect ratio
         if (!string.IsNullOrWhiteSpace(ResourceName))
         {
            SvgImage.SvgImageManager.CalculateBounds(ResourceName, Aspect, widthConstraint, heightConstraint, out SKSizeI pixelSize);
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
         SKBitmapImageSource updated = SvgImage.SvgImageManager.Load(ResourceName, Aspect, Width, Height);
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
   }
}