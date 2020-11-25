// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using BCReaderDemo.Utils;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Leadtools.Demos;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TableViewModelRenderer), typeof(CustomTableViewModelRenderer))]
[assembly: ExportRenderer(typeof(CustomTableView), typeof(CustomTableViewRenderer))]
[assembly: ExportRenderer(typeof(GradientContentView), typeof(GradientContentViewRenderer))]
[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(CustomSwitchRenderer))]

namespace BCReaderDemo.iOS
{
   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for TableView in order to customize its sections margins and sections text attributes */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomTableViewModelRenderer : TableViewModelRenderer
   {
      public CustomTableViewModelRenderer(TableView model) : base(model)
      {
      }

      public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
      {
         if (headerView is UITableViewHeaderFooterView sectionHeaderView)
         {
            sectionHeaderView.TextLabel.TextColor = CustomColors.LightSharkonColor.ToUIColor();
            sectionHeaderView.TextLabel.Font = UIFont.BoldSystemFontOfSize((nfloat)PlatformsConstants.TableViewGroupHeaderFontSize);

            for (int i = sectionHeaderView.Layer.Sublayers.Length - 1; i >= 0; i--)
            {
               CALayer sublayer = sectionHeaderView.Layer.Sublayers[i];
               if (sublayer.Frame.Height == 0.5f)
                  sublayer.RemoveFromSuperLayer();
            }

            CALayer borderLayer = new CALayer();
            borderLayer.MasksToBounds = true;
            borderLayer.Frame = new CGRect(0f, sectionHeaderView.Frame.Height - 3, tableView.Frame.Width, 0.5f);
            borderLayer.BorderColor = CustomColors.LightSharkonColor.ToCGColor();
            borderLayer.BorderWidth = 0.5f;
            sectionHeaderView.Layer.AddSublayer(borderLayer);
         }
      }

      public override nfloat GetHeightForHeader(UITableView tableView, nint section)
      {
         return 15;
      }

      public override nfloat GetHeightForFooter(UITableView tableView, nint section)
      {
         return 0.000001f;
      }
   }

   public class CustomTableViewRenderer : TableViewRenderer
   {
      protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
      {
         base.OnElementChanged(e);

         if (Control == null)
            return;

         Control.ContentInset = new UIEdgeInsets(-10, 0, 0, 0);
         Control.Bounces = false;
         Control.SeparatorColor = CustomColors.LightSharkonColor.ToUIColor();
         if (e.NewElement != null)
            Control.Source = new CustomTableViewModelRenderer(e.NewElement);
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for Entry field to customize its border to only show a line under the text            */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomEntryRenderer : EntryRenderer
   {
      private CALayer _borderLayer = null;

      protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
      {
         base.OnElementChanged(e);

         if (Control == null)
            return;

         _borderLayer = new CALayer();
         _borderLayer.MasksToBounds = true;
         _borderLayer.Frame = new CGRect(0f, Frame.Height, Frame.Width, 0.5f);
         _borderLayer.BorderColor = CustomColors.LightSharkonColor.ToCGColor();
         _borderLayer.BorderWidth = 0.5f;
         Control.Layer.AddSublayer(_borderLayer);
         Control.BorderStyle = UITextBorderStyle.None;
         Control.ReturnKeyType = UIReturnKeyType.Done;

         if(Control.KeyboardType == UIKeyboardType.PhonePad)
            AddDoneButton();
      }

      public override void LayoutSubviews()
      {
         base.LayoutSubviews();

         if (Element != null)
            (Element as CustomEntry).ValidateText(Control.Text);
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         Entry entry = sender as Entry;

         if (e.PropertyName.Equals("Width") || e.PropertyName.Equals("Height"))
         {
            if (_borderLayer != null)
               _borderLayer.Frame = new CGRect(0f, entry.Bounds.Height, entry.Bounds.Width, 0.5f);
         }
         else if(e.PropertyName == CustomEntry.ShowCautionImageProperty.PropertyName)
         {
            CustomEntry customEntry = Element as CustomEntry;
            if(customEntry.ShowCautionImage)
            {
               // check if we already added this image, if so then no need to proceed.
               if (Control.RightView != null) return;

               UIImageView uiImage = new UIImageView(UIImage.FromBundle("caution.png"))
               {
                  Frame = new CGRect(0, 0, Control.Frame.Height, Control.Frame.Height),
               };

               UIView objRightView = new UIView(new CGRect(0, 0, Control.Frame.Height + 5, Control.Frame.Height + 5));
               objRightView.AddSubview(uiImage);

               Control.RightView = objRightView;
               Control.RightViewMode = UITextFieldViewMode.Always;
            }
            else
            {
               Control.RightView = null;
            }
         }
      }

      protected void AddDoneButton()
      {
         UIToolbar toolbar = new UIToolbar(new CGRect(0.0f, 0.0f, 50.0f, 44.0f));

         var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
         {
            this.Control.ResignFirstResponder();
         });

         toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton
            };
         this.Control.InputAccessoryView = toolbar;
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for Editor field to customize its border to only show a line under the text           */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomEditorRenderer : EditorRenderer
   {
      protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
      {
         base.OnElementChanged(e);

         if (Control == null)
            return;

         Control.BackgroundColor = UIColor.Clear;
         Control.Layer.BorderWidth = 1;
         Control.Layer.BorderColor = CustomColors.LightSharkonColor.ToCGColor();
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for Picker field to customize its border to only show a line under the text           */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomPickerRenderer : PickerRenderer
   {
      private CALayer _borderLayer = null;
      protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
      {
         base.OnElementChanged(e);

         if (Control == null)
            return;

         Control.Font = UIFont.SystemFontOfSize((nfloat)PlatformsConstants.TableViewFieldEntryFontSize);
         if (_borderLayer == null)
         {
            _borderLayer = new CALayer();
            _borderLayer.MasksToBounds = true;
            _borderLayer.Frame = new CGRect(0f, Frame.Height, Frame.Width, 0.5f);
            _borderLayer.BorderColor = CustomColors.LightSharkonColor.ToCGColor();
            _borderLayer.BorderWidth = 0.5f;
            Control.Layer.AddSublayer(_borderLayer);
            Control.BorderStyle = UITextBorderStyle.None;
         }
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         Picker picker = sender as Picker;

         if (e.PropertyName.Equals("Width") || e.PropertyName.Equals("Height"))
         {
            if (_borderLayer != null)
               _borderLayer.Frame = new CGRect(0f, Math.Max(Frame.Height, picker.Bounds.Height), picker.Bounds.Width, 0.5f);
         }
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer to show gradient area under the floating buttons                                      */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class GradientContentViewRenderer : VisualElementRenderer<ContentView>
   {
      public GradientContentViewRenderer()
      {
      }

      protected override void OnElementChanged(ElementChangedEventArgs<ContentView> e)
      {
         base.OnElementChanged(e);

         if (NativeView != null && e.NewElement != null)
         {
            NativeView.BackgroundColor = UIColor.Clear;

            CAGradientLayer gradientLayer = new CAGradientLayer()
            {
               StartPoint = new CGPoint(0.5, 0),
               EndPoint = new CGPoint(0.5, 0.7)
            };

            gradientLayer.Frame = new CGRect(0, 0, DemoUtilities.DisplayWidth, e.NewElement.HeightRequest);

            gradientLayer.Colors = new CGColor[]
            {
               new Color(255, 255, 255, 0).ToCGColor(), new Color(255, 255, 255, 255).ToCGColor()
            };
            
            NativeView.Layer.InsertSublayer(gradientLayer, 0);
         }
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom Switch renderer to change its color and look                                                   */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomSwitchRenderer : SwitchRenderer
   {
      protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
      {
         base.OnElementChanged(e);

         UpdateSwitchStyle();
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         if (e.PropertyName == Switch.IsToggledProperty.PropertyName || e.PropertyName == Switch.IsEnabledProperty.PropertyName)
         {
            UpdateSwitchStyle();
         }
      }

      private void UpdateSwitchStyle()
      {
         if (Element == null) return;

         // Setting UISwitch.ThumbTintColor seems to stop the sliding animation of the thumb control, so I am adding some transitioning animation to smooth it a little.
         var transition = new CATransition();
         transition.Type = (Element.IsToggled) ? CATransition.TransitionFromLeft : CATransition.TransitionFromRight;
         transition.Duration = 0.5;
         Control.Layer.AddAnimation(transition, null);

         Control.TintColor = (Element.IsEnabled) ? CustomColors.BlueButtonColor.ToUIColor() : Color.LightGray.ToUIColor();
         Control.OnTintColor = (Element.IsEnabled) ? CustomColors.BlueButtonColor.ToUIColor() : Color.LightGray.ToUIColor();
         Control.ThumbTintColor = (Element.IsToggled) ? UIColor.White : (Element.IsEnabled) ? CustomColors.BlueButtonColor.ToUIColor() : Color.LightGray.ToUIColor();
      }
   }
}
