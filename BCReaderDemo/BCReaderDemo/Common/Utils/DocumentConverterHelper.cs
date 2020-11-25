// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Document;
using Leadtools.Document.Converter;
using Leadtools.Document.Viewer;
using Leadtools.Document.Writer;
using Leadtools.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.Utils
{
   // OCR LEAD "Recognition.Preprocess.ModifyOriginalImageOptions"
   [Flags]
   internal enum ModifyOriginalImageOptions
   {
      None = 0x00,
      Deskew = 0x01,
      Rotate = 0x02,
      Invert = 0x04
   }

   // OCR LEAD "Recognition.Zoning.Options"
   [Flags]
   internal enum AutoZoneOptions
   {
      None = 0x0000,
      DetectText = 0x0001,
      DetectGraphics = 0x0002,
      DetectTable = 0x0004,
      AllowOverlap = 0x0008,
      DetectAccurateZones = 0x0010,
      RecognizeOneCellTable = 0x0020,
      TableCellsAsZones = 0x0040,
      UseAdvancedTableDetection = 0x0080,
      UseTextExtractor = 0x0100,
      DetectCheckbox = 0x0200,
      FavorGraphics = 0x0400
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class SelectedAreaInfo
   {
      public LeadRect SelectedArea { get; set; }
      public bool SelectedAreaEnabled { get; set; }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class ConversionData
   {
      public ConversionData()
      {
      }

      public LEADDocument Document { get; set; }
      public DocumentViewer DocumentViewer { get; set; }
      public int FirstPageIndex { get; set; }
      public int LastPageIndex { get; set; }
      public OcrOutputFormat OutputFormat { get; set; }
      public string OutputDocumentPath { get; set; }
      public string OutputAnnotationsFileName { get; set; }
      public DocumentConverterAnnotationsMode AnnotationsMode { get; set; }
      public DocumentScanType DocumentScanType { get; set; }
      public Dictionary<int, SelectedAreaInfo> Zones { get; set; }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public static class DocumentConverterJobOperationExtensions
   {
      public static string GetFriendlyName(this DocumentConverterJobOperation operation)
      {
         string operationFriendlyName = string.Empty;

         switch (operation)
         {
            case DocumentConverterJobOperation.Started: operationFriendlyName = "Started"; break;
            case DocumentConverterJobOperation.Completed: operationFriendlyName = "Completed"; break;
            case DocumentConverterJobOperation.CreateDocument: operationFriendlyName = "Create Document"; break;
            case DocumentConverterJobOperation.BeginDocumentWriter: operationFriendlyName = "Begin Document Writer"; break;
            case DocumentConverterJobOperation.EndDocumentWriter: operationFriendlyName = "End Document Writer"; break;
            case DocumentConverterJobOperation.Other: operationFriendlyName = "Other"; break;
            case DocumentConverterJobOperation.LoadRasterPage: operationFriendlyName = "Load Raster Page"; break;
            case DocumentConverterJobOperation.SaveRasterPage: operationFriendlyName = "Save Raster Page"; break;
            case DocumentConverterJobOperation.LoadSvgPage: operationFriendlyName = "Load Svg Page"; break;
            case DocumentConverterJobOperation.AddDocumentPage: operationFriendlyName = "Add Document Page"; break;
            case DocumentConverterJobOperation.PreprocessPage: operationFriendlyName = "Preprocess Page"; break;
            case DocumentConverterJobOperation.CreateOcrDocument: operationFriendlyName = "Create Ocr Document"; break;
            case DocumentConverterJobOperation.AddOcrPage: operationFriendlyName = "Add Ocr Page"; break;
            case DocumentConverterJobOperation.AutoZoneOcrPage: operationFriendlyName = "Auto Zone Ocr Page"; break;
            case DocumentConverterJobOperation.RecognizeOcrPage: operationFriendlyName = "Recognize Ocr Page"; break;
            case DocumentConverterJobOperation.SaveOcrPage: operationFriendlyName = "Save Ocr Page"; break;
            case DocumentConverterJobOperation.SaveOcrDocument: operationFriendlyName = "Save Ocr Document"; break;
            case DocumentConverterJobOperation.ConvertOcrDocument: operationFriendlyName = "Convert Ocr Document"; break;
            case DocumentConverterJobOperation.LoadAnnotations: operationFriendlyName = "Load Annotations"; break;
            case DocumentConverterJobOperation.SaveAnnotations: operationFriendlyName = "Save Annotations"; break;
            case DocumentConverterJobOperation.EmbedAnnotations: operationFriendlyName = "Embed Annotations"; break;
            case DocumentConverterJobOperation.ConvertDocument: operationFriendlyName = "Convert Document"; break;
            case DocumentConverterJobOperation.OverlayAnnotations: operationFriendlyName = "Overlay Annotations"; break;
            case DocumentConverterJobOperation.BeginUploadDocument: operationFriendlyName = "Begin Upload Document"; break;
            case DocumentConverterJobOperation.UploadDocument: operationFriendlyName = "Upload Document"; break;
            case DocumentConverterJobOperation.UploadAnnotations: operationFriendlyName = "Upload Annotations"; break;
            case DocumentConverterJobOperation.EndUploadDocument: operationFriendlyName = "End Upload Document"; break;
            case DocumentConverterJobOperation.LinearizeDocument: operationFriendlyName = "Linearize Document"; break;
         }

         return operationFriendlyName;
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DocumentConverterHelper : BindableObject
   {
      public DocumentConverterHelper()
      {
      }

      #region Bindable properties

      #region ActiveLanguages

      public static readonly BindableProperty ActiveLanguagesProperty = BindableProperty.Create(
         propertyName: nameof(ActiveLanguages),
         returnType: typeof(ObservableCollection<OcrLanguage>),
         declaringType: typeof(DocumentConverterHelper),
         defaultValue: new ObservableCollection<OcrLanguage>() { new OcrLanguage("en") },
         defaultBindingMode: BindingMode.TwoWay
      );

      public ObservableCollection<OcrLanguage> ActiveLanguages
      {
         get => GetValue(ActiveLanguagesProperty) is ObservableCollection<OcrLanguage> value ? value : null;
         set => SetValue(ActiveLanguagesProperty, value);
      }

      #endregion // ActiveLanguages

      #region DetectGraphicsAndColors

      public static readonly BindableProperty DetectGraphicsAndColorsProperty = BindableProperty.Create(
         propertyName: nameof(DetectGraphicsAndColors),
         returnType: typeof(bool),
         declaringType: typeof(DocumentConverterHelper),
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
         declaringType: typeof(DocumentConverterHelper),
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
         declaringType: typeof(DocumentConverterHelper),
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
         declaringType: typeof(DocumentConverterHelper),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool IntelligentSelectArea
      {
         get => GetValue(IntelligentSelectAreaProperty) is bool value ? value : true;
         set => SetValue(IntelligentSelectAreaProperty, value);
      }

      #endregion // IntelligentSelectArea

      #region AutoInvertImages

      public static readonly BindableProperty AutoInvertImagesProperty = BindableProperty.Create(
         propertyName: nameof(AutoInvertImages),
         returnType: typeof(bool),
         declaringType: typeof(DocumentConverterHelper),
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
         declaringType: typeof(DocumentConverterHelper),
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
         declaringType: typeof(DocumentConverterHelper),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );

      public bool DiscardNoisyZones
      {
         get => GetValue(DiscardNoisyZonesProperty) is bool value ? value : true;
         set => SetValue(DiscardNoisyZonesProperty, value);
      }

      #endregion // DiscardNoisyZones

      #endregion // Bindable properties

      #region properties

      public IOcrEngine OcrEngine { get; set; }
      public Exception Error { get; private set; }
      public string DetectedLanguage { get; private set; }
      public List<string> OutputDocumentFiles { get; private set; }

      #endregion

      #region Methods

      #region Helpers

      public void Init(string ocrRuntimeDirectory)
      {
         if (OcrEngine != null && OcrEngine.IsStarted)
            return;

         try
         {
#if LEADTOOLS_V21_OR_LATER
            OcrEngine = OcrEngineManager.CreateEngine(OcrEngineType.LEAD);
#else
            OcrEngine = OcrEngineManager.CreateEngine(OcrEngineType.LEAD, false);
#endif // #if LEADTOOLS_V21_OR_LATER
            OcrEngine.Startup(null, null, null, ocrRuntimeDirectory);

            // The engine should create a work image each time it does processing/recognition
            OcrEngine.SettingManager.SetBooleanValue("Recognition.ModifyProcessingImage", false);
            // The engine should create and manage it's own image for each page
            OcrEngine.SettingManager.SetBooleanValue("Recognition.ShareOriginalImage", false);

            if (ActiveLanguages == null)
            {
               ActiveLanguages = new ObservableCollection<OcrLanguage>();
               string[] activeLanguages = OcrEngine.LanguageManager.GetEnabledLanguages();
               foreach (string lang in activeLanguages)
               {
                  ActiveLanguages.Add(new OcrLanguage(lang) { IsDefaultLanguage = lang.Equals(activeLanguages[0]) });
               }
            }
         }
         catch (Exception ex)
         {
            Error = ex;
         }
      }

      private bool _abort = false;
      public void Abort()
      {
         _abort = true;
      }

      private void DocumentConverterJobs_JobStarted(object sender, DocumentConverterJobEventArgs e)
      {
         JobStarted?.Invoke(sender, e);
      }

      private void DocumentConverterJobs_JobOperation(object sender, DocumentConverterJobEventArgs e)
      {
         if (_abort)
         {
            e.Job.DocumentConverter.Jobs.AbortAllJobs();
            e.Status = DocumentConverterJobStatus.Aborted;
            return;
         }

         ConversionData conversionData = e.Job.JobData.UserData as ConversionData;

         if (e.Operation == DocumentConverterJobOperation.AutoZoneOcrPage && !e.IsPostOperation && e.OcrPage != null)
         {
            SelectedAreaInfo pageAreaInfo = null;
            if (conversionData != null && conversionData.Zones != null && conversionData.Zones.Count > 0 && conversionData.Zones.ContainsKey(e.InputDocumentPageNumber - 1) && conversionData.Zones[e.InputDocumentPageNumber - 1] != null)
            {
               pageAreaInfo = conversionData.Zones[e.InputDocumentPageNumber - 1];
               if(pageAreaInfo != null && pageAreaInfo.SelectedAreaEnabled && !pageAreaInfo.SelectedArea.IsEmpty)
                  e.OcrPage.SetAreaOptions(new OcrPageAreaOptions() { Area = pageAreaInfo.SelectedArea, IntersectPercentage = 50, UseTextZone = !IntelligentSelectArea });
            }
         }

         if (e.Operation == DocumentConverterJobOperation.RecognizeOcrPage && !e.IsPostOperation && e.OcrPage != null)
         {
            // Detect the language for the first page only
            if (string.IsNullOrWhiteSpace(DetectedLanguage))
            {
               List<string> supportedLanguages = new List<string>();
               foreach (OcrLanguage language in ActiveLanguages)
               {
                  supportedLanguages.Add(language.Language);
               }
               string[] detectedLanguages = e.OcrPage.DetectLanguages(supportedLanguages.ToArray());
               if (detectedLanguages != null && detectedLanguages.Length > 0)
                  DetectedLanguage = detectedLanguages[0];
            }
         }

         JobOperation?.Invoke(sender, e);
      }

      private void DocumentConverterJobs_JobCompleted(object sender, DocumentConverterJobEventArgs e)
      {
         JobCompleted?.Invoke(sender, e);
      }

      public event EventHandler<DocumentConverterJobEventArgs> JobStarted;
      public event EventHandler<DocumentConverterJobEventArgs> JobCompleted;
      public event EventHandler<DocumentConverterJobEventArgs> JobOperation;

      public DocumentConverterJobStatus Run(ConversionData conversionData)
      {
         Error = null;
         DocumentConverter converter = null;
         OutputDocumentFiles = null;
         Dictionary<string, string> modifiedSettings = null;

         try
         {
            _abort = false;
            DetectedLanguage = null;

            IEnumerable<KeyValuePair<int, SelectedAreaInfo>> selectedAreas = (conversionData.Zones != null) ? conversionData.Zones.Where(area => area.Value != null) : null;

            modifiedSettings = SetRecognizeOptions(conversionData.OutputFormat, (selectedAreas != null && selectedAreas.Count() > 0), conversionData.Document.Pages[0].GetImage());
            EnableEngineLanguages();

            converter = new DocumentConverter();
            converter.Jobs.JobStarted += DocumentConverterJobs_JobStarted;
            converter.Jobs.JobOperation += DocumentConverterJobs_JobOperation;
            converter.Jobs.JobCompleted += DocumentConverterJobs_JobCompleted;

            // Set the options in the document converter from our data
            SetConversionOptions(converter, conversionData);

            // Create the document converter job
            var job = CreateConverterJob(converter, conversionData);

            try
            {
               try
               {
                  // Delete output file path (if exists)
                  if (!string.IsNullOrWhiteSpace(conversionData.OutputDocumentPath) && File.Exists(conversionData.OutputDocumentPath))
                     File.Delete(conversionData.OutputDocumentPath);
               }
               catch (Exception) { }
            }
            catch(Exception)
            { }

            // We need this call to save annotations drawn over the document viewer
            if (conversionData.DocumentViewer != null)
               conversionData.DocumentViewer.PrepareToSave();

            // Run the job
            converter.Jobs.RunJob(job);

            IList<string> outputDocumentFiles = (job.OutputFiles.Count > job.OutputDocumentFiles.Count) ? job.OutputFiles : job.OutputDocumentFiles;
            OutputDocumentFiles = new List<string>();
            if (outputDocumentFiles != null && outputDocumentFiles.Count > 0)
            {
               // Check if we have annotations XML files in the list then remove them from the list
               for(int i = outputDocumentFiles.Count - 1; i >= 0; i--)
               {
                  string file = outputDocumentFiles[i];
                  if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                     outputDocumentFiles.RemoveAt(i);
               }

               // Check the errors
               if (_abort)
               {
                  DeleteAllFiles(outputDocumentFiles);
                  outputDocumentFiles.Clear();
                  return DocumentConverterJobStatus.Aborted;
               }

               if (outputDocumentFiles.Count > 1)
               {
                  // When saving to document format that doesn't support multipage like SVG, the output files are not necessarily in the correct 
                  // source document pages order, so we need to sort them out ourselves.
                  string directory = Path.GetDirectoryName(conversionData.OutputDocumentPath);
                  string fileName = Path.GetFileNameWithoutExtension(conversionData.OutputDocumentPath);
                  string ext = conversionData.OutputFormat.GetFormatExtention();
                  for (int i = 0; i < outputDocumentFiles.Count; i++)
                  {
                     string filePath = Path.Combine(directory, $"{fileName}_Page({i + 1}){ext}");
                     OutputDocumentFiles.Add(filePath);
                  }
               }
               else
                  OutputDocumentFiles.Add(outputDocumentFiles[0]);
            }

            // Get the first error (if there is any)
            if (job.Errors.Count > 0)
               Error = job.Errors[0].Error;

            return job.Status;
         }
         catch (Exception ex)
         {
            Error = ex;
            Console.WriteLine(ex.Message);
         }
         finally
         {
            if (modifiedSettings != null)
               RestoreOcrEngineSettings(modifiedSettings);

            if (converter != null)
               converter.Dispose();
         }

         return DocumentConverterJobStatus.SuccessWithErrors;
      }

      public static void DeleteAllFiles(IList<string> files)
      {
         if (files == null)
            return;

         foreach (var file in files)
         {
            if (File.Exists(file))
            {
               try
               {
                  File.Delete(file);
               }
               catch { }
            }
         }
      }

      // Change the recognition options of the engine based on the demo options and output format
      private Dictionary<string, string> SetRecognizeOptions(OcrOutputFormat outputFormat, bool hasSelectionArea, RasterImage ocrImage)
      {
         IOcrSettingManager settingManager = OcrEngine.SettingManager;

         // Detect whether we want to export graphics and colored text
         // Disabling this option if not needed (for example, when the output format is text) enhances the optimization speed
         bool detectColors;

         switch (outputFormat)
         {
            case OcrOutputFormat.Text:
            case OcrOutputFormat.TextFormatted:
            case OcrOutputFormat.PdfImageOverText:
            case OcrOutputFormat.PdfAImageOverText:
            case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
               detectColors = false;
               break;

            default:
               detectColors = DetectGraphicsAndColors;
               break;
         }

         // Detect whether we want to detect font styles and attributes
         // Disabling this option if not needed (for example, when the output format is text) enhances the optimization speed
         bool detectFontStylesAndAttributes;
         switch (outputFormat)
         {
            case OcrOutputFormat.Text:
            case OcrOutputFormat.TextFormatted:
               detectFontStylesAndAttributes = false;
               break;

            default:
               detectFontStylesAndAttributes = true;
               break;
         }

         // Detect whether we want to detect font styles and attributes
         // Disabling this option if not needed (for example, when the output format is text) enhances the optimization speed
         bool detectTables;
         switch (outputFormat)
         {
            case OcrOutputFormat.Text:
               // For formatted text, use the option since tables will help in constructing the final document accurately
               detectTables = false;
               break;

            default:
               // Use what the user selected in the options
               detectTables = DetectTables;
               break;
         }

         // Set this if image has DPI greater than or equal to 150
         bool isDocumentImage = Math.Max(ocrImage.XResolution, ocrImage.YResolution) >= 150;

         // Now set the settings in the OCR engine (saving the original settings so we can restore them later)
         Dictionary<string, string> modifiedSettings = new Dictionary<string, string>();

         string settingName = "Recognition.DetectColors";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         settingManager.SetBooleanValue(settingName, detectColors);

         settingName = "Recognition.Fonts.RecognizeFontAttributes";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         settingManager.SetBooleanValue(settingName, detectFontStylesAndAttributes);

         settingName = "Recognition.Fonts.DetectFontStyles";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         if (!detectFontStylesAndAttributes)
            settingManager.SetEnumValue(settingName, (int)OcrCharacterFontStyle.Regular);

         settingName = "Recognition.Zoning.Options";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         int autoZoneOptions = (int)settingManager.GetEnumValue(settingName);
         if (!detectTables)
         {
            // Remove table detection
            autoZoneOptions &= ~(int)AutoZoneOptions.DetectTable;
         }

         if (!DetectGraphicsAndColors)
         {
            // Remove graphics detection
            autoZoneOptions &= ~(int)AutoZoneOptions.DetectGraphics;
         }
         settingManager.SetEnumValue(settingName, autoZoneOptions);

         // Tell the OCR engine if the image is a document or a picture
         settingManager.SetBooleanValue("Recognition.Preprocess.MobileImagePreprocess", !isDocumentImage);

         // Tell the OCR engine to auto-process the inverted regions in the image
         settingName = "Recognition.Preprocess.RemoveInvertedTextRegionsFromProcessImage";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         settingManager.SetBooleanValue(settingName, DetectInvertedRegions);

         // Tell the OCR engine to ignore zones with low confidence values if we are performing full page recognition
         settingName = "Recognition.Words.DiscardLowConfidenceZones";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         settingManager.SetBooleanValue(settingName, !hasSelectionArea);

         // Discard all the results in the zone if the engine determines that all the characters recognized are noise
         settingName = "Recognition.CharacterFilter.DiscardNoisyZones";
         modifiedSettings[settingName] = settingManager.GetValue(settingName);
         settingManager.SetBooleanValue(settingName, DiscardNoisyZones);

         return modifiedSettings;
      }

      private void RestoreOcrEngineSettings(Dictionary<string, string> modifiedSettings)
      {
         foreach (KeyValuePair<string, string> entry in modifiedSettings)
            OcrEngine.SettingManager.SetValue(entry.Key, entry.Value);
      }

      private void EnableEngineLanguages()
      {
         if (OcrEngine == null) return;

         try
         {
            var enabledEngineLanguages = OcrEngine.LanguageManager.GetEnabledLanguages();
            bool enabledLanguagesCahnged = enabledEngineLanguages.Length != ActiveLanguages.Count;
            if (!enabledLanguagesCahnged)
            {
               foreach (OcrLanguage lang in ActiveLanguages)
               {
                  if (!enabledEngineLanguages.Contains(lang.Language))
                  {
                     enabledLanguagesCahnged = true;
                     break;
                  }
               }
            }

            if (enabledLanguagesCahnged)
            {
               List<string> newLanguagesToEnable = new List<string>();
               foreach (OcrLanguage lang in ActiveLanguages)
                  newLanguagesToEnable.Add(lang.Language);

               OcrEngine.LanguageManager.EnableLanguages(newLanguagesToEnable.ToArray());
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private void SetConversionOptions(DocumentConverter converter, ConversionData conversionData)
      {
         if (conversionData.DocumentViewer != null && conversionData.DocumentViewer.Annotations != null)
            converter.SetAnnRenderingEngineInstance(conversionData.DocumentViewer.Annotations.AutomationControl.RenderingEngine);

         // Set the OCR engine
         if (OcrEngine != null && OcrEngine.IsStarted)
            converter.SetOcrEngineInstance(OcrEngine, false);

         if (converter.DocumentWriterInstance == null)
            converter.SetDocumentWriterInstance(new Leadtools.Document.Writer.DocumentWriter());

         var outputFormat = conversionData.OutputFormat.GetDocumentFormat();
         var documentWriterOptions = conversionData.OutputFormat.GetDocumentOptions();
         OcrEngine.DocumentWriterInstance.SetOptions(outputFormat, documentWriterOptions);
         converter.DocumentWriterInstance.SetOptions(outputFormat, documentWriterOptions);

         // Set pre-processing options
         converter.Preprocessor.Invert = AutoInvertImages;
         converter.Preprocessor.Orient = AutoRotateImages;

         // Set options
         converter.Options.JobErrorMode = DocumentConverterJobErrorMode.Continue;
         converter.Options.EnableSvgConversion = (conversionData.Zones != null && conversionData.Zones.Count > 0) ? false : true; // Force page to go through OCR engine if we have zone
         converter.Options.SvgImagesRecognitionMode = DocumentConverterSvgImagesRecognitionMode.Auto;
         converter.Options.EmptyPageMode = DocumentConverterEmptyPageMode.None;
         converter.Options.UseThreads = true;
         converter.Diagnostics.EnableTrace = false;
      }

      private DocumentConverterJob CreateConverterJob(DocumentConverter converter, ConversionData conversionData)
      {
         // Set the maximum page
         var firstPageNumber = conversionData.FirstPageIndex + 1;
         if (firstPageNumber == 0)
            firstPageNumber = 1;

         var lastPageNumber = conversionData.LastPageIndex + 1;
         if (lastPageNumber == 0)
            lastPageNumber = -1;

         // Create a job
         var jobData = new DocumentConverterJobData
         {
            InputDocumentFileName = null,
            Document = conversionData.Document,
            InputDocumentFirstPageNumber = firstPageNumber,
            InputDocumentLastPageNumber = lastPageNumber,
            DocumentFormat = (conversionData.AnnotationsMode == DocumentConverterAnnotationsMode.Overlay) ? DocumentFormat.User : conversionData.OutputFormat.GetDocumentFormat(),
            OutputDocumentFileName = conversionData.OutputDocumentPath,
            AnnotationsMode = conversionData.AnnotationsMode,
            OutputAnnotationsFileName = (conversionData.AnnotationsMode == DocumentConverterAnnotationsMode.External) ? conversionData.OutputAnnotationsFileName : null,
            RasterImageFormat = (conversionData.AnnotationsMode == DocumentConverterAnnotationsMode.Overlay) ? RasterImageFormat.RasPdfJpeg422 : RasterImageFormat.Unknown,
            RasterImageBitsPerPixel = 24,
            JobName = "My Job",
            UserData = conversionData,
         };

         var job = converter.Jobs.CreateJob(jobData);
         return job;
      }

#endregion

#endregion
   }
}
