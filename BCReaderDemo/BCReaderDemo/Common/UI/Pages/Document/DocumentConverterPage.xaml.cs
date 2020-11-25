// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Caching;
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.Utils;
using Leadtools.Document;
using Leadtools.Document.Converter;
using Leadtools.Document.Viewer;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.UI.Pages.Document
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public partial class DocumentConverterPage : PopupPage
   {
      public static Color OverlayColor { get; } = Color.FromRgb(235, 242, 255);

      private DocumentConverterHelper _documentConverterHelper = null;
      private string _applicationDirectory = string.Empty;
      private string _documentsDirectory = string.Empty;
      private List<string> DocumentsIds = null;
      private DocumentViewer _documentViewer = null;
      private DocumentConverterAnnotationsMode _annotationsMode = DocumentConverterAnnotationsMode.External;
      private DocumentConvertEventArgs _pageClosingEventArgs = null;

      public event EventHandler<DocumentConvertEventArgs> PageClosing;

      public DocumentConverterPage(DocumentViewer documentViewer, string appDirectory, string documentsDirectory, List<string> documentsIds, DocumentConverterHelper documentConverterHelper, OcrOutputFormat outputFormat, DocumentConverterAnnotationsMode annotationsMode)
      {
         InitializeComponent();

         _documentViewer = documentViewer;
         _applicationDirectory = appDirectory;
         _documentsDirectory = documentsDirectory;
         _documentConverterHelper = documentConverterHelper;
         DocumentsIds = documentsIds;
         _annotationsMode = annotationsMode;

         Disappearing += DocumentConverterPage_Disappearing;

         if (Device.RuntimePlatform == Device.iOS)
            HasSystemPadding = false;

         Task.Factory.StartNew(() => ConvertDocuments(documentsIds, outputFormat));
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

      private async void ConvertDocuments(List<string> documentsIds, OcrOutputFormat outputFormat)
      {
         ConversionData conversionData = null;

         _documentConverterHelper.JobOperation += Converter_JobOperation;

         DocumentConverterJobStatus status = DocumentConverterJobStatus.Success;
         Dictionary<string, DocumentConvertResult> results = new Dictionary<string, DocumentConvertResult>();
         foreach (var documentId in documentsIds)
         {
            try
            {
               LEADDocument document = LEADDocumentHelper.LoadFromCache(documentId);
               if(document == null)
               {
                  results.Add(documentId, new DocumentConvertResult(null, DocumentConverterJobStatus.SuccessWithErrors, new Exception($"Unable to load document with ID {documentId} from cache")));
                  continue;
               }

               string outputDocumentPath = GetOutputDocumentFilePath(document, outputFormat);
               conversionData = new ConversionData()
               {
                  Document = document,
                  DocumentViewer = _documentViewer,
                  FirstPageIndex = 0,
                  LastPageIndex = -1,
                  OutputFormat = outputFormat,
                  OutputDocumentPath = outputDocumentPath,
                  OutputAnnotationsFileName = Path.ChangeExtension(outputDocumentPath, "xml"),
                  AnnotationsMode = _annotationsMode,
                  Zones = null,
                  DocumentScanType = DocumentScanType.Document
               };

               status = _documentConverterHelper.Run(conversionData);
               if (_abort)
                  status = DocumentConverterJobStatus.Aborted;

               List<string> finalOutputDocumentFiles = new List<string>();
               if (_annotationsMode == DocumentConverterAnnotationsMode.External)
               {
                  string firstDocumentFilePath = string.Empty;
                  foreach (string file in _documentConverterHelper.OutputDocumentFiles)
                  {
                     if (File.Exists(file))
                     {
                        if (string.IsNullOrWhiteSpace(firstDocumentFilePath))
                           firstDocumentFilePath = file;
                        string outputDocumentFilePath = Path.Combine(_documentsDirectory, Path.GetFileName(file));
                        if (File.Exists(outputDocumentFilePath))
                           File.Delete(outputDocumentFilePath);

                        File.Move(file, outputDocumentFilePath);
                        finalOutputDocumentFiles.Add(outputDocumentFilePath);
                     }
                  }

                  if (!string.IsNullOrWhiteSpace(firstDocumentFilePath))
                  {
                     // Move annotations file to documents directory
                     string tempAnnotationsFilePath = Path.Combine(Path.GetDirectoryName(firstDocumentFilePath), $"{document.Name}.xml");
                     if (File.Exists(tempAnnotationsFilePath))
                     {
                        string outputAnnotationsFilePath = Path.Combine(_documentsDirectory, Path.ChangeExtension(Path.GetFileName(tempAnnotationsFilePath), "xml"));
                        if (File.Exists(outputAnnotationsFilePath))
                           File.Delete(outputAnnotationsFilePath);

                        File.Move(tempAnnotationsFilePath, outputAnnotationsFilePath);
                     }
                  }
               }
               else if (_documentConverterHelper.OutputDocumentFiles.Count > 0)
               {
                  // In case of burning annotation into PDF file, which only occurs when sharing the document while it has annotations objects drawn
                  // don't move the saved temp file into the final documents directory, share the document from the temp folder
                  finalOutputDocumentFiles.Add(_documentConverterHelper.OutputDocumentFiles[0]);
               }

               results.Add(documentId, new DocumentConvertResult(finalOutputDocumentFiles, status, _documentConverterHelper.Error));

               if (_abort)
               {
                  List<string> convertedFiles = new List<string>();
                  foreach (KeyValuePair<string, DocumentConvertResult> entry in results)
                  {
                     if(entry.Value.OutputDocumentFiles != null && entry.Value.OutputDocumentFiles.Count > 0)
                        convertedFiles.AddRange(entry.Value.OutputDocumentFiles);
                  }

                  DocumentConverterHelper.DeleteAllFiles(convertedFiles);
                  break;
               }
            }
            catch (Exception ex)
            {
               results.Add(documentId, new DocumentConvertResult(null, DocumentConverterJobStatus.SuccessWithErrors, ex));
            }
         }

         _documentConverterHelper.JobOperation -= Converter_JobOperation;

         // Go back
         _pageClosingEventArgs = new DocumentConvertEventArgs(results, status, outputFormat);
         await PopupNavigation.Instance.PopAsync();
      }

      private void DocumentConverterPage_Disappearing(object sender, EventArgs e)
      {
         PageClosing?.Invoke(this, _pageClosingEventArgs);
      }

      private string GetOutputDocumentFilePath(LEADDocument document, OcrOutputFormat outputFormat)
      {
         if (string.IsNullOrWhiteSpace(_documentsDirectory))
         {
            if (document.GetCache() != null)
               _documentsDirectory = (document.GetCache() as FileCache).CacheDirectory;
            else
            {
               _documentsDirectory = Path.Combine(_applicationDirectory, "Cache");
               if (!Directory.Exists(Path.GetDirectoryName(_documentsDirectory)))
                  Directory.CreateDirectory(Path.GetDirectoryName(_documentsDirectory));
            }
         }

         string tempFilesDirectory = Path.Combine(_applicationDirectory, "Temp");
         if (!Directory.Exists(Path.GetDirectoryName(tempFilesDirectory)))
            Directory.Delete(tempFilesDirectory, true);

         Directory.CreateDirectory(tempFilesDirectory);

         // Remove extension from document name (if any)
         string documentName = Path.GetFileNameWithoutExtension(document.Name);
         string outputDocumentFilePath = Path.Combine(tempFilesDirectory, documentName + outputFormat.GetFormatExtention());
         return outputDocumentFilePath;
      }

      private async void Converter_JobOperation(object sender, Leadtools.Document.Converter.DocumentConverterJobEventArgs e)
      {
         ConversionData conversionData = e.Job.JobData.UserData as ConversionData;
         await Device.InvokeOnMainThreadAsync(() =>
         {
            int documentIndex = DocumentsIds.IndexOf(e.Document.DocumentId);
            DocumentsInfoLabel.Text = string.Format("Document {0}/{1}{2}", documentIndex + 1, DocumentsIds.Count, string.IsNullOrWhiteSpace(e.Document.Name) ? string.Empty : $" ({e.Document.Name})");

            CurrentOperationLabel.Text = e.Operation.GetFriendlyName().ToUpper();
            int currentPageNumber = (e.InputDocumentPageNumber == 0) ? 1 : e.InputDocumentPageNumber;
            CurrentPageLabel.Text = string.Format("Page {0}/{1}", currentPageNumber, (conversionData.LastPageIndex > 0) ? conversionData.LastPageIndex + 1 : e.Document.Pages.Count);
         });
      }

      private bool _abort = false;
      private void AbortButton_Tapped(object sender, EventArgs e)
      {
         _abort = true;
         _documentConverterHelper.Abort();
      }

      private void DeleteAlreadyConvertedFiles(List<string> files)
      {
         try
         {
            foreach (string file in files)
            {
               if (File.Exists(file))
                  File.Delete(file);
            }
         }
         catch (Exception) { }
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DocumentConvertResult
   {
      public DocumentConvertResult(List<string> outputDocumentFiles, DocumentConverterJobStatus status, Exception error)
      {
         OutputDocumentFiles = outputDocumentFiles;
         Status = status;
         Error = error;
      }

      public List<string> OutputDocumentFiles { get; }
      public DocumentConverterJobStatus Status { get; }
      public Exception Error { get; }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DocumentConvertEventArgs : EventArgs
   {
      public DocumentConvertEventArgs(Dictionary<string, DocumentConvertResult> results, DocumentConverterJobStatus status, OcrOutputFormat outputFormat)
      {
         Results = results;
         Status = status;
         OutputFormat = outputFormat;
      }

      public Dictionary<string, DocumentConvertResult> Results { get; }
      public DocumentConverterJobStatus Status { get; }
      public OcrOutputFormat OutputFormat { get; }
   }
}
