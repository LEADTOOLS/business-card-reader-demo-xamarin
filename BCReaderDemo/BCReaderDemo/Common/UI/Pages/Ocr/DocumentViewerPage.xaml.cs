// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Controls;
using Leadtools.Demos.UI.Page;
using Leadtools.Demos.Utils;
using Leadtools.Document;
using Leadtools.Document.Viewer;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Pages.Ocr
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public partial class DocumentViewerPage : PopupPage
   {
      private DocumentViewer _documentViewer = null;
      private string _pageLanguage = null;
      private CancellationTokenSource _cts = null;
      private bool _isSpeaking = false;
      private string _documentPath = string.Empty;

      private const double Popup_ShadowOpacity = 0.5;
      private const uint Popup_AnimationDuration = 150;

      public event EventHandler PageClosing;

      public DocumentViewerPage(string documentPath, DocumentScanType scanType, string pageLanguage)
      {
         InitializeComponent();

#if __IOS__
         HasSystemPadding = false;
#endif

         _documentPath = documentPath;
         _pageLanguage = pageLanguage;
         _cts = new CancellationTokenSource();

         SetupUI(scanType);

         InitDocumentViewer(documentPath, scanType);
      }

      private void SetupUI(DocumentScanType scanType)
      {
         PageTitle.Text = (scanType == DocumentScanType.Text) ? "Extracted Results" : "Recognition Results";
         CopyButton.HorizontalOptions = (scanType == DocumentScanType.Text) ? LayoutOptions.End : LayoutOptions.CenterAndExpand;

         SpeakButton.IsVisible = (scanType == DocumentScanType.Text);
         CopyButton.IsVisible = (scanType == DocumentScanType.Text);
         ExportButton.IsVisible = (scanType == DocumentScanType.Document);
      }

      protected override bool OnBackButtonPressed()
      {
         if (PageBackButton.IsEnabled)
         {
            OnPageClosing();
            return true;
         }
         return base.OnBackButtonPressed();
      }

      private void BackButton_Tapped(object sender, EventArgs e)
      {
         OnPageClosing();
      }

      private async void OnPageClosing()
      {
         // Delete last saved temp file when user tapped on "Export" button.
         if (!string.IsNullOrWhiteSpace(_tempOutputFilePath) && _documentPath.CompareTo(_tempOutputFilePath) != 0)
            File.Delete(_tempOutputFilePath);

         _documentViewer.View.ImageViewer.ScrollOffsetChanged -= ImageViewer_ScrollOffsetChanged;

         // Prevent double-tapping
         PageBackButton.IsEnabled = false;

         // Cancel speeck (if playing)
         CancelSpeech();

         PageClosing?.Invoke(this, null);

         // Go back
         await PopupNavigation.Instance.PopAsync();
      }

      // Create the document viewer
      private void InitDocumentViewer(string documentPath, DocumentScanType scanType)
      {
         var createOptions = new DocumentViewerCreateOptions();

         // Set the UI part where the main view is displayed
         createOptions.ViewContainer = DocumentViewerContainer;
         createOptions.UseAnnotations = false;

         // Now create the viewer
         _documentViewer = DocumentViewerFactory.CreateDocumentViewer(createOptions);
         // Set the user name
         _documentViewer.UserName = Environment.UserName;
         // We prefer SVG viewing (if supported)
         _documentViewer.View.PreferredItemType = DocumentViewerItemType.Svg;

         var imageViewer = _documentViewer.View.ImageViewer;
         if(scanType == DocumentScanType.Document)
         {
            imageViewer.ScrollOffsetChanged += ImageViewer_ScrollOffsetChanged;
         }
         imageViewer.BackgroundColor = Color.FromHex("#F5F7FA");
         imageViewer.IsFastScrollEnabled = true;
         imageViewer.Zoom(ControlSizeMode.FitWidth, 1, imageViewer.DefaultZoomOrigin);

         // Helps with debugging of there was a rendering error
         imageViewer.RenderError += (sender, e) =>
         {
            var message = string.Format("Error during render item {0} part {1}: {2}",
               e.Item != null ? imageViewer.Items.IndexOf(e.Item) : -1,
               e.Part,
               e.Error.Message);
            System.Diagnostics.Debug.WriteLine(message);
         };

         _documentViewer.Text.AutoGetText = true;
         _documentViewer.Commands.Run(DocumentViewerCommands.InteractiveAutoPan);

         // Enable the pan/zoom interactive mode only if the scan type is Document and not Text because 
         // we want the suer to be able to select text in case of extracted text mode.
         if(scanType == DocumentScanType.Document)
            _documentViewer.Commands.Run(DocumentViewerCommands.InteractivePanZoom);

         // Enable inertia scroll
         ToggleInertiaScroll(true);

         SetDocument(documentPath, scanType);
      }

      private async void ImageViewer_ScrollOffsetChanged(object sender, EventArgs e)
      {
         ImageViewer imageViewer = sender as ImageViewer;
         if (imageViewer != null)
         {
            int itemIndex = imageViewer.GetLargestVisibleItemIndex(ImageViewerItemPart.Item);
            await MainThread.InvokeOnMainThreadAsync(() => CurrentPageNumberLabel.Text = $"Page {itemIndex + 1} / {_documentViewer.PageCount}");
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

      private void SetDocument(string documentPath, DocumentScanType scanType)
      {
         LEADDocument leadDocument = DocumentFactory.LoadFromFile(documentPath, new LoadDocumentOptions() { UseCache = false, FirstPageNumber = 1, LastPageNumber = -1, MimeType = (scanType == DocumentScanType.Text) ? "text/plain" : null });
         leadDocument.Text.ImagesRecognitionMode = DocumentTextImagesRecognitionMode.Auto;
         leadDocument.Text.TextExtractionMode = DocumentTextExtractionMode.Auto;

         // Set it in the document viewer
         _documentViewer.SetDocument(leadDocument);
      }

      private async void SpeakButton_Tapped(object sender, EventArgs e)
      {
         if (_isSpeaking)
         {
            CancelSpeech();
            _cts = new CancellationTokenSource();
            return;
         }

         _isSpeaking = !_isSpeaking;

         Label speakButton = sender as Label;
         if (speakButton != null)
            speakButton.Text = _isSpeaking ? "STOP" : "SPEAK";

         Locale currentLocale = await GetTextToSpeechLocale(_pageLanguage);
         if (currentLocale == null)
         {
            speakButton.Text = "SPEAK";
            _isSpeaking = false;
            OcrLanguage ocrLanguage = new OcrLanguage(_pageLanguage);
            await DisplayAlert("Error", $"Speech is not supported for the {ocrLanguage.LocalizedName} Language", "Ok");
            return;
         }

         string documentText = GetDocumetText();
         if (string.IsNullOrWhiteSpace(documentText))
         {
            await DisplayAlert("Error", $"Document does not contains text", "Ok");
            return;
         }

         _ = TextToSpeech.SpeakAsync(documentText, new SpeechOptions() { Locale = currentLocale, Pitch = 1.0f, Volume = 1.0f }, _cts.Token).ContinueWith((t) =>
           {
              // Logic that will run after utterance finishes.
              _isSpeaking = false;
              speakButton.Text = "SPEAK";
           }, TaskScheduler.FromCurrentSynchronizationContext());
      }

      private async Task<Locale> GetTextToSpeechLocale(string pageLanguage)
      {
         Locale locale = null;
         if (!string.IsNullOrWhiteSpace(pageLanguage))
         {
            CultureInfo culture = new CultureInfo(pageLanguage);
            IEnumerable<Locale> locales = await TextToSpeech.GetLocalesAsync();
            var filteredLocales = locales.Where(l => l.Language.Equals(culture.TwoLetterISOLanguageName) || l.Language.Equals(culture.ThreeLetterISOLanguageName));
            if (filteredLocales != null && filteredLocales.Count() > 0)
               locale = filteredLocales.ElementAt(0);
         }

         return locale;
      }

      private string GetDocumetText()
      {
         string documentText = _documentViewer.Text.ExportText(0).Replace("\n", "").Replace("\r", "");
         return documentText;
      }

      // Cancel speech if a cancellation token exists & hasn't been already requested.
      private void CancelSpeech()
      {
         SpeakButton.Text = "SPEAK";
         if (_cts?.IsCancellationRequested ?? true)
            return;

         _cts.Cancel();
      }

      private async void CopyButton_Tapped(object sender, EventArgs e)
      {
         if (!_documentViewer.Text.HasAnySelectedText)
         {
            bool oldValue = _documentViewer.Text.RenderSelection;
            _documentViewer.Text.RenderSelection = false;
            _documentViewer.Text.SelectAll(0);
            _documentViewer.Text.Copy(0);
            _documentViewer.Text.RenderSelection = oldValue;
         }
         else
            _documentViewer.Text.Copy(0);

         string message = string.Format("{0}", _documentViewer.Text.HasAnySelectedText ? "Selected text copied to clipboard" : "Page text copied to clipboard");
         await DependencyService.Get<IToast>().Show(message, false);
      }

      private async void ExportButton_Tapped(object sender, EventArgs e)
      {
         await ShowExportDocumentLayout();
      }

      private async void CancelButton_Tapped(object sender, EventArgs e)
      {
         await HideExportDocumentLayout();
      }

      string _tempOutputFilePath = string.Empty;
      private async void ContinueButton_Tapped(object sender, EventArgs e)
      {
         await HideExportDocumentLayout();

         // Delete last saved temp file when user tapped on "Export" button.
         if (!string.IsNullOrWhiteSpace(_tempOutputFilePath) && _documentPath.CompareTo(_tempOutputFilePath) != 0)
            File.Delete(_tempOutputFilePath);

         _tempOutputFilePath = Path.Combine(Path.GetDirectoryName(_documentPath), OutputDocumentNameLabel.Text);

         if(_documentPath.CompareTo(_tempOutputFilePath) != 0)
            File.Copy(_documentPath, _tempOutputFilePath, true);

         await Share.RequestAsync(new ShareFileRequest
         {
            Title = Path.GetFileName(_tempOutputFilePath),
            File = new ShareFile(_tempOutputFilePath)
         });
      }

      public async Task ShowExportDocumentLayout()
      {
         OutputDocumentNameLabel.Text = Path.GetFileName(_documentPath);

         // Animate
         PopupShadow.IsVisible = true;
         ExportDocumentLayout.IsVisible = true;
         await Task.WhenAll(
            PopupShadow.FadeTo(Popup_ShadowOpacity, Popup_AnimationDuration, CustomPageAnimation.QuintOut),
            Animator.AnimatePanelAsync(ExportDocumentLayout, false, true, Popup_AnimationDuration)
         );
      }

      public async Task HideExportDocumentLayout()
      {
         ExportDocumentLayout.IsVisible = false;
         await Task.WhenAll(
            PopupShadow.FadeTo(0, Popup_AnimationDuration, CustomPageAnimation.QuintIn),
            Animator.AnimatePanelAsync(ExportDocumentLayout, false, false, Popup_AnimationDuration)
         );
         PopupShadow.IsVisible = false;
      }

      private async void PopupShadow_Tapped(object sender, EventArgs e)
      {
         await HideExportDocumentLayout();
      }

      private void OutputDocumentNameEntry_TextChanged(object sender, TextChangedEventArgs e)
      {
         if (string.IsNullOrWhiteSpace(e.NewTextValue))
            OutputDocumentNameLabel.Text = Path.GetFileName(_documentPath);
         else
         {
            string ext = Path.GetExtension(_documentPath);
            OutputDocumentNameLabel.Text = string.Format("{0}{1}", e.NewTextValue, ext);
         }
      }
   }
}
