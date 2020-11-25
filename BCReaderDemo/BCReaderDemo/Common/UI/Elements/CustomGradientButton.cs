// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Utils;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   public enum CustomButtonDrawMode
   {
      None,
      Border,
      Gradient,
      GradientBorder
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomGradientButton : SKCanvasView
   {
      #region Config

      private static Color Border_Fill { get; } = Color.Transparent;
      private static Color Border_Stroke { get; } = Color.FromRgb(87, 104, 127);
      private static double Border_Radius { get; } = 999; // Will be limited to 50% of size
      private static double Border_Width { get; } = 3;

      private static CustomButtonDrawMode DrawMode_Selected { get; } = CustomButtonDrawMode.Gradient;
      private static CustomButtonDrawMode DrawMode_Unselected { get; } = CustomButtonDrawMode.Border;

      private static FontAttributes Font_Attributes { get; } = FontAttributes.Bold;
      private static string Font_Family { get; } = DemoUtilities.FontFamily;
      private double Padding => 0.75 * FontSize;

      private static Color Gradient_StartColor { get; } = Color.FromRgb(70, 155, 211);
      private static Color Gradient_EndColor { get; } = Color.FromRgb(151, 111, 232);
      private static double Gradient_Angle { get; } = 18;

      private static Color Text_SelectedColor { get; } = Color.FromRgb(215, 223, 236);
      private static Color Text_UnselectedColor { get; } = Color.FromRgb(103, 119, 140);

      #endregion

      public CustomGradientButton()
      {
         // Enable touch events (allows for custom filtering)
         EnableTouchEvents = true;

         // Custom tap event, instead of gestures
         TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Tapped += (sender, e) => Tapped?.Invoke(sender, e);
         GestureRecognizers.Add(tapGestureRecognizer);
      }

      #region Bindable properties

      #region FontSize

      public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
         propertyName: nameof(FontSize),
         returnType: typeof(double),
         declaringType: typeof(CustomGradientButton),
         defaultValue: FontSizeExtension.SmallFontSize,
         defaultBindingMode: BindingMode.TwoWay
      );
      public double FontSize { get => GetValue(FontSizeProperty) is double value ? value : 0.0; set => SetValue(FontSizeProperty, value); }

      #endregion

      #region IsSelected

      public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
         propertyName: nameof(IsSelected),
         returnType: typeof(bool),
         declaringType: typeof(CustomGradientButton),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomGradientButton self)
            {
               // Update other properties
               if (self.IsSelected)
               {
                  self.DrawMode = DrawMode_Selected;
                  self.TextColor = Text_SelectedColor;
               }
               else
               {
                  self.DrawMode = DrawMode_Unselected;
                  self.TextColor = Text_UnselectedColor;
               }

               // Redraw
               self.InvalidateSurface();
            }
         }
      );
      public bool IsSelected { get => GetValue(IsSelectedProperty) is bool value ? value : false; set => SetValue(IsSelectedProperty, value); }

      #endregion

      #region Text

      public static readonly BindableProperty TextProperty = BindableProperty.Create(
         propertyName: nameof(Text),
         returnType: typeof(string),
         declaringType: typeof(CustomGradientButton),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay
      );
      public string Text { get => GetValue(TextProperty) is string value ? value : string.Empty; set => SetValue(TextProperty, value); }

      #endregion

      #endregion

      #region Events

      public event EventHandler Tapped;

      #endregion

      #region Internal properties

      private CustomButtonDrawMode DrawMode { get; set; } = DrawMode_Unselected;
      private Color TextColor { get; set; } = Text_UnselectedColor;
      private static SKTypeface Typeface { get; set; } = null;

      #endregion

      #region Methods

      #region OnXXX

      protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
      {
         CalculateBounds(false, widthConstraint, heightConstraint, out SKRect bounds, out double _, out SKRect _, out double _);
         if (bounds.Width != 0 && bounds.Height != 0)
            return new SizeRequest(new Size(bounds.Width, bounds.Height));

         return base.OnMeasure(widthConstraint, heightConstraint);
      }

      protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
      {
         base.OnPaintSurface(e);

         // Clear existing
         SKCanvas canvas = e.Surface.Canvas;
         canvas.Clear();

         // Background color
         if (BackgroundColor.A > 0)
            using (SKPaint paint = new SKPaint())
            {
               paint.Color = BackgroundColor.ToSKColor();
               paint.Style = SKPaintStyle.Fill;
               canvas.DrawRect(new SKRect(0, 0, CanvasSize.Width, CanvasSize.Height), paint);
            }

         // Draw the various elements
         using (SKPaint paint = new SKPaint())
         {
            // Set default options
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;

            // Determine the drawing bounds
            CalculateBounds(true, CanvasSize.Width, CanvasSize.Height, out SKRect _, out double strokeWidth, out SKRect rect, out double cornerRadius);

            // Draw the gradient, if it's visible
            if ((DrawMode == CustomButtonDrawMode.Gradient || DrawMode == CustomButtonDrawMode.GradientBorder) && (Gradient_StartColor.A > 0 || Gradient_EndColor.A > 0))
            {
               // Determine where to draw the gradient
               CalculateGradientPoints(rect, out SKPoint start, out SKPoint end);

               // Create gradient shader
               using (SKShader gradientShader = CreateGradientShader(start, end))
               {
                  // Apply shader
                  paint.Shader = gradientShader;

                  // Actually draw
                  DrawRect(canvas, rect, cornerRadius, paint);

                  // Clear shader
                  paint.Shader = null;
               }
            }

            // Draw the border fill, if it's visible
            if (DrawMode == CustomButtonDrawMode.Border && Border_Fill.A > 0)
            {
               // Update color
               paint.Color = Border_Fill.ToSKColor();

               // Actually draw
               DrawRect(canvas, rect, cornerRadius, paint);
            }

            // Draw the text, if it's visible
            if (!string.IsNullOrWhiteSpace(Text) && !string.IsNullOrWhiteSpace(Font_Family) && FontSize > 0 && TextColor.A > 0)
            {
               // Update SKPaint
               paint.Color = TextColor.ToSKColor();
               paint.TextSize = (float)(FontSize * DemoUtilities.DisplayDensity);
               paint.Typeface = GetTypeface();

               // Measure the text
               SKRect textBounds = new SKRect();
               paint.MeasureText(Text, ref textBounds);

               // Actually draw, centered
               canvas.DrawText(Text, rect.MidX - textBounds.MidX, rect.MidY - textBounds.MidY, paint);
            }

            // Drawing with Stroke
            paint.StrokeWidth = (float)strokeWidth;
            paint.Style = SKPaintStyle.Stroke;

            // Draw the border stroke, if it's visible
            if ((DrawMode == CustomButtonDrawMode.Border || DrawMode == CustomButtonDrawMode.GradientBorder) && Border_Stroke.A > 0 && strokeWidth > 0)
            {
               // Update color
               paint.Color = Border_Stroke.ToSKColor();

               // Actually draw
               DrawRect(canvas, rect, cornerRadius, paint);
            }
         }

         // Flush the changes
         canvas.Flush();
      }

      protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         base.OnPropertyChanged(propertyName);

         // Redraw
         InvalidateSurface();
      }

      protected override void OnTouch(SKTouchEventArgs e)
      {
         // Manually perform hit detection
         CalculateBounds(true, CanvasSize.Width, CanvasSize.Height, out SKRect _, out double strokeWidth, out SKRect rect, out double cornerRadius);
         rect.Inflate((float)(strokeWidth * 0.5), (float)(strokeWidth * 0.5));
         if (cornerRadius <= 0)
         {
            // Just a normal rectangle, use rect
            if (!rect.Contains(e.Location))
            {
               e.Handled = true;
               return;
            }
         }
         else
            // Use SkiaSharp's round rectangle helper class
            using (SKRoundRect rrect = new SKRoundRect(rect, (float)cornerRadius, (float)cornerRadius))
            {
               // The round rectangle method doesn't have a Contains(SKPoint) method
               float delta = 0.01f; // Must be non-zero
               SKRect location = new SKRect(e.Location.X - delta, e.Location.Y - delta, e.Location.X + delta, e.Location.Y + delta);
               if (!rrect.Contains(location))
               {
                  e.Handled = true;
                  return;
               }
            }

         // Intersects with shape, fallback to default behavior
         base.OnTouch(e);
      }

      #endregion

      #region Helpers

      private static void CalculateAlignedBounds(LayoutAlignment horizontalAlignment, LayoutAlignment verticalAlignment, double maxWidth, double maxHeight, double width, double height, double scaleX, double scaleY, out SKRect bounds)
      {
         // Handle alignment
         double left, top;
         switch (horizontalAlignment)
         {
            case LayoutAlignment.Center:
            case LayoutAlignment.Fill:
            default:
               left = (maxWidth - scaleX * width) * 0.5;
               break;
            case LayoutAlignment.End:
               left = maxWidth - scaleX * width;
               break;
            case LayoutAlignment.Start:
               left = 0;
               break;
         }
         switch (verticalAlignment)
         {
            case LayoutAlignment.Center:
            case LayoutAlignment.Fill:
            default:
               top = (maxHeight - scaleY * height) * 0.5;
               break;
            case LayoutAlignment.End:
               top = maxHeight - scaleY * height;
               break;
            case LayoutAlignment.Start:
               top = 0;
               break;
         }

         // Update bounds
         bounds = new SKRect(
            (float)left, (float)top,
            (float)(left + scaleX * width),
            (float)(top + scaleY * height)
         );
      }

      public void CalculateBounds(bool forDrawing, double maxWidth, double maxHeight, out SKRect bounds, out double strokeWidth, out SKRect rect, out double cornerRadius)
      {
         // Stroke only visible with Border
         if (DrawMode == CustomButtonDrawMode.Border || DrawMode == CustomButtonDrawMode.GradientBorder)
            strokeWidth = Border_Width;
         else
            strokeWidth = 0;

         // Determine the minimum size
         double minWidth = 2 * (Border_Width + Padding), minHeight = 2 * (Border_Width + Padding);
         if (!string.IsNullOrWhiteSpace(Text) && !string.IsNullOrWhiteSpace(Font_Family) && FontSize > 0 /* Zero alpha is fine here */)
            using (SKPaint paint = new SKPaint())
            {
               // Update SKPaint
               paint.TextSize = (float)FontSize;
               paint.Typeface = GetTypeface();

               // Measure the text
               SKRect textBounds = new SKRect();
               paint.MeasureText(Text, ref textBounds);

               // Increase minimum size
               minWidth += textBounds.Width;
               minHeight += textBounds.Height;
            }

         // Increase sizing for drawing
         if (forDrawing)
         {
            minWidth *= DemoUtilities.DisplayDensity;
            minHeight *= DemoUtilities.DisplayDensity;
         }

         // Try to expand bounds to max
         if (double.IsInfinity(maxWidth) || double.IsInfinity(maxHeight))
            bounds = new SKRect(0, 0, (float)minWidth, (float)minHeight);
         else
         {
            // Fill?
            double scaleX = maxWidth / minWidth, scaleY = maxHeight / minHeight;
            LayoutAlignment halign = HorizontalOptions.Alignment, valign = VerticalOptions.Alignment;
            if (halign != LayoutAlignment.Fill && scaleX > 1)
               scaleX = 1;
            if (valign != LayoutAlignment.Fill && scaleY > 1)
               scaleY = 1;

            // Expand to fill
            CalculateAlignedBounds(halign, valign, maxWidth, maxHeight, minWidth, minHeight, scaleX, scaleY, out bounds);
         }

         // Deflate bounds by stroke width (with a few extra pixels to account for aliasing)
         double margin = strokeWidth * 0.5 + 2;
         rect = new SKRect(
            (float)(bounds.Left + strokeWidth), (float)(bounds.Top + strokeWidth),
            (float)(bounds.Right - strokeWidth), (float)(bounds.Bottom - strokeWidth)
         );

         // Limit the corner radius to 50%
         cornerRadius = Math.Min(Math.Min(Border_Radius, rect.Width * 0.5), rect.Height * 0.5);
      }

      private static void CalculateGradientPoints(SKRect rect, out SKPoint start, out SKPoint end)
      {
         // Gradient ray
         double centerX = rect.MidX, centerY = rect.MidY;
         double rayDX = Math.Cos(Gradient_Angle * Math.PI / 180), rayDY = -Math.Sin(Gradient_Angle * Math.PI / 180);

         // Find maximum bounds along line
         double tMin = 0, tMax = 0;
         ValueTuple<double, double>[] corners = new ValueTuple<double, double>[]
         {
            (rect.Left, rect.Top), (rect.Left, rect.Bottom),
            (rect.Right, rect.Bottom), (rect.Right, rect.Top)
         };
         foreach (var (cornerX, cornerY) in corners)
         {
            // Orthogonal projection (with assumption of unit line vector)
            double t = (cornerX - centerX) * rayDX + (cornerY - centerY) * rayDY;
            tMin = Math.Min(tMin, t);
            tMax = Math.Max(tMax, t);
         }

         // Generate output points
         start = new SKPoint((float)(centerX + tMin * rayDX), (float)(centerY + tMin * rayDY));
         end = new SKPoint((float)(centerX + tMax * rayDX), (float)(centerY + tMax * rayDY));
      }

      private static SKShader CreateGradientShader(SKPoint start, SKPoint end)
      {
         return SKShader.CreateLinearGradient(
            start, end,
            new SKColor[]
            {
               Gradient_StartColor.ToSKColor(),
               Gradient_EndColor.ToSKColor()
            },
            null, SKShaderTileMode.Clamp
         );
      }

      private static void DrawRect(SKCanvas canvas, SKRect rect, double cornerRadius, SKPaint paint)
      {
         if (cornerRadius <= 0)
            // Just a normal rectangle
            canvas.DrawRect(rect, paint);
         else
            canvas.DrawRoundRect(rect, (float)cornerRadius, (float)cornerRadius, paint);
      }

      private static SKTypeface GetTypeface()
      {
         // Forms to SK
         SKFontStyleSlant skSlant = Font_Attributes.HasFlag(FontAttributes.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
         SKFontStyleWeight skWeight = Font_Attributes.HasFlag(FontAttributes.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;

         // Check cached font
         if (Typeface == null || Typeface.FamilyName != Font_Family || Typeface.FontSlant != skSlant || Typeface.FontWeight != (int)skWeight)
         {
            // Dispose previous
            Typeface?.Dispose();

            // Load new font
            Typeface = SKTypeface.FromFamilyName(Font_Family, skWeight, SKFontStyleWidth.Normal, skSlant);
         }

         return Typeface;
      }

      #endregion

      #endregion
   }
}
