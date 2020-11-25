// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class OcrLanguage : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public OcrLanguage()
      {
      }

      public OcrLanguage(string language)
      {
         try
         {
            Dictionary<string, Dictionary<string, object>> languageManifest = (OcrLanguageManager.Manifest != null && !string.IsNullOrWhiteSpace(language)) ? OcrLanguageManager.Manifest[language] : new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, string> remotePaths = new Dictionary<string, string>(languageManifest.Count);
            Dictionary<string, string> localPaths = new Dictionary<string, string>(languageManifest.Count);

            if (!string.IsNullOrWhiteSpace(language))
            {
               Language = language;

               foreach (string languageFile in languageManifest.Keys)
               {
                  if (OcrLanguageManager.AdditionalLanguagesURL != null)
                     remotePaths[languageFile] = Path.Combine(OcrLanguageManager.AdditionalLanguagesURL, language, languageFile);

                  localPaths[languageFile] = Path.Combine(OcrLanguageManager.LanguagesDirectory, languageFile);
               }
            }

            RemotePaths = remotePaths;
            LocalPaths = localPaths;

            int numberOfLocalFiles = 0;
            long fileSize = 0;
            foreach (string localFilePath in localPaths.Values)
            {
               if (File.Exists(localFilePath))
                  numberOfLocalFiles++;
            }

            if (numberOfLocalFiles == 0)
            {
               foreach (Dictionary<string, object> val in languageManifest.Values)
                  fileSize += (long)val[OcrLanguageManager.FileSizeKeyString];

               FileSize = fileSize;
            }
            else
               FileSize = 0;
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private string _language = string.Empty;
      public string Language
      {
         get
         {
            return _language;
         }
         set
         {
            if (value != _language)
            {
               _language = value;
               NotifyPropertyChanged();
            }
         }
      }

      [XmlIgnore]
      public string LocalizedName
      {
         get
         {
            return new CultureInfo(Language).DisplayName;
         }
      }

      private double _fileSize = 0;
      [XmlIgnore]
      public double FileSize
      {
         get
         {
            return _fileSize;
         }
         private set
         {
            if (value != _fileSize)
            {
               _fileSize = value;
               NotifyPropertyChanged();
            }
         }
      }

      [XmlIgnore]
      public string FormattedFileSizeString
      {
         get
         {
            string sizeString = "(INF)";
            string[] sizes = { " B", " KB", " MB", " GB", " TB", " PB", " EB", " ZB", " YB" };

            int index = 0;
            double length = FileSize;
            while (length >= 1024.0)
            {
               // divide by 1024 for accurate step conversion
               length /= 1024.0;
               index++;
            }
            if (length >= 1000.0)
            {
               // Round down if 'length' is greater than 1000
               length = 1.0;
               index++;
            }

            if (index < sizes.Length)
            {
               string precision = "1";
               sizeString = string.Format("({0:N" + precision + "}{1})", length, sizes[index]);
            }

            return sizeString;
         }
      }

      private bool _isDownloading = false;
      [XmlIgnore]
      public bool IsDownloading
      {
         get
         {
            return _isDownloading;
         }
         set
         {
            if (value != _isDownloading)
            {
               _isDownloading = value;
               NotifyPropertyChanged();
            }
         }
      }

      private bool _isDownloadable = false;
      [XmlIgnore]
      public bool IsDownloadable
      {
         get
         {
            return _isDownloadable;
         }
         set
         {
            if (value != _isDownloadable)
            {
               _isDownloadable = value;
               NotifyPropertyChanged();
            }
         }
      }

      private bool _isDefaultLanguage = false;
      public bool IsDefaultLanguage
      {
         get
         {
            return _isDefaultLanguage;
         }
         set
         {
            if (value != _isDefaultLanguage)
            {
               _isDefaultLanguage = value;
               NotifyPropertyChanged();
               NotifyPropertyChanged(nameof(LanguageLabelTextColor));
            }
         }
      }

      private double _downloadPercentage = 0;
      [XmlIgnore]
      public double DownloadPercentage
      {
         get
         {
            return _downloadPercentage / 100;
         }
         set
         {
            if (value != _downloadPercentage)
            {
               _downloadPercentage = value;
               NotifyPropertyChanged();
            }
         }
      }

      [XmlIgnore]
      public bool CanBeDeleted
      {
         get
         {
            return OcrLanguageManager.CanDeleteLanguage(Language);
         }
      }

      [XmlIgnore]
      public CancellationTokenSource CancellationTokenSource { get; set; }

      [XmlIgnore]
      public Color LanguageLabelTextColor
      {
         get
         {
            return IsDefaultLanguage ? Color.FromHex("#56A7FD") : Color.FromHex("#57687F");
         }
      }

      [XmlIgnore]
      public object Tag { get; set; }

      [XmlIgnore]
      public Dictionary<string, string> LocalPaths { get; private set; }

      [XmlIgnore]
      public Dictionary<string, string> RemotePaths { get; private set; }
   }
}
