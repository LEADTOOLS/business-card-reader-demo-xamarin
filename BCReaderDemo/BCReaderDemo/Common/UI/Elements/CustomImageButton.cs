// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.IO;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public abstract class CustomImageButton : Grid
   {
      public CustomImageButton(Color text_SelectedColor, Color text_UnselectedColor)
      {
         // Save the color options
         Text_SelectedColor = text_SelectedColor;
         Text_UnselectedColor = text_UnselectedColor;

         // Create the controls
         InitializeComponent();

         // Update the text color
         if (Label != null)
            Label.TextColor = IsSelected ? Text_SelectedColor : Text_UnselectedColor;

         // Custom tap event, instead of gestures
         TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Tapped += (sender, e) => Tapped?.Invoke(sender, e);
         GestureRecognizers.Add(tapGestureRecognizer);
      }

      #region Bindable properties

      #region Image

      #region Selected

      public static readonly BindableProperty Image_SelectedProperty = BindableProperty.Create(
         propertyName: nameof(Image_Selected),
         returnType: typeof(string),
         declaringType: typeof(CustomImageButton),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomImageButton self && self.SvgImage != null)
               if (self.IsSelected)
                  self.SvgImage.ResourceName = self.Image_Selected;
               else
                  self.SvgImage.ResourceName = self.GetUnselectedImage();
         }
      );
      public string Image_Selected { get => GetValue(Image_SelectedProperty) is string value ? value : string.Empty; set => SetValue(Image_SelectedProperty, value); }

      #endregion

      #region Unselected

      public static readonly BindableProperty Image_UnselectedProperty = BindableProperty.Create(
         propertyName: nameof(Image_Unselected),
         returnType: typeof(string),
         declaringType: typeof(CustomImageButton),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomImageButton self && !self.IsSelected && self.SvgImage != null)
               self.SvgImage.ResourceName = self.GetUnselectedImage();
         }
      );
      public string Image_Unselected { get => GetValue(Image_UnselectedProperty) is string value ? value : string.Empty; set => SetValue(Image_UnselectedProperty, value); }

      #endregion

      #endregion

      #region IsSelected

      public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
         propertyName: nameof(IsSelected),
         returnType: typeof(bool),
         declaringType: typeof(CustomImageButton),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomImageButton self)
               // Update other properties
               if (self.IsSelected)
               {
                  if (self.Label != null)
                     self.Label.TextColor = self.Text_SelectedColor;
                  if (self.SvgImage != null)
                     self.SvgImage.ResourceName = self.Image_Selected;
               }
               else
               {
                  if (self.Label != null)
                     self.Label.TextColor = self.Text_UnselectedColor;
                  if (self.SvgImage != null)
                     self.SvgImage.ResourceName = self.GetUnselectedImage();
               }
         }
      );
      public bool IsSelected { get => GetValue(IsSelectedProperty) is bool value ? value : false; set => SetValue(IsSelectedProperty, value); }

      #endregion

      #region Text

      public static readonly BindableProperty TextProperty = BindableProperty.Create(
         propertyName: nameof(Text),
         returnType: typeof(string),
         declaringType: typeof(CustomImageButton),
         defaultValue: string.Empty,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomImageButton self && self.Label != null)
               // Update label
               self.Label.Text = self.Text;
         }
      );
      public string Text { get => GetValue(TextProperty) is string value ? value : string.Empty; set => SetValue(TextProperty, value); }

      #endregion

      #endregion

      #region Events

      public event EventHandler Tapped;

      #endregion

      #region Internal properties

      protected Label Label { get; set; }
      protected SvgImage SvgImage { get; set; }
      private Color Text_SelectedColor { get; }
      private Color Text_UnselectedColor { get; }

      #endregion

      #region Methods

      private string GetUnselectedImage()
      {
         // Use existing value?
         if (string.IsNullOrWhiteSpace(Image_Selected) || !string.IsNullOrWhiteSpace(Image_Unselected))
            return Image_Unselected;

         // Add -disabled before extension of Image_Selected
         return $"{Path.GetFileNameWithoutExtension(Image_Selected)}-disabled{Path.GetExtension(Image_Selected)}";
      }

      protected abstract void InitializeComponent();

      #endregion
   }
}
