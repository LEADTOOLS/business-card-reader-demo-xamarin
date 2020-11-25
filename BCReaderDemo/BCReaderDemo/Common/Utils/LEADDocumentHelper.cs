// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Caching;
using Leadtools.Codecs;
using Leadtools.Document;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public static class LEADDocumentHelper
   {
      private static string ApplicationDirectory;
      private static string CacheDirectory;
      private static string DocumentsSerializationPath;

      public static FileCache CacheObject = null;

      public static void Init(string appDirectory, string cacheDirectory, string documentsSerializationPath)
      {
         DocumentsSerializationPath = documentsSerializationPath;
         ApplicationDirectory = appDirectory;
         CacheDirectory = cacheDirectory;
         if(!string.IsNullOrWhiteSpace(CacheDirectory))
            CacheObject = CreateCacheObject(CacheDirectory);
      }

      public static LEADDocument CreateDocument(List<string> files, Stream stream, LEADDocument userDocumentToAppendTo, out List<Exception> errors)
      {
         errors = new List<Exception>();
         // If we are loading from single file then don't create virtual document, just create it directly from file
         LEADDocument virtualDocument = userDocumentToAppendTo ?? ((stream != null || (files != null && files.Count == 1)) ? null : CreateVirtualDocument());

         // Setup the document load options
         var options = new LoadDocumentOptions();
         options.Cache = CacheObject;
         options.UseCache = !string.IsNullOrWhiteSpace(CacheDirectory);
         options.MayHaveDifferentPageSizes = true;

         if (files != null)
         {
            int numberOfLoadedFiles = 0;
            string annotationsFileName = string.Empty;
            foreach (string imagePath in files)
            {
               string filePath = imagePath;

               try
               {
                  // Check source file size, if zero then ignore it, will return error later.
                  FileInfo fi = new FileInfo(filePath);
                  if (fi == null || fi.Length == 0)
                     continue;

                  numberOfLoadedFiles++;
                  if (Device.RuntimePlatform == Device.iOS)
                  {
                     // The only way to keep the file after loading it from photo gallery on iOS is to save it somewhere else, so copy these files to Cache directory.
                     // only do this for the files that came from gallery and not the already converted document files.
                     string documentsDirectory = Path.Combine(ApplicationDirectory, "Documents");
                     if (!string.IsNullOrWhiteSpace(CacheDirectory) && !Path.GetDirectoryName(filePath).Equals(documentsDirectory, StringComparison.OrdinalIgnoreCase) && File.Exists(filePath))
                     {
                        string backupFilePath = Path.Combine(CacheDirectory, Path.GetFileName(filePath));
                        File.Copy(filePath, backupFilePath, true);
                        filePath = backupFilePath;
                     }
                  }

                  if(string.IsNullOrWhiteSpace(annotationsFileName))
                  {
                     if (filePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                     {
                        // In case of multipage SVG files we need to get the document name from the file names
                        string directory = Path.GetDirectoryName(filePath);
                        string docName = Path.GetFileNameWithoutExtension(filePath);
                        int svgMultiPageSuffixIndex = docName.IndexOf("_Page(");
                        // Check if we have multipage suffix "_Page(X)", if not then this is a single page SVG and we should use original SVG file name
                        if (svgMultiPageSuffixIndex != -1)
                           docName = docName.Substring(0, docName.IndexOf("_Page("));

                        annotationsFileName = Path.Combine(directory, $"{docName}.xml");
                     }
                     else
                        annotationsFileName = Path.ChangeExtension(filePath, "xml");
                  }

                  if(File.Exists(annotationsFileName))
                     options.AnnotationsUri = new Uri(annotationsFileName);

                  LEADDocument childDocument = null;
                  if (virtualDocument == null)
                  {
                     virtualDocument = DocumentFactory.LoadFromFile(filePath, options);
                     childDocument = virtualDocument;
                  }
                  else
                     childDocument = DocumentFactory.LoadFromFile(filePath, options);

                  if (childDocument != null)
                  {
                     childDocument.IsReadOnly = false;
                     childDocument.AutoSaveToCache = true;
                     childDocument.AutoDeleteFromCache = false;
                     if(virtualDocument.DataType == DocumentDataType.Virtual || virtualDocument.DataType == DocumentDataType.Cached)
                        virtualDocument.Pages.AddRange(childDocument.Pages);
                  }
               }
               catch (Exception ex)
               {
                  System.Diagnostics.Debug.WriteLine(ex.Message);
                  try
                  {
                     if (File.Exists(filePath))
                        File.Delete(filePath);
                  }
                  catch (Exception) { }
                  errors.Add(ex);
               }
            }

            if(numberOfLoadedFiles == 0)
               errors.Add(new Exception("Source document has no recognized text"));
         }
         else if (stream != null)
         {
            LEADDocument childDocument = null;
            if (virtualDocument == null)
            {
               virtualDocument = DocumentFactory.LoadFromStream(stream, options);
               childDocument = virtualDocument;
            }
            else
               childDocument = DocumentFactory.LoadFromStream(stream, options);

            if(childDocument != null)
            {
               childDocument.IsReadOnly = false;
               childDocument.AutoSaveToCache = true;
               childDocument.AutoDeleteFromCache = false;
               if (virtualDocument.DataType == DocumentDataType.Virtual || virtualDocument.DataType == DocumentDataType.Cached)
                  virtualDocument.Pages.AddRange(childDocument.Pages);
            }
         }

         if (virtualDocument != null)
         {
            virtualDocument.Name = string.Empty;
            if(options.UseCache)
               virtualDocument.SaveToCache();
         }
         return virtualDocument;
      }

      public static void SaveDocument(LEADDocument document, ObservableCollection<DocumentItemData> documentsCollection, OcrOutputFormat format = OcrOutputFormat.None, int replaceItemAtIndex = -1, bool autoSerializeDocuments = true)
      {
         string thumbnailPath = Path.Combine(CacheDirectory, document.DocumentId, $"thumbnail.jpeg");

         // Create new document item data
         DateTime currentDatetime = DateTime.Now;
         if (string.IsNullOrWhiteSpace(document.Name))
         {
            if (replaceItemAtIndex == -1) // we are adding new document and not replacing existing one, so use the old document name
               document.Name = FindUniqueDocumentName(documentsCollection);
            else
               document.Name = documentsCollection[replaceItemAtIndex].Title;
         }

         string annotationsFilePath = Path.Combine(ApplicationDirectory, "Documents", $"{document.Name}.xml");

         DocumentItemData newDocumentItemData = new DocumentItemData();
         newDocumentItemData.DocumentId = document.DocumentId;
         newDocumentItemData.DocumentThumbnailPath = thumbnailPath;
         newDocumentItemData.AnnotationsFilePath = annotationsFilePath;
         newDocumentItemData.Title = document.Name;
         newDocumentItemData.Date = currentDatetime;
         newDocumentItemData.DocumentFormat = format;

         if (Device.RuntimePlatform == Device.iOS)
         {
            // Save the original file path that was loaded from gallery, so we can delete it later when we convert this document in 
            // order to save space since we won't need this file anymore.
            // only do this for the newly created documents and not the already converted ones.
            if (format == OcrOutputFormat.None)
               newDocumentItemData.DocumentFiles = GetDocumentSourceFiles(document);
         }

         IEnumerable<DocumentItemData> items = documentsCollection.Where(x => x.DocumentId.Equals(document.DocumentId));
         if ((items != null && items.Count() > 0) || replaceItemAtIndex != -1)
         {
            // Document already exist, so just update it
            int index = (replaceItemAtIndex != -1) ? replaceItemAtIndex : documentsCollection.IndexOf(items.ElementAt(0));
            documentsCollection.Insert(index, newDocumentItemData);
            documentsCollection.RemoveAt(index + 1);
         }
         else
         {
            documentsCollection.Add(newDocumentItemData);
         }

         if (autoSerializeDocuments)
            SerializeDocuments(documentsCollection);

         document.SaveToCache();

         if (document.Pages != null && document.Pages.Count > 0)
         {
            using (var codecs = new RasterCodecs())
            {
               try
               {
                  LeadSize thumbnailSize = LeadSize.Create((int)(document.Images.ThumbnailPixelSize.Width * 1.5), (int)(document.Images.ThumbnailPixelSize.Height * 1.5));
                  using (var rasterImage = document.Pages[0].GetImage())
                  {
                     var thumbnailImage = rasterImage.CreateThumbnail(thumbnailSize.Width, thumbnailSize.Height, 32, RasterViewPerspective.TopLeft, RasterSizeFlags.Bicubic);
                     if (thumbnailImage == null)
                        thumbnailImage = document.Pages[0].GetThumbnailImage();
                     codecs.Save(thumbnailImage, thumbnailPath, RasterImageFormat.Jpeg, 0);
                  }
               }
               catch (Exception ex)
               {
                  Console.WriteLine(ex.Message);
                  codecs.Save(document.Pages[0].GetThumbnailImage(), thumbnailPath, RasterImageFormat.Jpeg, 0);
               }
            }
         }
      }

      public static void SerializeDocuments(ObservableCollection<DocumentItemData> documentsCollection)
      {
         if (string.IsNullOrWhiteSpace(DocumentsSerializationPath)) return;

         XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<DocumentItemData>));

         using (StreamWriter writer = new StreamWriter(DocumentsSerializationPath))
         {
            if (documentsCollection != null && documentsCollection.Count > 0)
            {
               serializer.Serialize(writer, documentsCollection);
            }
         }
      }

      public static ObservableCollection<DocumentItemData> DeserializeDocuments()
      {
         ObservableCollection<DocumentItemData> documents = new ObservableCollection<DocumentItemData>();

         if (string.IsNullOrWhiteSpace(DocumentsSerializationPath)) return null;

         try
         {
            if (File.Exists(DocumentsSerializationPath))
            {
               XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<DocumentItemData>));
               using (StreamReader reader = new StreamReader(DocumentsSerializationPath))
               {
                  documents = (ObservableCollection<DocumentItemData>)serializer.Deserialize(reader);
                  if (documents != null)
                  {
                     if (Device.RuntimePlatform == Device.iOS)
                     {
                        // The documents were saved with previous CacheDirectory path which contains unique GUID in the path which won't
                        // exist when load it from another app instance, so we need to replace the previously saved directory with
                        // the current session CacheDirectory path.
                        foreach (DocumentItemData documentItemData in documents)
                        {
                           string thumbnailPath = Path.Combine(CacheDirectory, documentItemData.DocumentId, $"thumbnail.jpeg");
                           documentItemData.DocumentThumbnailPath = thumbnailPath;
                           for (int i = 0; i < documentItemData.DocumentFiles.Count; i++)
                           {
                              string newDocFilePath = Path.Combine(CacheDirectory, Path.GetFileName(documentItemData.DocumentFiles[i]));
                              documentItemData.DocumentFiles[i] = newDocFilePath;
                              if(i == 0)
                              {
                                 // Do it once
                                 string annotationsFilePath = Path.Combine(ApplicationDirectory, "Documents", $"{documentItemData.Title}.xml");
                                 documentItemData.AnnotationsFilePath = annotationsFilePath;
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         return documents;
      }

      public static LEADDocument LoadFromCache(string documentId)
      {
         LoadFromCacheOptions options = new LoadFromCacheOptions() { Cache = CacheObject, DocumentId = documentId };
         LEADDocument document = DocumentFactory.LoadFromCache(options);
         return document;
      }

      public static bool DeleteFromCache(string documentId)
      {
         bool result = true;
         try
         {
            List<string> allImbeddedDocumentsIds = new List<string>();
            LoadFromCacheOptions options = new LoadFromCacheOptions() { Cache = CacheObject, DocumentId = documentId };
            LEADDocument document = DocumentFactory.LoadFromCache(options);
            if (document != null)
            {
               allImbeddedDocumentsIds.Add(documentId);
               if (document.Documents != null)
               {
                  foreach (var embeddedDocument in document.Documents)
                  {
                     allImbeddedDocumentsIds.Add(embeddedDocument.DocumentId);
                  }
               }
            }

            foreach (var docId in allImbeddedDocumentsIds)
            {
               options.DocumentId = docId;
               DocumentFactory.DeleteFromCache(options);

               try
               {
                  string cachedDocumentFolderPath = Path.Combine(CacheDirectory, docId);
                  if (Directory.Exists(cachedDocumentFolderPath))
                     Directory.Delete(cachedDocumentFolderPath, true);
               }
               catch(Exception ex1)
               {
                  Console.WriteLine(ex1.Message);
               }
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
            result = false;
         }

         return result;
      }

      public static async Task<string> ShareDocument(LEADDocument document, string documentId, string sharedFileTitle)
      {
         string retTempFilePath = string.Empty;
         string sharedfilePath = string.Empty;

         LoadFromCacheOptions options = new LoadFromCacheOptions() { Cache = CacheObject, DocumentId = documentId };
         LEADDocument sourceDocument = (document != null) ? document : DocumentFactory.LoadFromCache(options);
         if (sourceDocument != null)
         {
            sharedfilePath = sourceDocument.GetDocumentFileName();
            if (string.IsNullOrWhiteSpace(sharedfilePath))
            {
               List<string> sourceDocumentFiles = GetDocumentSourceFiles(sourceDocument);
               if (sourceDocumentFiles.Count > 1)
               {
                  sharedfilePath = Path.Combine(ApplicationDirectory, "Temp", (!string.IsNullOrWhiteSpace(sharedFileTitle) ? sharedFileTitle : sourceDocument.Name) + ".zip");
                  if (!Directory.Exists(Path.GetDirectoryName(sharedfilePath)))
                     Directory.CreateDirectory(Path.GetDirectoryName(sharedfilePath));

                  retTempFilePath = sharedfilePath;
                  CompressFiles(sourceDocumentFiles, sharedfilePath);
               }
            }
            else if (!string.IsNullOrWhiteSpace(sharedFileTitle) && sharedFileTitle.CompareTo(Path.GetFileNameWithoutExtension(sharedfilePath)) != 0)
            {
               retTempFilePath = Path.Combine(ApplicationDirectory, "Temp", sharedFileTitle + Path.GetExtension(sharedfilePath));
               if (!Directory.Exists(Path.GetDirectoryName(retTempFilePath)))
                  Directory.CreateDirectory(Path.GetDirectoryName(retTempFilePath));
               File.Copy(sharedfilePath, retTempFilePath, true);
               sharedfilePath = retTempFilePath;
            }

            await Device.InvokeOnMainThreadAsync(async() =>
            {
               await Share.RequestAsync(new ShareFileRequest()
               {
                  Title = (!string.IsNullOrWhiteSpace(sharedFileTitle)) ? sharedFileTitle : Path.GetFileName(sharedfilePath),
                  File = new ShareFile(sharedfilePath),
                  // for iOS iPad running iOS 13.4 or later we need to set the PresentationSourceBounds otherwise the share dialog won't show up.
                  PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? new System.Drawing.Rectangle(0, 20, 0, 0) : System.Drawing.Rectangle.Empty
               });
            });
         }

         // only return temporarily created file so the user can delete it later, otherwise return null if we are using the original document file path.
         return retTempFilePath;
      }

      public static async Task<string> ShareDocuments(List<string> documentsIds)
      {
         string retTempFilePath = string.Empty;
         List<string> documentsUnderlyingFiles = new List<string>();

         foreach(var documentId in documentsIds)
         {
            LoadFromCacheOptions options = new LoadFromCacheOptions() { Cache = CacheObject, DocumentId = documentId };
            LEADDocument sourceDocument = DocumentFactory.LoadFromCache(options);
            if (sourceDocument != null)
            {
               string sharedfilePath = sourceDocument.GetDocumentFileName();
               if (string.IsNullOrWhiteSpace(sharedfilePath))
               {
                  foreach(var subDocument in sourceDocument.Documents)
                  {
                     sharedfilePath = subDocument.GetDocumentFileName();
                     if (!string.IsNullOrWhiteSpace(sharedfilePath))
                        documentsUnderlyingFiles.Add(sharedfilePath);
                  }
               }
               else
               {
                  documentsUnderlyingFiles.Add(sharedfilePath);
               }
            }
         }

         if (documentsUnderlyingFiles.Count == 0)
            return retTempFilePath;
         else if (documentsUnderlyingFiles.Count == 1)
            retTempFilePath = documentsUnderlyingFiles[0];
         else
         {
            retTempFilePath = Path.Combine(ApplicationDirectory, "Temp", "Documents.zip");
            if (!Directory.Exists(Path.GetDirectoryName(retTempFilePath)))
               Directory.CreateDirectory(Path.GetDirectoryName(retTempFilePath));

            CompressFiles(documentsUnderlyingFiles, retTempFilePath);
         }

         await Device.InvokeOnMainThreadAsync(async () =>
         {
            await Share.RequestAsync(new ShareFileRequest()
            {
               Title = Path.GetFileName(retTempFilePath),
               File = new ShareFile(retTempFilePath),
               // for iOS iPad running iOS 13.4 or later we need to set the PresentationSourceBounds otherwise the share dialog won't show up.
               PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? new System.Drawing.Rectangle(0, 20, 0, 0) : System.Drawing.Rectangle.Empty
            });
         });

         // only return temporarily created file so the user can delete it later, otherwise return null if we are using the original document file path.
         return (documentsUnderlyingFiles.Count > 1) ? retTempFilePath : string.Empty;
      }

      public static LEADDocument ReplaceDocument(string oldDocumentID, List<string> newDocumentFiles, ObservableCollection<DocumentItemData> documentsCollection, int oldDocumentIndex, out Exception error)
      {
         error = null;
         var items = documentsCollection.Where(x => x.DocumentId.Equals(oldDocumentID));
         if (items == null || items.Count() == 0)
         {
            error = new Exception($"No document found with ID {oldDocumentID}");
            return null;
         }

         DocumentItemData oldItemData = items.ElementAt(0);

         DateTime oldDocumentDate = oldItemData.Date;
         string oldDocumentTitle = oldItemData.Title;
         bool oldIsSelectedState = oldItemData.IsSelected;
         OcrOutputFormat oldDocumentFormat = oldItemData.DocumentFormat;

         List<Exception> errors = null;
         LEADDocument convertedDocument = CreateDocument(newDocumentFiles, null, null, out errors);
         if (convertedDocument != null && File.Exists(newDocumentFiles[0]))
         {
            OcrOutputFormat format = GetDocumentFormatFromExt(newDocumentFiles[0]);
            SaveDocument(convertedDocument, documentsCollection, format, oldDocumentIndex);

            items = documentsCollection.Where(x => x.DocumentId.Equals(convertedDocument.DocumentId));
            DocumentItemData newItemData = null;
            if (items != null && items.Count() > 0)
            {
               newItemData = items.ElementAt(0);
               // Keep the old item name and date since we are sorting documents by date and we don't want to change the converted item position.
               newItemData.Date = oldDocumentDate;
               newItemData.Title = oldDocumentTitle;
               newItemData.IsSelected = oldIsSelectedState;

               // Update the document files inside the "Documents" folder of this item in order to delete them later when we convert this document to another format.
               newItemData.DocumentFiles = new List<string>(newDocumentFiles);
               SerializeDocuments(documentsCollection);
            }

            // Delete the old document from cache and create new LEADDocument out of the converted file and replace the converted item in the documents list.
            DeleteFromCache(oldDocumentID);

            // Delete the old document files that built this document object from the "Documents" folder.
            if (newItemData != null && oldItemData.DocumentFiles != null && oldItemData.DocumentFiles.Count > 0)
            {
               foreach (string docFile in oldItemData.DocumentFiles)
               {
                  try
                  {
                     if (!newItemData.DocumentFiles.Contains(docFile) && File.Exists(docFile))
                        File.Delete(docFile);
                  }
                  catch (Exception) { }
               }
            }
         }
         else
         {
            if (errors != null && errors.Count > 0)
               error = errors[0];
         }

         return convertedDocument;
      }

      public static OcrOutputFormat GetDocumentFormatFromExt(string documentPath)
      {
         OcrOutputFormat sourceDocumentFormat = OcrOutputFormat.None;

         if (!string.IsNullOrWhiteSpace(documentPath))
         {
            string ext = Path.GetExtension(documentPath);
            if (ext.ToLower().Contains("pdf"))
               sourceDocumentFormat = OcrOutputFormat.Pdf;
            else if (ext.ToLower().Contains("doc"))
               sourceDocumentFormat = OcrOutputFormat.Docx;
            else if (ext.ToLower().Contains("rtf"))
               sourceDocumentFormat = OcrOutputFormat.Rtf;
            else if (ext.ToLower().Contains("svg"))
               sourceDocumentFormat = OcrOutputFormat.Svg;
            else if (ext.ToLower().Contains("txt"))
               sourceDocumentFormat = OcrOutputFormat.Text;
         }

         return sourceDocumentFormat;
      }

      public static string CompressFiles(List<string> files, string zippedFilePath)
      {
         string tempDocumentFolderName = Path.GetFileNameWithoutExtension(files[0]);
         int namingTemplateStartIndex = tempDocumentFolderName.LastIndexOf("_");
         tempDocumentFolderName = tempDocumentFolderName.Substring(0, (namingTemplateStartIndex >= 0) ? namingTemplateStartIndex : 0);
         string tempFilesDirectory = Path.Combine(ApplicationDirectory, "Temp", !string.IsNullOrWhiteSpace(tempDocumentFolderName) ? tempDocumentFolderName : "TempDoc");
         if (!Directory.Exists(tempFilesDirectory))
            CreatePath(tempFilesDirectory);

         string compressedFilesPath = (!string.IsNullOrWhiteSpace(zippedFilePath)) ? zippedFilePath : Path.Combine(tempFilesDirectory, "Files.zip");
         try
         {
            if (File.Exists(compressedFilesPath))
               File.Delete(compressedFilesPath);
         }
         catch (Exception) { }

         foreach (string file in files)
         {
            if (File.Exists(file))
               File.Copy(file, Path.Combine(tempFilesDirectory, Path.GetFileName(file)), true);
         }

         ZipFile.CreateFromDirectory(tempFilesDirectory, compressedFilesPath);

         // Delete the temp files directory and the files inside it after compression is complete
         Directory.Delete(tempFilesDirectory, true);

         return compressedFilesPath;
      }

      public static void CreatePath(string path)
      {
         if (string.IsNullOrEmpty(path)) return;

         string[] pathComponents = path.Split('/');
         string builtPath = "/";
         foreach(var component in pathComponents)
         {
            if (string.IsNullOrWhiteSpace(component)) continue;

            try
            {
               builtPath = Path.Combine(builtPath, component);
               if (!Directory.Exists(builtPath))
                  Directory.CreateDirectory(builtPath);
            }
            catch(Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
         }
      }

      private static FileCache CreateCacheObject(string baseCacheDirectory)
      {
         if (!string.IsNullOrWhiteSpace(baseCacheDirectory) && !Directory.Exists(baseCacheDirectory))
            Directory.CreateDirectory(baseCacheDirectory);

         var cacheObject = new FileCache();
         cacheObject.CacheDirectory = baseCacheDirectory;
         return cacheObject;
      }

      public static LEADDocument CreateVirtualDocument()
      {
         LEADDocument virtualDocument = DocumentFactory.Create(new CreateDocumentOptions() { UseCache = true, Cache = CacheObject });

         virtualDocument.AutoDisposeDocuments = true;
         virtualDocument.AutoSaveToCache = true;
         virtualDocument.AutoDeleteFromCache = false;
         virtualDocument.IsReadOnly = false;

         return virtualDocument;
      }

      private static string FindUniqueDocumentName(ObservableCollection<DocumentItemData> documentsCollection)
      {
         string documentName = string.Empty;

         int docNumber = 1;
         while(true)
         {
            string testName = $"New Doc {docNumber}";
            IEnumerable<DocumentItemData> items = documentsCollection.Where(x => x.Title.Equals(testName));
            if (items == null || items.Count() == 0)
            {
               // this document name is unique, take it
               documentName = testName;
               break;
            }

            docNumber++;
         }

         return documentName;
      }

      private static List<string> GetDocumentSourceFiles(LEADDocument sourceDocument)
      {
         List<string> sourceDocumentFiles = new List<string>();

         string sourceDocumentFilePath = sourceDocument.GetDocumentFileName();
         if (!string.IsNullOrWhiteSpace(sourceDocumentFilePath))
            sourceDocumentFiles.Add(sourceDocumentFilePath);
         else
         {
            if (sourceDocument.Documents != null && sourceDocument.Documents.Count > 0)
            {

               foreach (var subDocument in sourceDocument.Documents)
               {
                  var documentFilePath = subDocument.GetDocumentFileName();
                  if (!string.IsNullOrWhiteSpace(documentFilePath))
                     sourceDocumentFiles.Add(documentFilePath);
               }
            }
         }

         return sourceDocumentFiles;
      }
   }

   [XmlRoot]
   public class DocumentItemData : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public string DocumentId { get; set; }
      public string DocumentThumbnailPath { get; set; }
      public string AnnotationsFilePath { get; set; }
      public string Title { get; set; }
      public DateTime Date { get; set; }
      public List<string> DocumentFiles { get; set; }

      private OcrOutputFormat _documentFormat;
      public OcrOutputFormat DocumentFormat
      {
         get
         {
            return _documentFormat;
         }
         set
         {
            _documentFormat = value;
         }
      }

      [XmlIgnore]
      public string FormattedDate
      {
         get
         {
            string date = Date.ToString($"MMMM, d yyyy").ToUpper();
            return date;
         }
      }

      // for internal use by the DocumentsList control
      [XmlIgnore]
      private Color _itemBorderColor = Color.Transparent;
      public Color ItemBorderColor
      {
         get
         {
            return _itemBorderColor;
         }
         set
         {
            _itemBorderColor = value;
            NotifyPropertyChanged();
         }
      }

      [XmlIgnore]
      private bool _isSelected = false;
      public bool IsSelected
      {
         get
         {
            return _isSelected;
         }
         set
         {
            _isSelected = value;
            NotifyPropertyChanged();
         }
      }

      [XmlIgnore]
      public string DocumentFormatResourceName
      {
         get
         {
            switch (DocumentFormat)
            {
               default:
               case OcrOutputFormat.None:
                  return "Icons/none-file.svg";
               case OcrOutputFormat.Docx:
               case OcrOutputFormat.DocxFramed:
                  return "Icons/doc-file.svg";
               case OcrOutputFormat.Pdf:
               case OcrOutputFormat.PdfA:
               case OcrOutputFormat.PdfAImageOverText:
               case OcrOutputFormat.PdfEmbeddedFonts:
               case OcrOutputFormat.PdfImageOverText:
               case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
                  return "Icons/pdf-file.svg";
               case OcrOutputFormat.Rtf:
               case OcrOutputFormat.RtfFramed:
                  return "Icons/rtf-file.svg";
               case OcrOutputFormat.Svg:
                  return "Icons/svg-file.svg";
               case OcrOutputFormat.Text:
               case OcrOutputFormat.TextFormatted:
                  return "Icons/txt-file.svg";
            }
         }
      }

      public DocumentItemData Clone()
      {
         DocumentItemData temp = new DocumentItemData();

         temp.DocumentId = this.DocumentId;
         temp.DocumentThumbnailPath = this.DocumentThumbnailPath;
         temp.AnnotationsFilePath = this.AnnotationsFilePath;
         temp.Title = this.Title;
         temp.Date = this.Date;

         return temp;
      }

      private string GetDayOrdinal(int number)
      {
         var ones = number % 10;
         var tens = Math.Floor(number / 10f) % 10;
         if (tens == 1)
         {
            return number + "th";
         }

         switch (ones)
         {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
         }
      }
   }
}
