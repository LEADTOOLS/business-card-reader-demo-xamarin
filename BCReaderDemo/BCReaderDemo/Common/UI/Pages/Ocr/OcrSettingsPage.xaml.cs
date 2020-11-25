// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.UI.Page;
using Leadtools.Demos.UI.Pages.Info;
using Leadtools.Ocr;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.Document.UI.Pages.Ocr
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OcrSettingsPage : CustomPage
   {
      private IOcrEngine _ocrEngine = null;
      public OcrSettingsPage(IOcrEngine ocrEngine)
      {
         _ocrEngine = ocrEngine;
         BindingContext = this;
         InitializeComponent();
         AfterInitializeComponent();

         // Handle converter options
         LanguagesLabel.Text = ReadableActiveLanguages(ActiveLanguages);
         OutputFormatLabel.Text = OcrOutputFormat.GetFormatName();
         TitleExtraButtonTapped += AboutButton_TitleExtraButtonTapped;
      }

      #region Public properties

      private ObservableCollection<OcrLanguage> _activeLanguages = new ObservableCollection<OcrLanguage>();
      public ObservableCollection<OcrLanguage> ActiveLanguages
      {
         get
         {
            return _activeLanguages;
         }
         set
         {
            _activeLanguages = value;
            LanguagesLabel.Text = ReadableActiveLanguages(_activeLanguages);
         }
      }

      private OcrOutputFormat _ocrOutputFormat = OcrOutputFormat.Pdf;
      public OcrOutputFormat OcrOutputFormat
      {
         get
         {
            return _ocrOutputFormat;
         }
         set
         {
            _ocrOutputFormat = value;
            OutputFormatLabel.Text = _ocrOutputFormat.GetFormatName();
         }
      }

      #endregion // Public properties

      #region Bindable properties

      #region DetectGraphicsAndColors

      public static readonly BindableProperty DetectGraphicsAndColorsProperty = BindableProperty.Create(
         propertyName: nameof(DetectGraphicsAndColors),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool DetectGraphicsAndColors
      {
         get => GetValue(DetectGraphicsAndColorsProperty) is bool value ? value : true;
         set => SetValue(DetectGraphicsAndColorsProperty, value);
      }

      #endregion // DetectGraphicsAndColors

      #region DetectInvertedRegions

      public static readonly BindableProperty DetectInvertedRegionsProperty = BindableProperty.Create(
         propertyName: nameof(DetectInvertedRegions),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool DetectInvertedRegions
      {
         get => GetValue(DetectInvertedRegionsProperty) is bool value ? value : false;
         set => SetValue(DetectInvertedRegionsProperty, value);
      }

      #endregion // DetectInvertedRegions

      #region DetectTables

      public static readonly BindableProperty DetectTablesProperty = BindableProperty.Create(
         propertyName: nameof(DetectTables),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool DetectTables
      {
         get => GetValue(DetectTablesProperty) is bool value ? value : false;
         set => SetValue(DetectTablesProperty, value);
      }

      #endregion // DetectTables

      #region IntelligentSelectArea

      public static readonly BindableProperty IntelligentSelectAreaProperty = BindableProperty.Create(
         propertyName: nameof(IntelligentSelectArea),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool IntelligentSelectArea
      {
         get => GetValue(IntelligentSelectAreaProperty) is bool value ? value : true;
         set => SetValue(IntelligentSelectAreaProperty, value);
      }

      public static readonly BindableProperty ShowIntelligentSelectAreaSettingProperty = BindableProperty.Create(
         propertyName: nameof(ShowIntelligentSelectAreaSetting),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool ShowIntelligentSelectAreaSetting
      {
         get => GetValue(ShowIntelligentSelectAreaSettingProperty) is bool value ? value : true;
         set => SetValue(ShowIntelligentSelectAreaSettingProperty, value);
      }

      #endregion // IntelligentSelectArea

      #region AutoInvertImages

      public static readonly BindableProperty AutoInvertImagesProperty = BindableProperty.Create(
         propertyName: nameof(AutoInvertImages),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool AutoInvertImages
      {
         get => GetValue(AutoInvertImagesProperty) is bool value ? value : false;
         set => SetValue(AutoInvertImagesProperty, value);
      }

      #endregion // AutoInvertImages

      #region AutoRotateImages

      public static readonly BindableProperty AutoRotateImagesProperty = BindableProperty.Create(
         propertyName: nameof(AutoRotateImages),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool AutoRotateImages
      {
         get => GetValue(AutoRotateImagesProperty) is bool value ? value : true;
         set => SetValue(AutoRotateImagesProperty, value);
      }

      #endregion // AutoRotateImages

      #region DiscardNoisyZones

      public static readonly BindableProperty DiscardNoisyZonesProperty = BindableProperty.Create(
         propertyName: nameof(DiscardNoisyZones),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool DiscardNoisyZones
      {
         get => GetValue(DiscardNoisyZonesProperty) is bool value ? value : true;
         set => SetValue(DiscardNoisyZonesProperty, value);
      }

      #endregion // DiscardNoisyZones

      #region Enable3DDeskewAfterCapture

      public static readonly BindableProperty Enable3DDeskewAfterCaptureProperty = BindableProperty.Create(
         propertyName: nameof(Enable3DDeskewAfterCapture),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool Enable3DDeskewAfterCapture
      {
         get => GetValue(Enable3DDeskewAfterCaptureProperty) is bool value ? value : true;
         set => SetValue(Enable3DDeskewAfterCaptureProperty, value);
      }

      public static readonly BindableProperty Show3DDeskewSettingProperty = BindableProperty.Create(
         propertyName: nameof(Show3DDeskewSetting),
         returnType: typeof(bool),
         declaringType: typeof(OcrSettingsPage),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool Show3DDeskewSetting
      {
         get => GetValue(Show3DDeskewSettingProperty) is bool value ? value : true;
         set => SetValue(Show3DDeskewSettingProperty, value);
      }

      #endregion // DiscardNoisyZones

      #endregion // Bindable properties

      #region Methods

      private string ReadableActiveLanguages(ObservableCollection<OcrLanguage> languages)
      {
         // No elements?
         var langs = languages?.Select(l => new CultureInfo(l.Language).DisplayName)?.ToList();
         if (langs == null || langs.Count == 0)
            return "N/A";

         // Only one: Just return
         if (langs.Count == 1)
            return langs[0];

         // Multiple: Language name (+Count-1)
         return $"{langs[0]} (+{langs.Count - 1})";
      }

      #endregion

      #region Events

      private async void AboutButton_TitleExtraButtonTapped(object sender, EventArgs e)
      {
         InfoPage infoPage = new InfoPage();
         if (infoPage.Resources.ContainsKey("BackButtonResourceName"))
            infoPage.Resources["BackButtonResourceName"] = "back.svg";

         await PopupNavigation.Instance.PushAsync(infoPage);
      }

      private async void Languages_Tapped(object sender, EventArgs e)
      {
         // Prevent double-tapping
         LanguagesLabelContainer.IsEnabled = false;

         OcrLanguagesPage ocrLanguagesPage = new OcrLanguagesPage(ActiveLanguages);
         ocrLanguagesPage.PageClosing += OcrLanguagesPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(ocrLanguagesPage);

         LanguagesLabelContainer.IsEnabled = true;
      }

      private void OcrLanguagesPage_PageClosing(object sender, EventArgs e)
      {
         OcrLanguagesPage ocrLanguagesPage = sender as OcrLanguagesPage;
         ActiveLanguages = ocrLanguagesPage.ActiveLanguages;
         LanguagesLabel.Text = ReadableActiveLanguages(ActiveLanguages);
      }

      private async void OutputFormatLabel_Tapped(object sender, EventArgs e)
      {
         // Prevent double-tapping
         OutputFormatLabelContainer.IsEnabled = false;

         OcrFormatsPage ocrFormatsPage = new OcrFormatsPage(OcrOutputFormat);
         ocrFormatsPage.SelectedFormatChanged += OcrFormatsPage_SelectedFormatChanged;
         await PopupNavigation.Instance.PushAsync(ocrFormatsPage);

         OutputFormatLabelContainer.IsEnabled = true;
      }

      private void OcrFormatsPage_SelectedFormatChanged(object sender, SelectedFormatChangedEventArgs e)
      {
         OcrOutputFormat = e.OcrOutputFormat;
         OutputFormatLabel.Text = OcrOutputFormat.GetFormatName();
      }

      private void Grid_Tapped(object sender, EventArgs e)
      {
         if (sender is Xamarin.Forms.Grid grid && grid.Children[1] is Leadtools.Demos.UI.Elements.CustomSwitch customSwitch)
            customSwitch.IsToggled = !customSwitch.IsToggled;
      }

      #endregion
   }
}