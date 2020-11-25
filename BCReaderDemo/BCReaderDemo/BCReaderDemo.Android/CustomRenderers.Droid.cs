// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using BCReaderDemo;
using BCReaderDemo.Droid;
using BCReaderDemo.Utils;
using CarouselView.FormsPlugin.Abstractions;
using CarouselView.FormsPlugin.Android;
using Leadtools.Demos;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(TableViewModelRenderer), typeof(CustomTableViewModelRenderer))]
[assembly: ExportRenderer(typeof(CustomTableView), typeof(CustomTableViewRenderer))]
[assembly: ExportRenderer(typeof(GradientContentView), typeof(GradientContentViewRenderer))]
[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Switch), typeof(CustomSwitchRenderer))]
[assembly: ExportRenderer(typeof(MyCarouselViewControl), typeof(MyCarouselViewControlRenderer))]

namespace BCReaderDemo.Droid
{
   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for TableView in order to customize its sections margins and sections text attributes */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomTableViewModelRenderer : TableViewModelRenderer
   {
      public CustomTableViewModelRenderer(Context context, Android.Widget.ListView listView, TableView view) : base(context, listView, view)
      {
      }

      public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
      {
         var view = base.GetView(position, convertView, parent);

         var element = GetCellForPosition(position);

         // Section header will be a TextCell.
         if (element.GetType() == typeof(TextCell))
         {
            try
            {
               // Get the textView of the actual layout.
               var textView = ((((view as LinearLayout).GetChildAt(0) as LinearLayout).GetChildAt(1) as LinearLayout).GetChildAt(0) as TextView);

               textView.SetText(textView.Text.ToUpper(), TextView.BufferType.Normal);

               textView.TextSize = (float)PlatformsConstants.TableViewGroupHeaderFontSize;

               // Set section header left padding to zero.
               textView.SetPadding((int) (15 * DemoUtilities.DisplayDensity), view.PaddingTop, view.PaddingRight, view.PaddingBottom);

               // Set section header text font to bold.
               //textView.SetTypeface(Typeface.Default, TypefaceStyle.Bold);

               // Set the section header color.
               textView.SetTextColor(CustomColors.DarkSharkonColor.ToAndroid());

               // Get the divider below the header
               var divider = (view as LinearLayout).GetChildAt(1);
               divider.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1);

               // Set divider color to gray
               divider.SetBackgroundColor(CustomColors.LightSharkonColor.ToAndroid());
            }
            catch (Exception) { }
         }

         return view;
      }
   }

   public class CustomTableViewRenderer : TableViewRenderer
   {
      public CustomTableViewRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
      {
         base.OnElementChanged(e);

         if (Control == null)
            return;

         if (e.NewElement != null)
         {
            Control.Adapter = new CustomTableViewModelRenderer(Context, Control, e.NewElement);

            var listView = Control as global::Android.Widget.ListView;
            //listView.DividerHeight = 0;

            listView.Divider.SetTint(Xamarin.Forms.Color.Transparent.ToAndroid());
            listView.SetHeaderDividersEnabled(false);
         }
      }
   }

   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom ContentView with gradient background                                                           */
   /*                                                                                                       */
   /*********************************************************************************************************/

   public class GradientContentViewRenderer : ViewRenderer
   {

      public GradientContentViewRenderer(Context context) : base(context)
      {
      }

      protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
      {
         Android.Graphics.Drawables.GradientDrawable gd = new Android.Graphics.Drawables.GradientDrawable(Android.Graphics.Drawables.GradientDrawable.Orientation.TopBottom,
            new int[] { new global::Android.Graphics.Color(255, 255, 255, 0), new global::Android.Graphics.Color(255, 255, 255, 255), new global::Android.Graphics.Color(255, 255, 255, 255) });

         Background = gd;

         base.DispatchDraw(canvas);
      }
   }

   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom editor renderer to change the divider color                                                    */
   /*                                                                                                       */
   /*********************************************************************************************************/

   public class CustomEditorRenderer : Xamarin.Forms.Platform.Android.EditorRenderer
   {
      public CustomEditorRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
      {
         base.OnElementChanged(e);

         if (Control == null || e.NewElement == null)
            return;

         if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            Control.BackgroundTintList = ColorStateList.ValueOf(CustomColors.LightSharkonColor.ToAndroid());
         else
            Control.Background.SetColorFilter(CustomColors.LightSharkonColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
      }
   }

   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom picker renderer to change the divider color                                                    */
   /*                                                                                                       */
   /*********************************************************************************************************/

   public class CustomPickerRenderer : Xamarin.Forms.Platform.Android.AppCompat.PickerRenderer
   {
      public CustomPickerRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
      {
         base.OnElementChanged(e);

         if (Control == null || e.NewElement == null)
            return;

         Control.SetPadding(0, 0, Control.PaddingRight, Control.PaddingBottom);
         Control.TextSize = (float)PlatformsConstants.TableViewFieldEntryFontSize;

         if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            Control.BackgroundTintList = ColorStateList.ValueOf(CustomColors.LightSharkonColor.ToAndroid());
         else
            Control.Background.SetColorFilter(CustomColors.LightSharkonColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
      }
   }

   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom entry renderer to show alert image on validation and change the divider color                  */
   /*                                                                                                       */
   /*********************************************************************************************************/

   public class CustomEntryRenderer : EntryRenderer
   {
      public CustomEntryRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
      {
         base.OnElementChanged(e);

         if (Control == null || e.NewElement == null)
            return;

         Control.SetPadding(5, 0, Control.PaddingRight, 10);
         Control.SetElegantTextHeight(true);

         if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            Control.BackgroundTintList = ColorStateList.ValueOf(CustomColors.LightSharkonColor.ToAndroid());
         else
            Control.Background.SetColorFilter(CustomColors.LightSharkonColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
      }

      protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
      {
         base.OnLayout(changed, left, top, right, bottom);

         if (Element != null)
            (Element as CustomEntry).ValidateText(Control.Text);
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         if (e.PropertyName == CustomEntry.ShowCautionImageProperty.PropertyName)
         {
            CustomEntry element = (CustomEntry)Element;

            if (element.ShowCautionImage)
            {
               // check if we already added this image, if so then no need to proceed.
               if (Control.GetCompoundDrawablesRelative().Length > 0) return;

               var drawableImage = ContextCompat.GetDrawable(Context, Resource.Drawable.caution);
               drawableImage.SetBounds(0, 0, Control.Height, Control.Height);

               Control.SetCompoundDrawablesRelative(null, null, drawableImage, null);
            }
            else
            {
               Control.SetCompoundDrawables(null, null, null, null);
            }
         }
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom CarouselViewControl renderer to get tap position                                               */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class MyCarouselViewControlRenderer : CarouselView.FormsPlugin.Android.CarouselViewRenderer
   {
      private TapGestureListener _tapGetsureListener = null;
      private GestureDetector _tapGestureDetector = null;
      public MyCarouselViewControlRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<CarouselViewControl> e)
      {
         base.OnElementChanged(e);

         if(e.NewElement != null)
         {
            _tapGetsureListener = new TapGestureListener();
            _tapGetsureListener.SingleTap += _tapGetsureListener_SingleTap;
            _tapGestureDetector = new GestureDetector(Context, _tapGetsureListener);
         }

         if (e.OldElement != null)
         {
            _tapGetsureListener.SingleTap -= _tapGetsureListener_SingleTap;
         }
      }

      public override bool DispatchTouchEvent(MotionEvent e)
      {
         _tapGestureDetector.OnTouchEvent(e);

         return base.DispatchTouchEvent(e);
      }

      private void _tapGetsureListener_SingleTap(object sender, MotionEvent e)
      {
         if (Element == null)
            return;

         float scale = Resources.System.DisplayMetrics.Density;
         Xamarin.Forms.Point pt = new Xamarin.Forms.Point((e.GetX() / scale), (e.GetY() / scale));
         (Element as MyCarouselViewControl).LastTapPosition = pt;
      }
   }

   internal class TapGestureListener : GestureDetector.SimpleOnGestureListener
   {
      public event EventHandler<MotionEvent> SingleTap;
      public override bool OnSingleTapUp(MotionEvent e)
      {
         if (SingleTap != null)
            SingleTap(this, e);
         return base.OnSingleTapUp(e);
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom Switch renderer to change its color and look                                                   */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomSwitchRenderer : SwitchRenderer
   {
      public CustomSwitchRenderer(Context context) : base(context)
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
      {
         base.OnElementChanged(e);

         UpdateSwitchStyle();
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         if (e.PropertyName == Xamarin.Forms.Switch.IsToggledProperty.PropertyName || e.PropertyName == Xamarin.Forms.Switch.IsEnabledProperty.PropertyName)
         {
            UpdateSwitchStyle();
         }
      }

      private void UpdateSwitchStyle()
      {
         Control.SetBackground(GetRoundRectangleDrawableShape(Element.IsToggled));
         if (Element.IsToggled)
         {
            Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.Multiply);
            Control.TrackDrawable.SetColorFilter((Element.IsEnabled) ? CustomColors.BlueButtonColor.ToAndroid() : Xamarin.Forms.Color.LightGray.ToAndroid(), PorterDuff.Mode.SrcAtop);
         }
         else
         {
            Control.ThumbDrawable.SetColorFilter((Element.IsEnabled) ? CustomColors.BlueButtonColor.ToAndroid() : Xamarin.Forms.Color.LightGray.ToAndroid(), PorterDuff.Mode.Multiply);
            Control.TrackDrawable.SetColorFilter(Android.Graphics.Color.Transparent, PorterDuff.Mode.SrcAtop);
         }
      }

      private ShapeDrawable GetRoundRectangleDrawableShape(bool isToggled)
      {
         var radius = ConvertDPToPixels(13);
         float[] radiusArray = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
         Android.Graphics.Drawables.Shapes.RoundRectShape roundRectShape = null;
         if(isToggled)
            roundRectShape = new Android.Graphics.Drawables.Shapes.RoundRectShape(radiusArray, null, null);
         else
            roundRectShape = new Android.Graphics.Drawables.Shapes.RoundRectShape(radiusArray, new RectF(4, 4, 4, 4), radiusArray);

         ShapeDrawable shapeDrawable = new ShapeDrawable(roundRectShape);
         shapeDrawable.Paint.Color = (Element.IsEnabled) ? CustomColors.BlueButtonColor.ToAndroid() : Xamarin.Forms.Color.LightGray.ToAndroid();
         shapeDrawable.Paint.StrokeWidth = 4;
         shapeDrawable.Paint.StrokeCap = Paint.Cap.Round;
         shapeDrawable.Paint.StrokeJoin = Paint.Join.Round;
         shapeDrawable.Paint.SetStyle(Paint.Style.Fill);

         return shapeDrawable;
      }

      private int ConvertDPToPixels(int dp)
      {
         var scale = global::Android.App.Application.Context.Resources.DisplayMetrics.Density;
         var px = (int)(dp * scale + 0.5);
         return px;
      }
   }
}
