// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.Utils
{
   public static class OcrLanguageManager
   {
      private static object _localLanguagesLockObject = new object();
      private static object _downloadableLanguagesLockObject = new object();
      private static List<string> _embeddedLanguageFiles = null;
      private static string _downloadLanguagesDirectory = string.Empty;

      public static readonly string VersionKeyString = "Version";
      public static readonly string FileSizeKeyString = "FileSize";

      public static void Init(string appDirectory, string ocrRuntimeDirectory, List<string> embeddedLanguageFiles)
      {
         ApplicationDirectory = appDirectory;
         LanguagesDirectory = ocrRuntimeDirectory;
         _embeddedLanguageFiles = embeddedLanguageFiles;
         _downloadLanguagesDirectory = Path.Combine(appDirectory, "DownloadedLanguages");
         if (!Directory.Exists(_downloadLanguagesDirectory))
            LEADDocumentHelper.CreatePath(_downloadLanguagesDirectory);

         UpdateDownloadableLanguagesInfo(null, null);
      }

      private static Dictionary<string, object> _originalDeserializedManifest = null;
      public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> Manifest { get; private set; }

      public static string AdditionalLanguagesURL = "https://demo.leadtools.com/AdditionalOCRLanguages/v1.0";

      public static string ApplicationDirectory { get; private set; }
      public static string LanguagesDirectory { get; private set; }

      private static ObservableCollection<OcrLanguage> _localLanguages = null;
      public static ObservableCollection<OcrLanguage> LocalLanguages
      {
         get
         {
            lock (_localLanguagesLockObject)
            {
               return _localLanguages;
            }
         }
         private set
         {
            lock (_localLanguagesLockObject)
            {
               _localLanguages = value;
            }
         }
      }

      private static ObservableCollection<OcrLanguage> _downloadableLanguages = null;
      public static ObservableCollection<OcrLanguage> DownloadableLanguages
      {
         get
         {
            lock (_downloadableLanguagesLockObject)
            {
               return _downloadableLanguages;
            }
         }
         private set
         {
            lock (_downloadableLanguagesLockObject)
            {
               _downloadableLanguages = value;
            }
         }
      }

      public static Exception DownloadableLanguagesError { get; private set; }

      public delegate void ProgressDelegate(OcrLanguage language, double progress);
      public delegate void CompletionDelegate(OcrLanguage language, Exception error);

      public static bool DownloadLanguage(string language, ProgressDelegate progress, CompletionDelegate completion)
      {
         var localLanguages = LocalLanguages.Where(l => l.Language.Equals(language));
         if (localLanguages.Count() > 0)
            return false;

         var downloadableLanguages = DownloadableLanguages.Where(l => l.Language.Equals(language));
         if (downloadableLanguages.Count() == 0)
            return false;

         Task.Run(() =>
         {
            DownloadLanguageFiles(downloadableLanguages.ElementAt(0).RemotePaths, language, progress, completion);
         });

         return true;
      }

      private static OcrLanguage GetCurrentlyDownloadingLanguage(string url)
      {
         OcrLanguage currentlyDownloadingLanguage = null;
         string identifier = Path.GetFileName(url).Split('.')[1];
         var downloadableLanguages = DownloadableLanguages.Where(l => l.Language.Equals(identifier));
         if (downloadableLanguages.Count() > 0)
            currentlyDownloadingLanguage = downloadableLanguages.ElementAt(0);

         return currentlyDownloadingLanguage;
      }

      private static void DownloadLanguageFiles(Dictionary<string, string> urls, string language, ProgressDelegate progress, CompletionDelegate completion)
      {
         OcrLanguage currentlyDownloadingLanguage = null;
         List<Exception> downloadErrors = new List<Exception>();
         bool cancelled = false;

         try
         {
            if (urls.Count == 0)
            {
               var downloadableLanguages = DownloadableLanguages.Where(l => l.Language.Equals(language));
               if (downloadableLanguages.Count() > 0)
                  currentlyDownloadingLanguage = downloadableLanguages.ElementAt(0);
               completion?.Invoke(currentlyDownloadingLanguage, null);
               return;
            }

            currentlyDownloadingLanguage = GetCurrentlyDownloadingLanguage(urls.Values.ElementAt(0));
            if (currentlyDownloadingLanguage != null)
            {
               if (currentlyDownloadingLanguage.CancellationTokenSource != null)
               {
                  currentlyDownloadingLanguage.CancellationTokenSource.Dispose();
                  currentlyDownloadingLanguage.CancellationTokenSource = null;
               }

               currentlyDownloadingLanguage.CancellationTokenSource = new CancellationTokenSource();
               currentlyDownloadingLanguage.IsDownloading = true;
            }

            long totalSizeToDownload = 0;
            Task task = null;
            Dictionary<string, long> totalBytesReceivedForAllDownloads = new Dictionary<string, long>();

            List<Task> downloadTasks = new List<Task>();
            HttpClientDownloader httpClientDownloader = new HttpClientDownloader();
            httpClientDownloader.DownloadCompleted += (object sender, DownloadCompletedEventArgs e) =>
            {
               if (!e.Cancelled && e.Error == null)
               {
                  string finalPath = Path.Combine(LanguagesDirectory, Path.GetFileName(e.TargetFilePath));
                  MoveFile(e.TargetFilePath, finalPath);
               }
               else if (e.Cancelled)
                  cancelled = true;
               else if (e.Error != null)
               {
                  downloadErrors.Add(e.Error);
               }

               if (cancelled || e.Error != null)
               {
                  // Delete temporary file in case or cancellation or error.
                  DeleteFile(e.TargetFilePath);
               }
            };

            httpClientDownloader.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
            {
               // if error occurred while downloading any of this language files, then abort the rest.
               if (downloadErrors.Count > 0)
               {
                  CancelDownloadForLanguage(currentlyDownloadingLanguage.Language);
                  return;
               }

               if (currentlyDownloadingLanguage != null)
               {
                  if (!currentlyDownloadingLanguage.IsDownloading)
                  {
                     // User cancelled the download of this language
                     return;
                  }

                  long totalBytesReceived = 0;
                  lock (totalBytesReceivedForAllDownloads)
                  {
                     totalBytesReceivedForAllDownloads[e.SourceFileUrl] = e.BytesReceived;

                     foreach (KeyValuePair<string, long> entry in totalBytesReceivedForAllDownloads)
                     {
                        totalBytesReceived += entry.Value;
                     }
                  }

                  currentlyDownloadingLanguage.DownloadPercentage = Math.Round((double)totalBytesReceived / totalSizeToDownload * 100, 2);

                  progress?.Invoke(currentlyDownloadingLanguage, currentlyDownloadingLanguage.DownloadPercentage);
               }
            };

            void StartDownloadTask(string url)
            {
               try
               {
                  if (downloadErrors.Count > 0) return;

                  string targetFilePath = Path.Combine(_downloadLanguagesDirectory, Path.GetFileName(url));
                  task = httpClientDownloader.DownloadFileAsync(url, targetFilePath, currentlyDownloadingLanguage.CancellationTokenSource?.Token);
                  downloadTasks.Add(task);
               }
               catch (Exception ex2)
               {
                  Console.WriteLine(ex2.Message);
               }
            }

            foreach (KeyValuePair<string, string> entry in urls)
            {
               string url = entry.Value;
               string languageFile = Path.GetFileName(url);
               string identifier = languageFile.Split('.')[1];
               object sizeValue = Manifest[identifier][languageFile][FileSizeKeyString];
               if (sizeValue.GetType() == typeof(long))
                  totalSizeToDownload += (long)sizeValue;

               StartDownloadTask(url);
            }

            Task.WaitAll(downloadTasks.ToArray(), -1, (currentlyDownloadingLanguage.CancellationTokenSource != null) ? currentlyDownloadingLanguage.CancellationTokenSource.Token : CancellationToken.None);
            downloadTasks.Clear();

            if (!cancelled)
            {
               DownloadableLanguages.Remove(currentlyDownloadingLanguage);
               LocalLanguages.Add(currentlyDownloadingLanguage);
               //UpdateLanguagesWithCurrentManifest();
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            if (ex.GetType() != typeof(TaskCanceledException) && ex.GetType() != typeof(OperationCanceledException) && currentlyDownloadingLanguage != null)
               downloadErrors.Add(ex);
         }
         finally
         {
            Exception error = null;
            currentlyDownloadingLanguage.IsDownloading = false;

            var errors = downloadErrors.Where(e => e != null);
            if (errors != null && errors.Count() > 0)
               error = errors.ElementAt(0);

            if(error != null)
            {
               DownloadableLanguagesError = error;
               currentlyDownloadingLanguage.DownloadPercentage = 0;
            }
            else
            {
               currentlyDownloadingLanguage.DownloadPercentage = 100;
            }

            if (!cancelled)
               completion?.Invoke(currentlyDownloadingLanguage, error);
         }
      }

      public static void UpdateDownloadableLanguagesInfo(string ocrLanguagesManifestFileLocalPath, CompletionDelegate completion)
      {
         DownloadableLanguagesError = null;
         List<Task> downloadTasks = new List<Task>();

         Task task = Task.Run(async() =>
         {
            string[] directories = { LanguagesDirectory };

            void ParseManifestJsonText(string jsonContent)
            {
               _originalDeserializedManifest = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
               if (_originalDeserializedManifest != null)
                  Manifest = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(_originalDeserializedManifest.Count);

               foreach (KeyValuePair<string, object> entry in _originalDeserializedManifest)
               {
                  if (entry.Value.GetType() != typeof(string))
                  {
                     // Not the "Version" tag
                     Dictionary<string, Dictionary<string, object>> val = (JToken.FromObject(entry.Value)).ToObject(typeof(Dictionary<string, Dictionary<string, object>>)) as Dictionary<string, Dictionary<string, object>>;
                     Manifest.Add(entry.Key, val);
                  }
               }

               DownloadableLanguagesError = Manifest != null ? null : new Exception("Error parsing OCR languages manifest file");

               if (Manifest == null)
                  Manifest = GenerateTemporaryManifestWithLanguageFilesInDirectories(directories);

               UpdateLanguagesWithCurrentManifest();

               completion?.Invoke(null, DownloadableLanguagesError);
            }

            if(!string.IsNullOrWhiteSpace(ocrLanguagesManifestFileLocalPath))
            {
               // Read from the embedded resource
               using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ocrLanguagesManifestFileLocalPath))
               using (StreamReader reader = new StreamReader(stream))
               {
                  string jsonContent = reader.ReadToEnd();
                  ParseManifestJsonText(jsonContent);
               }
            }
            else if (AdditionalLanguagesURL != null)
            {
               string url = Path.Combine(AdditionalLanguagesURL, "Manifest.json");
               string fileDownloadLocation = Path.Combine(_downloadLanguagesDirectory, Path.GetFileName(url));

               HttpClientDownloader httpClientDownloader = new HttpClientDownloader();
               httpClientDownloader.DownloadCompleted += (object sender, DownloadCompletedEventArgs e) =>
               {
                  if (!string.IsNullOrWhiteSpace(fileDownloadLocation) && File.Exists(fileDownloadLocation))
                  {
                     string jsonContent = File.ReadAllText(fileDownloadLocation);
                     ParseManifestJsonText(jsonContent);
                  }
                  else
                  {
                     Manifest = null;
                     DownloadableLanguagesError = e.Error;
                  }
               };

               try
               {
                  await httpClientDownloader.DownloadFileAsync(url, fileDownloadLocation, null);
               }
               catch (Exception ex)
               {
                  Console.WriteLine(ex.Message);
               }
            }
            else
            {
               Manifest = GenerateTemporaryManifestWithLanguageFilesInDirectories(directories);
            }
         });

         if (!string.IsNullOrWhiteSpace(ocrLanguagesManifestFileLocalPath))
         {
            downloadTasks.Add(task);
            Task.WaitAll(downloadTasks.ToArray(), -1);
         }
      }

      private static Dictionary<string, Dictionary<string, Dictionary<string, object>>> GenerateTemporaryManifestWithLanguageFilesInDirectories(string[] directories)
      {
         Dictionary<string, Dictionary<string, Dictionary<string, object>>> manifest = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

         string[] searchPatterns = "*.bin,*.bin2,*.dic".Split(',');
         foreach (string directory in directories)
         {
            List<string> files = new List<string>();
            foreach (string searchPattern in searchPatterns)
            {
               files.AddRange(Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly));
            }

            foreach (string file in files)
            {
               if (Path.GetFileName(file).StartsWith("LEAD.") && Path.GetFileName(file).Split('.').Length == 3)
               {
                  string languageFile = Path.GetFileName(file);
                  string languageID = languageFile.Split('.')[1];

                  // Non-language engine files
                  if (languageID.Equals("micr", StringComparison.OrdinalIgnoreCase) || languageID.Equals("mrz", StringComparison.OrdinalIgnoreCase) || languageID.Equals("field", StringComparison.OrdinalIgnoreCase) || languageID.Equals("cmc7", StringComparison.OrdinalIgnoreCase))
                     continue;

                  if (!manifest.Keys.Contains(languageID))
                  {
                     manifest.Add(languageID, new Dictionary<string, Dictionary<string, object>>());
                     manifest[languageID].Add(languageFile, new Dictionary<string, object>());
                  }

                  if (!manifest[languageID].Keys.Contains(languageFile))
                     manifest[languageID].Add(languageFile, new Dictionary<string, object>());

                  FileInfo fileInfo = new FileInfo(file);
                  if (fileInfo != null)
                     manifest[languageID][languageFile][FileSizeKeyString] = fileInfo.Length;
               };
            }
         }

         return manifest;
      }

      private static void UpdateLanguagesWithCurrentManifest()
      {
         if (_originalDeserializedManifest == null && Manifest == null) return;

         string versionString = (_originalDeserializedManifest != null) ? (string)_originalDeserializedManifest[VersionKeyString] : "1.0";
         ObservableCollection<OcrLanguage> localLanguages = null, downloadableLanguages = null;

         if (versionString.Equals("1.0"))
         {
            OcrLanguageManifestVersion1.ParseManifest(Manifest, out localLanguages, out downloadableLanguages);

            _localLanguages = localLanguages;
            _downloadableLanguages = downloadableLanguages;
         }
         else
         {
            DownloadableLanguagesError = new Exception("Unsupported language manifest version. Please download the latest OCR App from the store.");

            string[] directories = { LanguagesDirectory };
            Manifest = GenerateTemporaryManifestWithLanguageFilesInDirectories(directories);
            OcrLanguageManifestVersion1.ParseManifest(Manifest, out localLanguages, out downloadableLanguages);

            _localLanguages = localLanguages;
            _downloadableLanguages = downloadableLanguages;
         }
      }

      public static bool CancelDownloadForLanguage(string language)
      {
         bool cancelledDownload = false;

         lock (_downloadableLanguagesLockObject)
         {
            var downloadableLanguages = _downloadableLanguages.Where(l => l.Language.Equals(language));
            foreach (OcrLanguage lang in downloadableLanguages)
            {
               cancelledDownload = true;
               lang.CancellationTokenSource.Cancel();
               lang.IsDownloading = false;
               lang.DownloadPercentage = 0;
            }
         }

         return cancelledDownload;
      }

      private static List<string> LanguageFileURLsForLanguage(string language)
      {
         List<string> languageFileURLs = new List<string>();

         foreach (OcrLanguage localLanguage in LocalLanguages)
         {
            if (localLanguage.Language == language)
               languageFileURLs.AddRange(localLanguage.LocalPaths.Values);
         }

         return languageFileURLs;
      }

      public static bool CanDeleteLanguage(string language)
      {
         var localLanguages = _localLanguages.Where(l => l.Language.Equals(language));
         if (localLanguages.Count() > 0)
         {
            OcrLanguage ocrLanguage = localLanguages.ElementAt(0);
            if (ocrLanguage.LocalPaths != null && ocrLanguage.LocalPaths.Count > 0)
            {
               if(_embeddedLanguageFiles != null)
               {
                  foreach (KeyValuePair<string, string> entry in ocrLanguage.LocalPaths)
                  {
                     var embeddedFiles = _embeddedLanguageFiles.Where(e => Path.GetFileNameWithoutExtension(e).Equals(Path.GetFileNameWithoutExtension(entry.Key), StringComparison.OrdinalIgnoreCase));
                     if (embeddedFiles != null && embeddedFiles.Count() > 0)
                     {
                        // This language is an embedded language and not downloaded, so it can't be deleted
                        return false;
                     }
                  }
               }
            }
         }

         return true;
      }

      public static void DeleteLanguage(string language)
      {
         List<string> languageFileURLs = LanguageFileURLsForLanguage(language);

         if (languageFileURLs.Count > 0)
         {
            bool canDeleteLanguage = CanDeleteLanguage(language);
            if (canDeleteLanguage)
            {
               foreach (string languageFileURL in languageFileURLs)
               {
                  DeleteFile(languageFileURL);
               }

               UpdateLanguagesWithCurrentManifest();
            }
         }
      }

      private static async void MoveFile(string sourceFilePath, string targetFilePath)
      {
         try
         {
            if (File.Exists(sourceFilePath))
            {
               await DependencyService.Get<IFileManager>().MoveFile(sourceFilePath, targetFilePath);
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private static async void DeleteFile(string filePath)
      {
         await DependencyService.Get<IFileManager>().DeleteFile(filePath);
      }

      private static class OcrLanguageManifestVersion1
      {
         public static void ParseManifest(Dictionary<string, Dictionary<string, Dictionary<string, object>>> manifest, out ObservableCollection<OcrLanguage> localLanguages, out ObservableCollection<OcrLanguage> downloadableLanguages)
         {
            localLanguages = new ObservableCollection<OcrLanguage>();
            downloadableLanguages = new ObservableCollection<OcrLanguage>();

            foreach (string languageID in manifest.Keys)
            {
               OcrLanguage language = new OcrLanguage(languageID);

               if (language.FileSize == 0)
                  localLanguages.Add(language);
               else
               {
                  language.IsDownloadable = true;
                  downloadableLanguages.Add(language);
               }
            }
         }
      }
   }
}
