// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using Leadtools;
using Leadtools.Codecs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using Plugin.Connectivity;
using DataService;

namespace BCReaderDemo.Utils
{
   public static class Helpers
   {
      public static EmailType EmailStringToType(string emailString)
      {
         if (emailString.Equals("Personal Email"))
            return EmailType.Personal;
         else
            return EmailType.Work;
      }

      public static string EmailTypeToString(EmailType type)
      {
         if (type == EmailType.Personal)
            return "Personal Email";
         else
            return "Work Email";
      }

      public static PhoneType PhoneStringToType(string phoneString)
      {
         switch (phoneString)
         {
            case "Work Fax":
               return PhoneType.WorkFax;
            case "Work Mobile":
               return PhoneType.WorkMobile;
            case "Home Fax":
               return PhoneType.HomeFax;
            case "Home Mobile":
               return PhoneType.HomeMobile;
            case "Home Tel":
               return PhoneType.Home;
            case "Work Tel":
               return PhoneType.Work;
            default:
               return PhoneType.Work;
         }
      }

      public static string PhoneTypeToString(PhoneType type)
      {
         switch (type)
         {
            case PhoneType.WorkFax:
               return "Work Fax";
            case PhoneType.WorkMobile:
               return "Work Mobile";
            case PhoneType.HomeFax:
               return "Home Fax";
            case PhoneType.HomeMobile:
               return "Home Mobile";
            default:
               return type.ToString() + " Tel";
         }
      }
   }

   public class CallDataMethod : TriggerAction<VisualElement>
   {
      public string Method { get; set; }

      protected override void Invoke(VisualElement sender)
      {
         MethodInfo method = sender.GetType().GetRuntimeMethod(Method, new Type[0]);
         if (method != null)
         {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
               Device.BeginInvokeOnMainThread(() => { method.Invoke(sender, null); });
            }
         }
      }
   }

   public static class RasterImageHelper
   {
      public static Leadtools.LeadSize GetImageSize(int width, int height, PictureSaveResolution resolution)
      {
         int max = 0;

         if (resolution == PictureSaveResolution.Low)
         {
            max = 1000;
         }
         else if (resolution == PictureSaveResolution.Medium)
         {
            max = 2000;
         }
         else
         {
            max = 3000;
         }

         int newWidth = width;
         int newHeight = height;

         int maxWidthHeight = Math.Max(newWidth, newHeight);
         if (maxWidthHeight > max)
         {
            if (width == maxWidthHeight)
            {
               newWidth = max;
               newHeight = (int)((float)max / maxWidthHeight * newHeight);
            }
            else
            {
               newHeight = max;
               newWidth = (int)((float)max / maxWidthHeight * newWidth);
            }
         }

         return new LeadSize(newWidth, newHeight);
      }
   }

   public static class AdHelper
   {
      class Ad
      {
         public string content { get; set; }
      }

      class LeadtoolsApp
      {
         public string app { get; set; }
         public List<Ad> ads { get; set; }
      }

      class LeadtoolsApps
      {
         public List<LeadtoolsApp> apps { get; set; }
      }

      public static async Task PopulateAds()
      {
         string adsFileDownloadPath = Path.Combine(HomePage.APP_DIR, Path.GetFileName(HomePage._adsUrlPath));
         bool useEmbeddedAds = true;
         DateTime lastCheckedDate = DateTime.MinValue;

         if (HasWifiConnection())
         {
            // If the ads.json file already exists (downloaded) then check if its been over a week since the last time we downloaded the file 
            // in order to check if the ads URL was updated and download the updated file, otherwise use the embedded ads JSON file.
            if (File.Exists(adsFileDownloadPath))
            {
               if (IsItTimeToCheckForUpdatedAdsOnline(out lastCheckedDate))
                  useEmbeddedAds = false;
            }
            else
               useEmbeddedAds = false;
         }

         if (!useEmbeddedAds)
         {
            try
            {
               await DownloadAndPopulateAdFile(lastCheckedDate);
            }
            catch (HttpRequestException ex)
            {
               Console.WriteLine(ex.Message);

               // Use the embedded ads file in case of exception
               PopulateAdsFromFile();
            }
         }
         else
         {
            PopulateAdsFromFile();
         }

         CrossConnectivity.Current.ConnectivityTypeChanged += CrossConnectivity_ConnectivityTypeChanged;
      }

      private static bool HasWifiConnection()
      {
         bool ret = false;

         try
         {
            ret = CrossConnectivity.Current.IsConnected && CrossConnectivity.Current.ConnectionTypes.Contains(Plugin.Connectivity.Abstractions.ConnectionType.WiFi);
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         return ret;
      }

      private static bool IsItTimeToCheckForUpdatedAdsOnline(out DateTime lastCheckedDate)
      {
         bool ret = false;
         lastCheckedDate = DateTime.MinValue;

         try
         {
            if (!File.Exists(HomePage._adsLastCheckFilePath))
               ret = true;
            else
            {
               using (StreamReader reader = new StreamReader(HomePage._adsLastCheckFilePath))
               {
                  string lastCheckedDataString = reader.ReadLine();
                  if (DateTime.TryParse(lastCheckedDataString, out lastCheckedDate))
                  {
                     if ((DateTime.Now - lastCheckedDate).TotalDays > 7)
                        ret = true;
                  }
               }
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         return ret;
      }

      static bool _downloadFileInProgress = false;
      private static async Task DownloadAndPopulateAdFile(DateTime lastCheckedDate)
      {
         _downloadFileInProgress = true;
         try
         {
            HttpResponseMessage response = await HomePage._httpClient.GetAsync(HomePage._adsUrlPath);
            if (response.IsSuccessStatusCode)
            {
               if (response.Content.Headers.LastModified.Value.DateTime.CompareTo(lastCheckedDate) > 0)
               {
                  // Online JSON file is newer than the last one we downloaded, so download the newer file.
                  string responseBody = await response.Content.ReadAsStringAsync();
                  File.WriteAllText(HomePage.ADS_DOWNLOADED_FILE_PATH, responseBody);
                  PopulateAdsFromFile();
                  File.WriteAllText(HomePage._adsLastCheckFilePath, DateTime.Now.ToString());
               }
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
         finally
         {
            if (HomePage._ads.Count == 0)
               PopulateAdsFromFile();
            _downloadFileInProgress = false;
         }
      }

      private static void CrossConnectivity_ConnectivityTypeChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityTypeChangedEventArgs e)
      {
         if (HasWifiConnection() && !_downloadFileInProgress)
         {
            DateTime lastCheckedDate = DateTime.MinValue;
            string adsFileDownloadPath = Path.Combine(HomePage.APP_DIR, Path.GetFileName(HomePage._adsUrlPath));

            if (!File.Exists(adsFileDownloadPath) || IsItTimeToCheckForUpdatedAdsOnline(out lastCheckedDate))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
               DownloadAndPopulateAdFile(lastCheckedDate);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
         }
      }

      public static System.Timers.Timer ShowAdvertisement(AdsLayout adsLayout)
      {
         System.Timers.Timer timer = new System.Timers.Timer();
         timer.Interval = HomePage.AD_VISIBLE_DURATION;
         Device.BeginInvokeOnMainThread(async () =>
         {
            if (HomePage._ads.Count == 0) return;

            if (HomePage._lastShownAdIndex >= HomePage._ads.Count)
               HomePage._lastShownAdIndex = 0;

            adsLayout.AdText = HomePage._ads[HomePage._lastShownAdIndex];
            HomePage._lastShownAdIndex++;

            adsLayout.TranslationY = PlatformsConstants.AdRowHeight + App.DeviceSafeAreaBottom;
            await adsLayout.TranslateTo(0, -1, 1000);
            timer.Elapsed += async (sender, e) =>
            {
               timer.Stop();
               await adsLayout.TranslateTo(0, PlatformsConstants.AdRowHeight + App.DeviceSafeAreaBottom, 1000);
            };
            timer.Enabled = true;
            timer.Start();
         });

         return timer;
      }

      private static void PopulateAdsFromFile()
      {
         try
         {
            string sourceAdsFile = string.Empty;
            if (File.Exists(HomePage.ADS_DOWNLOADED_FILE_PATH))
               sourceAdsFile = HomePage.ADS_DOWNLOADED_FILE_PATH;
            else
               sourceAdsFile = HomePage.ADS_EMBEDDED_FILE_PATH;

            string jsonFileContents = File.ReadAllText(sourceAdsFile);
            LeadtoolsApps leadtoolsApps = JsonConvert.DeserializeObject<LeadtoolsApps>(jsonFileContents);
            foreach (LeadtoolsApp app in leadtoolsApps.apps)
            {
               if (app.app.Equals("business_card_reader"))
               {
                  if (app.ads != null && app.ads.Count > 0)
                     HomePage._ads.Clear();

                  foreach (Ad ad in app.ads)
                  {
                     HomePage._ads.Add(ad.content);
                  }
                  break;
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }
   }
}
