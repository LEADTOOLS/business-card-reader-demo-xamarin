// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class RoundImageButton : Frame
   {
      public RoundImageButton()
      {
         // Setup defaults
         Margin = new Thickness(0);
         Padding = new Thickness(0);
         BackgroundColor = Color.Black;
         HasShadow = false;
         TextColor = Color.White;

         // Initialize
         InitializeComponent();
         UpdateImage();
         UpdateSizes();
      }

      #region Bindable properties

      #region ColumnSpacing

      public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create(
         propertyName: nameof(ColumnSpacing),
         returnType: typeof(double),
         declaringType: typeof(RoundImageButton),
         defaultValue: 0.0,
         defaultBindingMode: BindingMode.TwoWay
      );
      public double ColumnSpacing
      {
         get => GetValue(ColumnSpacingProperty) is double value ? value : 0.0;
         private set => SetValue(ColumnSpacingProperty, value);
      }

      #endregion

      #region FontAttributes

      public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
         propertyName: nameof(FontAttributes),
         returnType: typeof(FontAttributes),
         declaringType: typeof(RoundImageButton),
         defaultValue: null,
         defaultBindingMode: BindingMode.TwoWay
      );
      public FontAttributes FontAttributes
      {
         get => GetValue(FontAttributesProperty) is FontAttributes value ? value : FontAttributes.None;
         set => SetValue(FontAttributesProperty, value);
      }

      #endregion

      #region FontSize

      public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
         propertyName: nameof(FontSize),
         returnType: typeof(double),
         declaringType: typeof(RoundImageButton),
         defaultValue: 0.0,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) => (bindable as RoundImageButton)?.UpdateSizes()
      );
      public double FontSize
      {
         get => GetValue(FontSizeProperty) is double value ? value : 0.0;
         set => SetValue(FontSizeProperty, value);
      }

      #endregion

      #region ImageSize

      public static readonly BindableProperty ImageSizeProperty = BindableProperty.Create(
         propertyName: nameof(ImageSize),
         returnType: typeof(double),
         declaringType: typeof(RoundImageButton),
         defaultValue: 0.0,
         defaultBindingMode: BindingMode.TwoWay
      );
      public double ImageSize
      {
         get => GetValue(ImageSizeProperty) is double value ? value : 0.0;
         private set => SetValue(ImageSizeProperty, value);
      }

      #endregion

      #region ResourceName

      public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
         propertyName: nameof(ResourceName),
         returnType: typeof(string),
         declaringType: typeof(RoundImageButton),
         defaultValue: null,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) => (bindable as RoundImageButton)?.UpdateImage()
      );
      public string ResourceName
      {
         get => GetValue(ResourceNameProperty) is string value ? value : null;
         set => SetValue(ResourceNameProperty, value);
      }

      #endregion

      #region Text

      public static readonly BindableProperty TextProperty = BindableProperty.Create(
         propertyName: nameof(Text),
         returnType: typeof(string),
         declaringType: typeof(RoundImageButton),
         defaultValue: null,
         defaultBindingMode: BindingMode.TwoWay
      );
      public string Text
      {
         get => GetValue(TextProperty) is string value ? value : null;
         set => SetValue(TextProperty, value);
      }

      #endregion

      #region TextColor

      public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
         propertyName: nameof(TextColor),
         returnType: typeof(Color),
         declaringType: typeof(RoundImageButton),
         defaultValue: Label.TextColorProperty.DefaultValue,
         defaultBindingMode: BindingMode.TwoWay
      );
      public Color TextColor
      {
         get => GetValue(TextColorProperty) is Color value ? value : Color.Transparent;
         set => SetValue(TextColorProperty, value);
      }

      #endregion

      #endregion

      #region Internal properties
      
      private bool ShowingImage { get; set; } = true;

      #endregion

      #region Methods

      #region Helpers

      private void UpdateImage()
      {
         if (string.IsNullOrEmpty(ResourceName))
         {
            if (ShowingImage)
            {
               // Remove the image
               TheGrid.Children.Remove(TheImage);
               TheGrid.ColumnDefinitions.RemoveAt(1);
               Grid.SetColumn(TheLabel, 1);
               ShowingImage = false;
            }
         }
         else if (!ShowingImage)
         {
            // Add the image back
            TheGrid.ColumnDefinitions.Insert(1, new ColumnDefinition()
            {
               Width = new GridLength(1.0, GridUnitType.Auto)
            });
            Grid.SetColumn(TheLabel, 2);
            TheGrid.Children.Add(TheImage, 1, 0);
            ShowingImage = true;
         }
         TheImage.ResourceName = ResourceName;
      }

      private void UpdateSizes()
      {
         ColumnSpacing = FontSize * 0.8;
         ImageSize = FontSize * 1.25;
      }

      #endregion

      #endregion
   }
}
