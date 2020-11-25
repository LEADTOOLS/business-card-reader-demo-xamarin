// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.UI.Page;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.Document.UI.Pages.Ocr
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OcrFormatsPage : CustomPage
   {
      #region Config

      private static Color Text_SelectedColor { get; } = Color.FromRgb(255, 87, 130);
      private static Color Text_UnselectedColor { get; } = Color.FromRgb(87, 104, 127);

      #endregion

      #region Events

      public event EventHandler<SelectedFormatChangedEventArgs> SelectedFormatChanged;

      #endregion // Events

      private bool _closePageAfterSelectingOutputFormat = false;
      public OcrFormatsPage(OcrOutputFormat ocrOutputFormat, bool fullScreenMode = true, bool closePageAfterSelectingOutputFormat = false, bool showFormatsListOnly = false, double topMargin = -1, int cornersRadius = 0)
         : base(!fullScreenMode, LayoutAlignment.Start, showFormatsListOnly, topMargin, cornersRadius)
      {
         InitializeComponent();

         _closePageAfterSelectingOutputFormat = closePageAfterSelectingOutputFormat;

         // Setup the list view
         Formats = ((OcrOutputFormat[])Enum.GetValues(typeof(OcrOutputFormat))).Where(format => format != OcrOutputFormat.None).Select(format => new OutputFormatObject(format)).ToArray();
         FormatListView.ItemsSource = Formats;

         // Rrestore selection
         for (int i = 0; i < Formats.Length; i++)
         {
            if (Formats[i].OcrOutputFormat == ocrOutputFormat)
            {
               // Select
               _SelectedIndex = i;

               // Ensure visible
               if (Device.RuntimePlatform != Device.UWP)
               {
                  // For some reason this line crashes UWP in Release mode only so don't execute it for UWP
                  FormatListView.ScrollTo(Formats[i], ScrollToPosition.MakeVisible, false);
               }
               break;
            }
         }

         // Default to first item
         if (_SelectedIndex < 0)
            _SelectedIndex = 0;

         Formats[_SelectedIndex].IsSelected = true;
      }

      #region Internal properties

      private OutputFormatObject[] Formats { get; }
      private int _SelectedIndex = -1;
      private int SelectedIndex
      {
         get => _SelectedIndex;
         set
         {
            try
            {
               // Unselect previous
               if (_SelectedIndex >= 0)
                  Formats[_SelectedIndex].IsSelected = false;
               // Select new
               _SelectedIndex = value;
               if (_SelectedIndex >= 0)
               {
                  Formats[_SelectedIndex].IsSelected = true;
                  SelectedFormatChanged?.Invoke(this, new SelectedFormatChangedEventArgs(Formats[_SelectedIndex].OcrOutputFormat));
               }
            }
            finally
            {
               if (_closePageAfterSelectingOutputFormat)
                  ClosePage();
            }
         }
      }

      public OcrOutputFormat SelectedFormat { get => Formats[_SelectedIndex].OcrOutputFormat; }

      #endregion

      #region Methods

      #region Events

      private void FormatListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         // Update selection
         SelectedIndex = e.ItemIndex;
      }

      #endregion

      private async void ClosePage()
      {
         base.OnBackButtonPressed();
         await PopupNavigation.Instance.PopAsync();
      }

      protected override bool OnBackButtonPressed()
      {
         ClosePage();
         return true;
      }
      #endregion

      #region OutputFormatObject

      private class OutputFormatObject : BindableObject
      {
         public OutputFormatObject(OcrOutputFormat format)
         {
            OcrOutputFormat = format;
            Text = OcrOutputFormat.GetDescription();
         }

         #region Bindable properties

         #region FontAttributes

         public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
            propertyName: nameof(FontAttributes),
            returnType: typeof(FontAttributes),
            declaringType: typeof(OutputFormatObject),
            defaultValue: FontAttributes.None,
            defaultBindingMode: BindingMode.OneWay
         );
         public FontAttributes FontAttributes
         {
            get => GetValue(FontAttributesProperty) is FontAttributes value ? value : FontAttributes.None;
            set => SetValue(FontAttributesProperty, value);
         }

         #endregion

         #region IsSelected

         public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            propertyName: nameof(IsSelected),
            returnType: typeof(bool),
            declaringType: typeof(OutputFormatObject),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindable, oldObject, newObject) =>
            {
               if (bindable is OutputFormatObject self)
                  // Update the text
                  if (self.IsSelected)
                  {
                     self.FontAttributes = FontAttributes.Bold;
                     self.TextColor = Text_SelectedColor;
                  }
                  else
                  {
                     self.FontAttributes = FontAttributes.None;
                     self.TextColor = Text_UnselectedColor;
                  }
            }
         );
         public bool IsSelected
         {
            get => GetValue(IsSelectedProperty) is bool value ? value : false;
            set => SetValue(IsSelectedProperty, value);
         }

         #endregion

         #region TextColor

         public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: nameof(TextColor),
            returnType: typeof(Color),
            declaringType: typeof(OutputFormatObject),
            defaultValue: Text_UnselectedColor,
            defaultBindingMode: BindingMode.OneWay
         );
         public Color TextColor
         {
            get => GetValue(TextColorProperty) is Color value ? value : Color.Transparent;
            set => SetValue(TextColorProperty, value);
         }

         #endregion

         #endregion

         #region Public properties

         public OcrOutputFormat OcrOutputFormat { get; }
         public string Text { get; }

         #endregion
      }

      #endregion
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class SelectedFormatChangedEventArgs : EventArgs
   {
      public SelectedFormatChangedEventArgs(OcrOutputFormat ocrOutputFormat)
      {
         OcrOutputFormat = ocrOutputFormat;
      }

      public OcrOutputFormat OcrOutputFormat { get; private set; }
   }
}