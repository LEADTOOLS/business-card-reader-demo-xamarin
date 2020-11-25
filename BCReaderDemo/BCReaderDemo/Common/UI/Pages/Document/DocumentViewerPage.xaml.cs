// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Xamarin;
using Leadtools.Controls;
using Leadtools.Demos.Document.UI.Pages.Ocr;
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.UI.Page;
using Leadtools.Demos.Utils;
using Leadtools.Document;
using Leadtools.Document.Converter;
using Leadtools.Document.Viewer;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.UI.Pages.Document
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public partial class DocumentViewerPage : PopupPage
   {
      private DocumentViewer _documentViewer = null;
      private DocumentConverterHelper _documentConverterHelper = null;
      private ObservableCollection<DocumentItemData> _documents = null;
      private CancellationTokenSource _cts = null;
      private bool _isSpeaking = false;
      private bool _documentAlreadyConverted = true;
      private bool _shareAfterConversion = false;
      private string _pageLanguage = null;
      private string _applicationDirectory = string.Empty;
      private string _documentsDirectory = string.Empty;
      private int _documentIndexInList = -1;
      private string _activeInteractiveMode = DocumentViewerCommands.InteractivePanZoom;

      private const double Popup_ShadowOpacity = 0.5;
      private const uint Popup_AnimationDuration = 150;

      public LEADDocument Document { get; private set; }
      public event EventHandler PageClosing;

      public OcrOutputFormat SourceDocumentFormat { get; private set; }

      public OcrOutputFormat OutputDocumentFormat { get; private set; }

      public bool AllowConversionInsideDocumentViewerPage { get; private set; }

      private bool _annObjectsModified = false;
      private bool IsDirty
      {
         get { return _annObjectsModified || (_documentViewer.Annotations != null && _documentViewer.Annotations.IsContainerModified(0)); }
      }

      public DocumentViewerPage(string appDirectory,
                                string documentsDirectory,
                                LEADDocument document,
                                ObservableCollection<DocumentItemData> documents,
                                OcrOutputFormat sourceDocumentFormat,
                                OcrOutputFormat outputDocumentFormat,
                                int documentIndexInList,
                                DocumentConverterHelper documentConverterHelper,
                                DocumentScanType scanType,
                                string pageTitle,
                                string pageLanguage,
                                bool allowConversionInsideDocumentViewerPage)
      {
         MessagingCenter.Subscribe<object, string>(this, "OnSleep", (sender, parameter) =>
         {
            CancelSpeech();
         });

         _documents = documents;
         _documentIndexInList = documentIndexInList;
         _pageLanguage = !string.IsNullOrWhiteSpace(pageLanguage) ? pageLanguage : "en";

         _applicationDirectory = appDirectory;
         _documentsDirectory = documentsDirectory;
         _documentConverterHelper = documentConverterHelper;
         SourceDocumentFormat = sourceDocumentFormat;
         OutputDocumentFormat = outputDocumentFormat;
         AllowConversionInsideDocumentViewerPage = allowConversionInsideDocumentViewerPage;

         _cts = new CancellationTokenSource();

         InitializeComponent();

         if (Device.RuntimePlatform == Device.iOS)
            HasSystemPadding = false;

         // Set thumbnails and annotations layouts heights and margins
         double bottomMargin = DemoUtilities.Instance.SafeAreaBottom;
         double thumbnailsLayoutHeight = DemoUtilities.DisplayHeight * 0.7 - bottomMargin;
         double annotationsLayoutHeight = DemoUtilities.DisplayHeight * 0.5 - bottomMargin;

         ThumbnailsLayout.Margin = new Thickness(0, 0, 0, bottomMargin);
         ThumbnailsLayout.HeightRequest = thumbnailsLayoutHeight;

         AnnotationsLayout.Margin = new Thickness(0, 0, 0, bottomMargin);
         AnnotationsLayout.HeightRequest = annotationsLayoutHeight;

         InitDocumentViewer(scanType);
         SetDocument(document);
         SetupUI(scanType, pageTitle);
      }

      protected override async void OnAppearing()
      {
         base.OnAppearing();

         // Delay so the ad doesn't appear immediately
         await Task.Delay(1000);

         // Start ads
         Ads.Start();
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         // Stop ads
         Ads.Stop();
      }

      protected override bool OnBackButtonPressed()
      {
         // check if we have any controls layout visible like annotations layout or thumbnails layout 
         // then don't exit page and hide the shown layout instead.
         if (PopupShadow.IsVisible)
            PopupShadow_Tapped(null, null);
         else
         {
            if (PageBackButton.IsEnabled)
               CheckDiscardChanges();
         }

         return true;
      }

      private void BackButton_Tapped(object sender, EventArgs e)
      {
         CheckDiscardChanges();
      }

      private async void OnPageClosing()
      {
         try
         {
            // Delete last saved temp file when user taps on "Share" button (this will contain the zip file path if the shared document 
            // format is SVG, since it doesn't support multipage and the document will be virtual document that consists of multiple documents).
            if (!string.IsNullOrWhiteSpace(_tempFileToDeleteAfterShare) && File.Exists(_tempFileToDeleteAfterShare))
               File.Delete(_tempFileToDeleteAfterShare);
         }
         catch (Exception) { }

         _documentViewer.View.ImageViewer.ScrollOffsetChanged -= ImageViewer_ScrollOffsetChanged;

         // Prevent double-tapping
         PageBackButton.IsEnabled = false;

         // Cancel speeck (if playing)
         CancelSpeech();

         PageClosing?.Invoke(this, null);

         // Go back
         await PopupNavigation.Instance.PopAsync();
      }

      private void CheckDiscardChanges()
      {
         if (IsDirty)
         {
            Device.BeginInvokeOnMainThread(async () =>
            {
               bool accept = await this.DisplayAlert("Discard Changes", "Changes will be lost. Continue?", "Yes", "No");
               if (accept)
               {
                  OnPageClosing();
               }
            });
         }
         else
         {
            OnPageClosing();
         }
      }

      private void AnnotationsActionButton_Tapped(object sender, EventArgs e)
      {
         var automation = _documentViewer.Annotations.Automation;
         if (automation == null) return;

         ContentView eventSource = sender as ContentView;

         if (eventSource.StyleId.Equals("AnnotationsDeleteImage"))
         {
            if (automation.CanDeleteObjects)
               automation.DeleteSelectedObjects();
         }
         else if (eventSource.StyleId.Equals("AnnotationsPropertiesImage"))
         {
            automation.ShowObjectProperties();
         }
         else if (eventSource.StyleId.Equals("AnnotationsUndoImage"))
         {
            if (automation.CanUndo)
               automation.Undo();
         }
         else if (eventSource.StyleId.Equals("AnnotationsRedoImage"))
         {
            if (automation.CanRedo)
               automation.Redo();
         }

         UpdateAnnotationActionButtons();
      }

      private void UpdateAnnotationActionButtons()
      {
         var automation = _documentViewer.Annotations.Automation;
         if (automation == null) return;

         AnnotationsDeleteImage.ResourceName = (automation.CurrentEditObject != null) ? "Annotations/delete-ann.svg" : "Annotations/delete-ann-disabled.svg";
         AnnotationsPropertiesImage.ResourceName = (automation.CanShowProperties) ? "Annotations/properties-ann.svg" : "Annotations/properties-ann-disabled.svg";
         AnnotationsUndoImage.ResourceName = (automation.CanUndo) ? "Annotations/undo.svg" : "Annotations/undo-disabled.svg";
         AnnotationsRedoImage.ResourceName = (automation.CanRedo) ? "Annotations/redo.svg" : "Annotations/redo-disabled.svg";
      }

      /// <summary>
      /// Shows Ann Object Properties Page.
      /// </summary>
      private async void ShowAnnPropertiesPage()
      {
         var automation = _documentViewer.Annotations.Automation;
         if (automation.CanShowProperties)
         {
            // Prevent fast tapping.
            AnnotationsPropertiesButton.IsEnabled = false;

            AnnObject currentObject = automation.CurrentEditObject;

            if (currentObject.Id == AnnObject.GroupObjectId || currentObject.Id == AnnObject.SelectObjectId)
            {
               await DisplayAlert("Error", "Cannot change properties for a group", "OK");
               return;
            }

            if (currentObject.IsLocked)
            {
               await DisplayAlert("Error", "Cannot change properties for a locked object", "OK");
               return;
            }

            AutomationUpdateObjectPage page = new AutomationUpdateObjectPage(automation, currentObject);
            page.Disappearing += AutomationUpdateObjectPage_Disappearing;
            await PopupNavigation.Instance.PushAsync(page);
         }
      }

      private void AutomationUpdateObjectPage_Disappearing(object sender, EventArgs e)
      {
         AnnotationsPropertiesButton.IsEnabled = true;
      }

      private async void ShowThumbnailsButtons_Tapped(object sender, EventArgs e)
      {
         await ShowThumbnailsLayout();
      }

      private async void ShowAnnotationsButtons_Tapped(object sender, EventArgs e)
      {
         await ShowAnnotationsLayout();
      }

      private void ConvertButton_Tapped(object sender, EventArgs e)
      {
         // Save button tapped, then convert the document to the selected user format.
         _shareAfterConversion = false;
         ConvertDocument();
      }

      private void ImageViewer_ScrollOffsetChanged(object sender, EventArgs e)
      {
         UpdatePageNumberLabel();
      }

      // Create the document viewer
      private void InitDocumentViewer(DocumentScanType scanType)
      {
         var createOptions = new DocumentViewerCreateOptions();

         // Set the UI part where the main view is displayed
         createOptions.ViewContainer = DocumentViewerContainer;
         createOptions.ThumbnailsContainer = ThumbnailsLayout;
         createOptions.UseAnnotations = true;

         // Now create the viewer
         _documentViewer = DocumentViewerFactory.CreateDocumentViewer(createOptions);
         // Set the user name
         _documentViewer.UserName = Environment.UserName;
         // We prefer SVG viewing (if supported)
         _documentViewer.View.PreferredItemType = DocumentViewerItemType.Svg;
         // Set thumbnail viewer options
         ImageViewer thumbnailsImageViewer = _documentViewer.Thumbnails.ImageViewer;
         thumbnailsImageViewer.BackgroundColor = Color.Transparent;
         thumbnailsImageViewer.ItemHorizontalAlignment = ControlAlignment.Near;
         thumbnailsImageViewer.ItemVerticalAlignment = ControlAlignment.Near;
         thumbnailsImageViewer.SelectedItemBackgroundColor = Color.FromRgb(80, 131, 242);

         _documentViewer.Operation += _documentViewer_Operation;

         var imageViewer = _documentViewer.View.ImageViewer;
         imageViewer.BackgroundColor = Color.FromHex("#F5F7FA");
#if LEADTOOLS_V21_OR_LATER
         imageViewer.IsFastScrollGadgetEnabled = true;
#endif // #if LEADTOOLS_V21_OR_LATER
         imageViewer.ViewVerticalAlignment = ControlAlignment.Near;
         imageViewer.ViewBorderThickness = 0;
         imageViewer.ItemBorderThickness = 0;
         imageViewer.ImageBorderThickness = 0;
         imageViewer.Zoom(ControlSizeMode.FitWidth, 1, imageViewer.DefaultZoomOrigin);
         imageViewer.InteractiveService.KeyDown += InteractiveService_KeyDown;

         // By default, document viewer has 11 interactive modes added and we don't need them all in our document viewer page.
         // So remove them all exception PanZoom, SelectText and Annotations interactive modes.
         ImageViewerPanZoomInteractiveMode panZoomMode = null;
         var panZoomModes = imageViewer.InteractiveModes.Where(mode => mode.GetType() == typeof(ImageViewerPanZoomInteractiveMode) && (mode as ImageViewerPanZoomInteractiveMode).EnablePan && (mode as ImageViewerPanZoomInteractiveMode).EnablePinchZoom);
         if (panZoomModes != null && panZoomModes.Count() > 0)
            panZoomMode = panZoomModes.ElementAt(0) as ImageViewerPanZoomInteractiveMode;

         DocumentViewerSelectTextInteractiveMode selectTextMode = null;
         var selectTextModes = imageViewer.InteractiveModes.Where(mode => mode.GetType() == typeof(DocumentViewerSelectTextInteractiveMode));
         if (selectTextModes != null && selectTextModes.Count() > 0)
            selectTextMode = selectTextModes.ElementAt(0) as DocumentViewerSelectTextInteractiveMode;

         AnnotationsInteractiveMode annotationsMode = null;
         var annotationsModes = imageViewer.InteractiveModes.Where(mode => mode.GetType() == typeof(AnnotationsInteractiveMode));
         if (annotationsModes != null && annotationsModes.Count() > 0)
            annotationsMode = annotationsModes.ElementAt(0) as AnnotationsInteractiveMode;

         imageViewer.BeginUpdate();
         imageViewer.InteractiveModes.Clear();
         imageViewer.InteractiveModes.Add(annotationsMode);
         imageViewer.InteractiveModes.Add(panZoomMode);
         imageViewer.InteractiveModes.Add(selectTextMode);
         imageViewer.EndUpdate();

         // Helps with debugging of there was a rendering error
         imageViewer.RenderError += (sender, e) =>
         {
            var message = string.Format("Error while rendering item {0} part {1}: {2}",
               e.Item != null ? imageViewer.Items.IndexOf(e.Item) : -1,
               e.Part,
               e.Error.Message);
            System.Diagnostics.Debug.WriteLine(message);
         };

         InitAutomation();

         _documentViewer.Text.AutoGetText = true;
         //_documentViewer.Commands.Run(DocumentViewerCommands.InteractiveAutoPan);

         _documentViewer.View.ImageViewer.ScrollOffsetChanged -= ImageViewer_ScrollOffsetChanged;
         if (scanType == DocumentScanType.Document)
            _documentViewer.View.ImageViewer.ScrollOffsetChanged += ImageViewer_ScrollOffsetChanged;

         // If the scan type is Document enable the pan/zoom interactive mode by default otherwise (Text scan type) enable select text interactive mode by default.
         if (scanType == DocumentScanType.Document)
         {
            _documentViewer.Commands.Run(DocumentViewerCommands.InteractivePanZoom);
            _activeInteractiveMode = DocumentViewerCommands.InteractivePanZoom;
         }
         else
            _activeInteractiveMode = DocumentViewerCommands.InteractiveSelectText;

         UpdateUIState();

         // Enable inertia scroll
         ToggleInertiaScroll(false);
      }

      private void _documentViewer_Operation(object sender, DocumentViewerOperationEventArgs e)
      {
         switch (e.Operation)
         {
            case DocumentViewerOperation.CreateAutomation:
            case DocumentViewerOperation.AutomationStateChanged:
               if (e.IsPostOperation)
               {
                  if (e.Operation == DocumentViewerOperation.CreateAutomation)
                  {
                     var automation = _documentViewer.Annotations.Automation;
                     automation.ObjectModified += Automation_ObjectModified;
                     automation.OnShowObjectProperties += Automation_OnShowObjectProperties;
                  }

                  UpdateAnnotationActionButtons();
               }
               break;
         }
      }

      private void Automation_OnShowObjectProperties(object sender, Annotations.Automation.AnnAutomationEventArgs e)
      {
         ShowAnnPropertiesPage();
      }

      private void Automation_ObjectModified(object sender, Annotations.Automation.AnnObjectModifiedEventArgs e)
      {
         _annObjectsModified = true;
      }

      // Automation manager helper
      private Leadtools.Annotations.Xamarin.AutomationManagerHelper _automationManagerHelper;
      private void InitAutomation()
      {
         if (_documentViewer.Annotations == null)
            return;

         // Get the automation manager from the document viewer
         var automationManager = _documentViewer.Annotations.AutomationManager;

         // Create the manager helper. This sets the rendering engine
         _automationManagerHelper = new Leadtools.Annotations.Xamarin.AutomationManagerHelper(automationManager);

         // Tell the document viewer that automation manager helper is created
         _documentViewer.Annotations.Initialize();

         // Update our automation objects (set transparency, etc)
         _automationManagerHelper.UpdateAutomationObjects();

         // Create the toolbar
         _automationManagerHelper.AnnObjectSelected += _automationManagerHelper_AnnObjectSelected;
         _automationManagerHelper.CreateToolBar();
         var toolBar = _automationManagerHelper.ToolBar;
         toolBar.BackgroundColor = Color.Transparent;

         AnnotationsScrollView.Content = toolBar;
      }

      /// <summary>
      /// Handles Viewer Key Down event, needed to handle delete selected object.
      /// </summary>
      private void InteractiveService_KeyDown(object sender, InteractiveKeyEventArgs e)
      {
         if (e.KeyCode != Keys.Delete)
            return;

         var automation = _documentViewer.Annotations.Automation;
         if (automation != null && automation.CanDeleteObjects)
         {
            automation.DeleteSelectedObjects();
            UpdateAnnotationActionButtons();
         }
      }

      private async void _automationManagerHelper_AnnObjectSelected(SvgImage sender, Leadtools.Annotations.Xamarin.AutomationManagerHelper.AnnObjectSelectedEventArgs args)
      {
         await HideAnnotationsLayout();

         int id = int.Parse(args.ObjectId);
         _documentViewer.Annotations.AutomationManager.CurrentObjectId = id;
         UpdateAnnotationActionButtons();
      }

      private async void SetDocument(LEADDocument document)
      {
         try
         {
            Document = document;

            _documentAlreadyConverted = SourceDocumentFormat != OcrOutputFormat.None;

            if (document != null)
            {
               document.Text.OcrEngine = _documentConverterHelper.OcrEngine;
               document.Text.ImagesRecognitionMode = DocumentTextImagesRecognitionMode.Auto;
               document.Text.TextExtractionMode = DocumentTextExtractionMode.Auto;
            }

            // Set it in the document viewer
            _documentViewer.SetDocument(document);
            UpdatePageNumberLabel();
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Error setting document inside document viewer: {ex.Message}", "Ok");
         }
      }

      private void SetupUI(DocumentScanType scanType, string pageTitle)
      {
         PageTitle.Text = pageTitle;
      }

      private void UpdateUIState()
      {
         if (_activeInteractiveMode.Equals(DocumentViewerCommands.InteractivePanZoom))
         {
            PanZoomButton.BackgroundColor = Color.LightGray;
            SelectTextButton.BackgroundColor = Color.Transparent;
         }
         else
         {
            SelectTextButton.BackgroundColor = Color.LightGray;
            PanZoomButton.BackgroundColor = Color.Transparent;
         }
      }

      private async void UpdatePageNumberLabel()
      {
         ImageViewer imageViewer = _documentViewer.View.ImageViewer;
         if (imageViewer != null)
         {
            int itemIndex = imageViewer.GetLargestVisibleItemIndex(ImageViewerItemPart.Item);
            if (itemIndex == -1)
               itemIndex = 0;
            await Device.InvokeOnMainThreadAsync(() => CurrentPageNumberLabel.Text = $"Page {itemIndex + 1} / {_documentViewer.PageCount}");
         }
      }

      private void ToggleInertiaScroll(bool turnOn)
      {
         // These commands have ImageViewerPanZoomInteractiveMode in the tag, update the value
         string[] commandNames = { DocumentViewerCommands.InteractivePanZoom, DocumentViewerCommands.InteractivePan };
         foreach (var commandName in commandNames)
         {
            var mode = _documentViewer.Commands.GetCommand(commandName).Tag as ImageViewerPanZoomInteractiveMode;
            if (mode != null)
            {
               var options = mode.InertiaScrollOptions;
               options.IsEnabled = turnOn;
               mode.InertiaScrollOptions = options;
            }
         }
      }

      private async void SpeakButton_Tapped(object sender, EventArgs e)
      {
         if (_isSpeaking)
         {
            SpeakButtonIcon.ResourceName = "Icons/stop-speaking.svg";
            CancelSpeech();
            return;
         }

         string documentText = GetDocumetText();
         if (string.IsNullOrWhiteSpace(documentText))
         {
            await DisplayAlert("Error", $"Document has no recognized text", "Ok");
            return;
         }

         if (string.IsNullOrWhiteSpace(_pageLanguage))
         {
            await DisplayAlert("Error", $"Invalid page language", "Ok");
            return;
         }

         Locale currentLocale = await GetTextToSpeechLocale(_pageLanguage);
         if (currentLocale == null)
         {
            CultureInfo culture = new CultureInfo(_pageLanguage);
            await DisplayAlert("Error", $"Speech is not supported for the {culture.DisplayName} Language", "Ok");
            return;
         }

         _isSpeaking = !_isSpeaking;

         SpeakButtonIcon.ResourceName = _isSpeaking ? "Icons/stop-speaking.svg" : "Icons/speak.svg";

         _ = TextToSpeech.SpeakAsync(documentText, new SpeechOptions() { Locale = currentLocale, Pitch = 1.0f, Volume = 1.0f }, _cts.Token).ContinueWith((t) =>
           {
              // Logic that will run after utterance finishes.
              _isSpeaking = false;
              SpeakButtonIcon.ResourceName = "Icons/speak.svg";
           }, TaskScheduler.FromCurrentSynchronizationContext());
      }

      private async Task<Locale> GetTextToSpeechLocale(string pageLanguage)
      {
         Locale locale = null;
         if (!string.IsNullOrWhiteSpace(pageLanguage))
         {
            CultureInfo culture = new CultureInfo(pageLanguage);
            IEnumerable<Locale> locales = await TextToSpeech.GetLocalesAsync();
            var filteredLocales = locales.Where(l =>
            {
               CultureInfo cultureInfo = null;
               try
               {
                  cultureInfo = new CultureInfo(l.Language);
               }
               catch (Exception)
               {
                  try
                  {
                     return l.Language.Equals(culture.TwoLetterISOLanguageName) || l.Language.Equals(culture.ThreeLetterISOLanguageName);
                  }
                  catch (Exception)
                  {
                  }
               }

               return (cultureInfo != null) ? (cultureInfo.TwoLetterISOLanguageName.Equals(culture.TwoLetterISOLanguageName) || cultureInfo.ThreeLetterISOLanguageName.Equals(culture.ThreeLetterISOLanguageName)) : false;
            });

            if (filteredLocales != null && filteredLocales.Count() > 0)
               locale = filteredLocales.ElementAt(0);
         }

         return locale;
      }

      private string GetDocumetText()
      {
         string documentText = null;
         try
         {
            documentText = _documentViewer.Text.ExportText(0).Replace("\n", "").Replace("\r", "");
         }
         catch (Exception)
         { }

         return documentText;
      }

      // Cancel speech if a cancellation token exists & hasn't been already requested.
      private void CancelSpeech()
      {
         try
         {
            _isSpeaking = false;
            if (_cts?.IsCancellationRequested ?? true)
               return;

            _cts.Cancel();
         }
         finally
         {
            _cts = new CancellationTokenSource();
         }
      }

      private async void CopyButton_Tapped(object sender, EventArgs e)
      {
         bool documentHasText = !string.IsNullOrWhiteSpace(GetDocumetText());
         if (!_documentViewer.Text.HasAnySelectedText && documentHasText)
         {
            bool oldValue = _documentViewer.Text.RenderSelection;
            _documentViewer.Text.RenderSelection = false;
            _documentViewer.Text.SelectAll(0);
            _documentViewer.Text.Copy(0);
            _documentViewer.Text.RenderSelection = oldValue;
            _documentViewer.Text.ClearSelection(0);
         }
         else if (documentHasText)
            _documentViewer.Text.Copy(0);

         string message = string.Format("{0}", (!documentHasText) ? "Document has no recognized text" : (_documentViewer.Text.HasAnySelectedText ? "Selected text copied to clipboard" : "Page text copied to clipboard"));
         await DependencyService.Get<IToast>().Show(message, false);
      }

      private void PanZoomButton_Tapped(object sender, EventArgs e)
      {
         _documentViewer.Commands.Run(DocumentViewerCommands.InteractivePanZoom);
         _activeInteractiveMode = DocumentViewerCommands.InteractivePanZoom;
         UpdateUIState();
      }

      private void SelectTextButton_Tapped(object sender, EventArgs e)
      {
         _documentViewer.Commands.Run(DocumentViewerCommands.InteractiveSelectText);
         _activeInteractiveMode = DocumentViewerCommands.InteractiveSelectText;
         UpdateUIState();
      }

      private async void ShareButton_Tapped(object sender, EventArgs e)
      {
         await ShowExportDocumentLayout();
      }

      private bool HasAnnotations
      {
         get
         {
            bool hasAnnotations = false;
            foreach (var container in _documentViewer.Annotations.Automation.Containers)
            {
               if (container.Children.Count > 0)
               {
                  hasAnnotations = true;
                  break;
               }
            }

            return hasAnnotations;
         }
      }

      private bool IsPdfFormat(OcrOutputFormat format)
      {
         return (
               format == OcrOutputFormat.Pdf ||
               format == OcrOutputFormat.PdfA ||
               format == OcrOutputFormat.PdfAImageOverText ||
               format == OcrOutputFormat.PdfEmbeddedFonts ||
               format == OcrOutputFormat.PdfImageOverText ||
               format == OcrOutputFormat.PdfImageOverTextEmbeddedFonts);
      }

      private async void PopupNavigation_Popped(object sender, Rg.Plugins.Popup.Events.PopupNavigationEventArgs e)
      {
         if (e.Page is OcrFormatsPage)
         {
            // This code handles a case where the user tapped on "SHARE" option and when the formats popup showed up
            // he/she tapped outside the popup to close it.
            await HideOutputFormatsLayout();
            PopupNavigation.Instance.Popped -= PopupNavigation_Popped;
         }
      }

      private async void ExportDocumentCancelButton_Tapped(object sender, EventArgs e)
      {
         await HideExportDocumentLayout();
      }

      string _tempFileToDeleteAfterShare = string.Empty;
      private async void ExportDocumentContinueButton_Tapped(object sender, EventArgs e)
      {
         await HideExportDocumentLayout();

         try
         {
            // Delete last saved temp file when user taps on "Share" button (this will contain the zip file path if the shared document 
            // format is SVG, since it doesn't support multipage and the document will be virtual document that consists of multiple documents).
            if (!string.IsNullOrWhiteSpace(_tempFileToDeleteAfterShare) && File.Exists(_tempFileToDeleteAfterShare))
               File.Delete(_tempFileToDeleteAfterShare);
         }
         catch (Exception) { }

         if (Document != null && (!_documentAlreadyConverted || (HasAnnotations && IsPdfFormat(SourceDocumentFormat))))
         {
            _shareAfterConversion = true;
            ConvertDocument();
            return;
         }

         if (_documentAlreadyConverted)
            _tempFileToDeleteAfterShare = await LEADDocumentHelper.ShareDocument(Document, null, OutputDocumentNameLabel.Text);
      }

      private async void ConvertDocument()
      {
         // If we are sharing the document while the source document is already a PDF file then don't show the select output format popup layout.
         if (_shareAfterConversion && IsPdfFormat(SourceDocumentFormat) && HasAnnotations)
            ShowDocumentConverterPage(outputFormat: SourceDocumentFormat);
         else
            await ShowOutputFormatsLayout();
      }

      private async Task ShowOutputFormatsLayout()
      {
         Grid.SetRowSpan(PopupShadow, 2);
         PopupShadow.IsVisible = true;
         await PopupShadow.FadeTo(Popup_ShadowOpacity, Popup_AnimationDuration, CustomPageAnimation.QuintOut);

         OcrFormatsPage ocrFormatsPage = new OcrFormatsPage(OutputDocumentFormat, false, true, true, DemoUtilities.DisplayHeight / 2, 15);
         ocrFormatsPage.SelectedFormatChanged += OcrFormatsPage_SelectedFormatChanged;
         PopupNavigation.Instance.Popped -= PopupNavigation_Popped;
         PopupNavigation.Instance.Popped += PopupNavigation_Popped;
         await PopupNavigation.Instance.PushAsync(ocrFormatsPage);
      }

      private async Task HideOutputFormatsLayout()
      {
         await PopupShadow.FadeTo(0, Popup_AnimationDuration, CustomPageAnimation.QuintIn);
         PopupShadow.IsVisible = false;
         Grid.SetRowSpan(PopupShadow, 4);
      }

      private async void OcrFormatsPage_SelectedFormatChanged(object sender, SelectedFormatChangedEventArgs e)
      {
         OutputDocumentFormat = e.OcrOutputFormat;
         await HideOutputFormatsLayout();

         ShowDocumentConverterPage(outputFormat: e.OcrOutputFormat);
      }

      private DocumentConverterAnnotationsMode _annotationsMode = DocumentConverterAnnotationsMode.External;
      private async void ShowDocumentConverterPage(OcrOutputFormat outputFormat)
      {
         bool shouldBurnAnnotations = false;
         if (_shareAfterConversion && (IsPdfFormat(SourceDocumentFormat) || IsPdfFormat(outputFormat)) && HasAnnotations)
         {
            // If we have annotation objects drawn and user would like to share his document then show message box asking him/her
            // if they would like to burn the annotations.
            shouldBurnAnnotations = await DisplayAlert("Burn Annotations", "Would you like to burn the drawn annotations into the shared document?", "Yes", "No");
         }

         _annotationsMode = shouldBurnAnnotations ? DocumentConverterAnnotationsMode.Overlay : DocumentConverterAnnotationsMode.External;
         DocumentConverterPage documentConverterPage = new DocumentConverterPage(
            _documentViewer,
            _applicationDirectory,
            _documentsDirectory,
            new List<string> { _documentViewer.Document.DocumentId },
            _documentConverterHelper,
            outputFormat,
            _annotationsMode);

         documentConverterPage.PageClosing += DocumentConverterPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(documentConverterPage);
      }

      private async void DocumentConverterPage_PageClosing(object sender, DocumentConvertEventArgs e)
      {
         string oldDocumentId = _documentViewer.Document.DocumentId;
         DocumentConvertResult result = e.Results[oldDocumentId];
         if (result == null) return;

         try
         {
            if (result.Error != null)
               await Device.InvokeOnMainThreadAsync(async () => { await DisplayAlert("Error", $"Error converting document: {result.Error.Message}", "OK"); });
            else
            {
               if (result.Status != DocumentConverterJobStatus.Aborted)
               {
                  await Device.InvokeOnMainThreadAsync(async () =>
                  {
                     try
                     {
                        Exception error = null;
                        LEADDocument convertedDocument = null;
                        // if we have no documents yet or if we are sharing document and burning annotations into the document then this 
                        // document is a temp and we shouldn't update the document viewer with it.
                        if (_documents == null || (_shareAfterConversion && _annotationsMode == DocumentConverterAnnotationsMode.Overlay))
                        {
                           List<Exception> errors = null;
                           convertedDocument = LEADDocumentHelper.CreateDocument(result.OutputDocumentFiles, null, null, out errors);
                           if (errors != null && errors.Count > 0)
                              error = errors[0];
                        }
                        else
                           convertedDocument = LEADDocumentHelper.ReplaceDocument(oldDocumentId, result.OutputDocumentFiles, _documents, _documentIndexInList, out error);

                        if (convertedDocument != null)
                        {
                           if (!(_shareAfterConversion && _annotationsMode == DocumentConverterAnnotationsMode.Overlay))
                           {
                              SetDocument(null);
                              _documentAlreadyConverted = true;
                              _annObjectsModified = false;
                              // Change the SourceDocumentFormat since we converted the document.
                              SourceDocumentFormat = e.OutputFormat;

                              SetDocument(convertedDocument);
                              Document = _documentViewer.Document;
                           }

                           if (_shareAfterConversion)
                              _tempFileToDeleteAfterShare = await LEADDocumentHelper.ShareDocument(convertedDocument, null, !string.IsNullOrWhiteSpace(OutputDocumentNameLabel.Text) ? OutputDocumentNameLabel.Text : PageTitle.Text);
                        }
                        else if (error != null)
                           await Device.InvokeOnMainThreadAsync(async () => { await DisplayAlert("Error", $"Error converting document: {error.Message}", "OK"); });
                     }
                     catch (Exception ex)
                     {
                        await Device.InvokeOnMainThreadAsync(async () => { await DisplayAlert("Error", $"Error converting document: {ex.Message}", "Ok"); });
                     }
                  });
               }
            }
         }
         finally
         {
            _annotationsMode = DocumentConverterAnnotationsMode.External;
         }
      }

      private async Task ShowExportDocumentLayout()
      {
         OutputDocumentNameLabel.Text = PageTitle.Text;

         // Animate
         PopupShadow.IsVisible = true;
         OutputDocumentNameEntry.Text = string.Empty;
         await Task.WhenAll(
            PopupShadow.FadeTo(Popup_ShadowOpacity, Popup_AnimationDuration, CustomPageAnimation.QuintOut),
            Animator.AnimatePanelAsync(ExportDocumentLayout, AnimationDirection.BottomToTop, true, Popup_AnimationDuration)
         );
      }

      private async Task HideExportDocumentLayout()
      {
         if (!ExportDocumentLayout.IsVisible) return;

         await Task.WhenAll(
            PopupShadow.FadeTo(0, Popup_AnimationDuration, CustomPageAnimation.QuintIn),
            Animator.AnimatePanelAsync(ExportDocumentLayout, AnimationDirection.BottomToTop, false, Popup_AnimationDuration)
         );
         PopupShadow.IsVisible = false;
      }

      private async Task ShowThumbnailsLayout()
      {
         PopupShadow.IsVisible = true;

         // Animate
         await Task.WhenAll(
            Animator.AnimatePanelAsync(ThumbnailsLayout, AnimationDirection.BottomToTop, true, Popup_AnimationDuration),
            PopupShadow.FadeTo(Popup_ShadowOpacity, Popup_AnimationDuration, CustomPageAnimation.QuintOut)
         );
      }

      private async Task HideThumbnailsLayout()
      {
         if (!ThumbnailsLayout.IsVisible) return;

         await Task.WhenAll(
            PopupShadow.FadeTo(0, Popup_AnimationDuration, CustomPageAnimation.QuintIn),
            Animator.AnimatePanelAsync(ThumbnailsLayout, AnimationDirection.BottomToTop, false, Popup_AnimationDuration)
         );
         PopupShadow.IsVisible = false;
      }

      private async Task ShowAnnotationsLayout()
      {
         PopupShadow.IsVisible = true;
         // Animate
         await Task.WhenAll(
            Animator.AnimatePanelAsync(AnnotationsLayout, AnimationDirection.BottomToTop, true, Popup_AnimationDuration),
            PopupShadow.FadeTo(Popup_ShadowOpacity, Popup_AnimationDuration, CustomPageAnimation.QuintOut)
         );
      }

      private async Task HideAnnotationsLayout()
      {
         if (!AnnotationsLayout.IsVisible) return;

         await Task.WhenAll(
            PopupShadow.FadeTo(0, Popup_AnimationDuration, CustomPageAnimation.QuintIn),
            Animator.AnimatePanelAsync(AnnotationsLayout, AnimationDirection.BottomToTop, false, Popup_AnimationDuration)
         );
         PopupShadow.IsVisible = false;
      }

      private async void PopupShadow_Tapped(object sender, EventArgs e)
      {
         await HideOutputFormatsLayout();
         await HideExportDocumentLayout();
         await HideThumbnailsLayout();
         await HideAnnotationsLayout();
      }

      private void OutputDocumentNameEntry_TextChanged(object sender, TextChangedEventArgs e)
      {
         if (string.IsNullOrWhiteSpace(e.NewTextValue))
            OutputDocumentNameLabel.Text = PageTitle.Text;
         else
            OutputDocumentNameLabel.Text = e.NewTextValue;
      }

      private async void OutputDocumentNameEntry_KeyboardWillHide(object sender, EventArgs e)
      {
         await ExportDocumentLayout.ScrollToAsync(0, 0, true);
      }
   }

   public class DocumentFormatItem
   {
      public DocumentFormatItem(OcrOutputFormat documentFormat)
      {
         OutputDocumentFormat = documentFormat;
      }

      public OcrOutputFormat OutputDocumentFormat { get; set; }
      public string FormatName
      {
         get
         {
            return (OutputDocumentFormat == OcrOutputFormat.Docx || OutputDocumentFormat == OcrOutputFormat.DocxFramed) ? "DOC" : OutputDocumentFormat.GetFormatName();
         }
      }
   }
}
