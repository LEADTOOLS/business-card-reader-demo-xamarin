// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using BCReaderDemo.Utils;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ViewCell), typeof(ListViewCellRenderer))]
[assembly: ExportRenderer(typeof(GradientContentView), typeof(GradientContentViewRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(CustomListViewRenderer))]
[assembly: ExportRenderer(typeof(UnEvenTableViewModelRenderer), typeof(CustomTableViewModelRenderer))]
[assembly: ExportRenderer(typeof(CustomTableView), typeof(CustomTableViewRenderer))]
[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
[assembly: ExportRenderer(typeof(CustomSearchBar), typeof(CustomSearchBarRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(CustomSwitchRenderer))]

[assembly: ResolutionGroupName("BCReaderDemo")]
[assembly: ExportEffect(typeof(CustomRoundCornersEffect), nameof(RoundCornersEffect))]

namespace BCReaderDemo.iOS
{
   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom cell view renderer to remove the background of the cell when selected                          */
   /* (no background color when list view item selected).                                                   */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class ListViewCellRenderer : ViewCellRenderer
   {
      public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
      {
         UITableViewCell cell = null;
         try
         {
            cell = base.GetCell(item, reusableCell, tv);
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
            cell = base.GetCell(item, null, tv);
         }

         if (cell != null)
         {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.BackgroundColor = UIColor.Clear;
         }

         tv.SeparatorStyle = UITableViewCellSeparatorStyle.None;

         return cell;
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for ListView control to hide the extra empty lines and remove the left margin of      */
   /* the cells separator line and support item's LongPress event.                                          */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomListViewRenderer : ListViewRenderer
   {
      protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
      {
         base.OnElementChanged(e);

         if (this.Control == null) return;

         this.Control.SeparatorInset = new UIEdgeInsets(this.Control.SeparatorInset.Top, 0, this.Control.SeparatorInset.Bottom, this.Control.SeparatorInset.Right);
         this.Control.TableFooterView = new UIView();

         if (Element is ListViewWithLongPressGesture)
         {
            UIGestureRecognizer[] gestureRecognizers = new UIGestureRecognizer[Control.GestureRecognizers.Length + 1];
            Control.GestureRecognizers.CopyTo(gestureRecognizers, 0);
            gestureRecognizers[Control.GestureRecognizers.Length] = new UILongPressGestureRecognizer(LongPressedAction);
            Control.GestureRecognizers = gestureRecognizers;
         }
      }

      public override UIView HitTest(CGPoint point, UIEvent uievent)
      {
         UIView hitTestView = base.HitTest(point, uievent);

         if (hitTestView is UITableView)
         {
            ListViewWithLongPressGesture element = Element as ListViewWithLongPressGesture;
            element?.OnClicked();
         }

         return hitTestView;
      }

      void LongPressedAction(UILongPressGestureRecognizer gestureRecognizer)
      {
         if (gestureRecognizer.State == UIGestureRecognizerState.Began)
         {
            ListViewWithLongPressGesture element = Element as ListViewWithLongPressGesture;
            NSIndexPath indexPath = Control.IndexPathForRowAtPoint(gestureRecognizer.LocationInView(Control));
            if(indexPath != null)
            {
               if (indexPath.Section < HomePage.ContactsGrouped.Count && indexPath.Row < HomePage.ContactsGrouped[indexPath.Section].Count)
                  element.ItemLongPressedAction?.Invoke(HomePage.ContactsGrouped[indexPath.Section][indexPath.Row]);
            }
         }
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom renderer for TableView in order to customize its sections margins and sections text attributes */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomTableViewModelRenderer : UnEvenTableViewModelRenderer
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
   /* Custom effect to show Xamarin controls with cornered edges                                            */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomRoundCornersEffect : PlatformEffect
   {
      protected override void OnAttached()
      {
         try
         {
            PrepareContainer();
            SetCornerRadius();
         }
         catch { }
      }

      protected override void OnDetached()
      {
         try
         {
            Container.Layer.CornerRadius = new nfloat(0);
         }
         catch { }
      }

      protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
      {
         if (args.PropertyName == RoundCornersEffect.CornerRadiusProperty.PropertyName)
            SetCornerRadius();
      }

      private void PrepareContainer()
      {
         Container.ClipsToBounds = true;
         Container.Layer.AllowsEdgeAntialiasing = true;
         Container.Layer.EdgeAntialiasingMask = CAEdgeAntialiasingMask.All;
      }

      private void SetCornerRadius()
      {
         var cornerRadius = RoundCornersEffect.GetCornerRadius(Element);
         Container.Layer.CornerRadius = new nfloat(cornerRadius);
      }
   }


   /*********************************************************************************************************/
   /*                                                                                                       */
   /* Custom SearchBar renderer to create round rectangular search bar with custom colors and also change   */
   /* its colors on focus/unfocus events.                                                                   */
   /*                                                                                                       */
   /*********************************************************************************************************/
   public class CustomSearchBarRenderer : SearchBarRenderer
   {
      protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
      {
         base.OnElementChanged(e);

         var searchbar = (UISearchBar)Control;
         if (e.NewElement != null)
         {
            bool isEntryStyle = false;
            if (!string.IsNullOrEmpty(e.NewElement.StyleId) && e.NewElement.StyleId.Equals("EntryStyle"))
               isEntryStyle = true;

            searchbar.BackgroundImage = new UIImage();

            NSString searchFieldName = new NSString("searchField");
            UITextField textFieldInsideSearchBar = (UITextField)searchbar.ValueForKey(searchFieldName);

            // I have customized the search bar to use it as Entry control as our design requires Entry field that looks exactly like 
            // the search bar without the magnifying glass and with different border and text colors, and we differentiate them by checking
            // for that "StyleId" member if it equals "EntryStyle" then tweak this SearchBar control a bit, this flag you have to set it 
            // yourself inside the page that uses the SearchBar.
            UIColor placeholderTextColor;
            if (isEntryStyle)
            {
               textFieldInsideSearchBar.LeftViewMode = UITextFieldViewMode.Never;
               textFieldInsideSearchBar.ReturnKeyType = UIReturnKeyType.Done;
               searchbar.Layer.BorderColor = CustomColors.LightBlueColor.ToCGColor();
               placeholderTextColor = CustomColors.SearchBarPlaceHolderInactiveLightBlueTextColor.ToUIColor();
            }
            else
            {
               UIImage searchIcon = UIImage.FromBundle("search_icon.png");
               if (searchIcon != null)
                  searchbar.SetImageforSearchBarIcon(searchIcon, UISearchBarIcon.Search, UIControlState.Normal);

               searchbar.Layer.BorderColor = CustomColors.LightSilverColor.ToCGColor();
               placeholderTextColor = CustomColors.SearchBarPlaceHolderInactiveTextColor.ToUIColor();
            }

            searchbar.ShowsCancelButton = false;
            searchbar.Layer.CornerRadius = 15;
            searchbar.Layer.BorderWidth = 2;
            searchbar.Layer.BackgroundColor = Color.Transparent.ToCGColor();

            textFieldInsideSearchBar.KeyboardAppearance = UIKeyboardAppearance.Alert;
            textFieldInsideSearchBar.AttributedPlaceholder = new NSAttributedString(textFieldInsideSearchBar.Placeholder, null, placeholderTextColor);
            textFieldInsideSearchBar.BackgroundColor = Color.Transparent.ToUIColor();
            textFieldInsideSearchBar.TextColor = CustomColors.SearchBarPlaceHolderActiveTextColor.ToUIColor();
            textFieldInsideSearchBar.Font = UIFont.SystemFontOfSize(UIFont.SmallSystemFontSize);
            textFieldInsideSearchBar.EditingDidBegin += TextFieldInsideSearchBar_EditingDidBegin;
            textFieldInsideSearchBar.EditingDidEnd += TextFieldInsideSearchBar_EditingDidEnd;

            AddDoneButton();
         }
      }

      public override void TouchesBegan(NSSet touches, UIEvent evt)
      {
         base.TouchesBegan(touches, evt);

         (Element as CustomSearchBar).OnClicked();
      }

      protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         // Below is workaround to hide the "Cancel" button shown inside the SearchBar since setting the "ShowCancelButton" to false does not work.
         if (e.PropertyName != SearchBar.CancelButtonColorProperty.PropertyName && e.PropertyName != SearchBar.TextProperty.PropertyName)
            base.OnElementPropertyChanged(sender, e);
      }

      private void TextFieldInsideSearchBar_EditingDidBegin(object sender, EventArgs e)
      {
         var searchbar = (UISearchBar)Control;
         searchbar.BackgroundColor = CustomColors.LightSilverColor.ToUIColor();
         searchbar.Layer.BackgroundColor = CustomColors.LightSilverColor.ToCGColor();

         (Element as CustomSearchBar).OnClicked();
      }

      private void TextFieldInsideSearchBar_EditingDidEnd(object sender, EventArgs e)
      {
         var searchbar = (UISearchBar)Control;
         searchbar.BackgroundColor = Color.Transparent.ToUIColor();
         searchbar.Layer.BackgroundColor = Color.Transparent.ToCGColor();
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

            gradientLayer.Frame = new CGRect(0, 0, App.DisplayScreenWidth, e.NewElement.HeightRequest);

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
