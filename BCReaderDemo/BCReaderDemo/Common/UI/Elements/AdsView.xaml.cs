// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class AdsView : ContentView
   {
      #region Config

      private static string AdsEmbeddedResource => Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(rn => rn.EndsWith("embedded_ads.json"));
      private static string AdsUrl => $"https://test.leadtools.com/content/ad_content/ads.json?{DemoUtilities.QueryString("ad")}";

      private static string AdsFileDownloadCache => Path.Combine(DemoUtilities.CacheDir, Path.GetFileName(AdsUrl));
      private static string AdsFileDownloadLastModified => Path.Combine(DemoUtilities.CacheDir, "AdsLastModified.txt");
      private static TimeSpan AdsDownloadKeepAlive { get; } = TimeSpan.FromDays(7);

      private static int AdDelay { get; } = 10000;
      private static int AdDuration { get; } = 15000;
      private static uint AdAnimateDuration { get; } = 200U; // Scales based on the offscreen distance

      public static Color TextColor { get; } = Color.FromRgb(47, 61, 76);
      private static Color KeywordColor { get; } = Color.FromRgb(68, 68, 224);
      private static string Keyword { get; } = "LEADTOOLS";

      // Adjust font size based on screen width
      private static int MaxLength { get; } = 50;
      private static double FontAspectRatio { get; } = 1.75; // Estimated
      private static double MaxFontSize { get; } = FontSizeExtension.SmallFontSize;
      public static double FontSize => Math.Min(DemoUtilities.DisplayWidth / MaxLength * FontAspectRatio, MaxFontSize);

      #endregion

      public AdsView()
      {
         // Setup
         Opacity = 0;
         VerticalOptions = LayoutOptions.End;

         // Initialize
         InitializeComponent();
         InitTimers();
      }

      #region Internal properties

      private int AdIndex { get; set; } = -1;
      private Timer DelayTimer { get; set; }
      private Timer DurationTimer { get; set; }

      #endregion

      #region Methods

      #region Events

      private void DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         ShowAd();
      }

      private void DurationTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         // Dispose previous
         DurationTimer.Dispose();
         DurationTimer = null;

         // Wait for another ad
         DelayTimer.Start();
      }

      #endregion

      #region Helpers

      public static async Task Init() => await AdManager.Init();

      private void InitTimers()
      {
         DelayTimer = new Timer(AdDelay)
         {
            AutoReset = false
         };
         DelayTimer.Elapsed += DelayTimer_Elapsed;
      }

      private void ShowAd()
      {
         DurationTimer = AdManager.ShowAdvertisement(this);
         DurationTimer.Elapsed += DurationTimer_Elapsed;
      }

      public void Start() => ShowAd();

      public void Stop()
      {
         // Stop timers
         DelayTimer.Stop();
         if (DurationTimer != null)
         {
            DurationTimer.Stop();
            DurationTimer.Dispose();
            DurationTimer = null;
         }

         // Not visible
         Opacity = 0;
      }

      private void UpdateContent(string content)
      {
         // Clear previous
         AdContent.FormattedText.Spans.Clear();

         if (content.Contains(Keyword))
         {
            int prevKeyword = 0;
            int nextKeyword;

            // Find each keyword
            while ((nextKeyword = content.IndexOf(Keyword, prevKeyword)) != -1)
            {
               // Add content before next keyword, if any
               if (nextKeyword > prevKeyword)
                  AdContent.FormattedText.Spans.Add(new Span()
                  {
                     Text = content.Substring(prevKeyword, nextKeyword - prevKeyword)
                  });

               // Add the keyword, with special color
               AdContent.FormattedText.Spans.Add(new Span()
               {
                  Text = Keyword,
                  TextColor = KeywordColor
               });

               // Skip keyword
               prevKeyword = nextKeyword + Keyword.Length;
            }

            // Add remaining contents
            if (prevKeyword < content.Length)
               AdContent.FormattedText.Spans.Add(new Span()
               {
                  Text = content.Substring(prevKeyword)
               });
         }
         else
            // Only non-keyword text
            AdContent.FormattedText.Spans.Add(new Span()
            {
               Text = content
            });
      }

      #endregion

      #endregion

      #region Data classes

#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters
      private class AdApp
      {
         public string app { get; set; }
         public List<AdText> ads { get; set; }
      }
      private class AdJSON
      {
         public List<AdApp> apps { get; set; }
      }
      private class AdText
      {
         public string content { get; set; }
      }
#pragma warning restore IDE1006 // Naming rule violation: These words must begin with upper case characters

      #endregion

      #region AdManager

      private static class AdManager
      {
         #region Internal properties

         private static List<string> Ads { get; } = new List<string>();
         private static object AdsLock { get; } = new object();
         private static bool DownloadingAds { get; set; }
         private static HttpClient HttpClient { get; } = new HttpClient();
         private static Random Random { get; } = new Random(Environment.TickCount);

         #endregion

         #region Methods

         #region Events

         private static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
         {
            if (HasWifiConnection() && !DownloadingAds)
            {
               DateTime lastModified = DateTime.MinValue;
               if (!File.Exists(AdsFileDownloadLastModified) || HaveAdsExpired(out lastModified))
               {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                  DownloadAndPopulateAds(lastModified);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
               }
            }
         }

         #endregion

         #region Helpers

         private static async Task DownloadAndPopulateAds(DateTime lastModified)
         {
            DownloadingAds = true;
            try
            {
               using (HttpResponseMessage response = await HttpClient.GetAsync(AdsUrl))
                  if (response.IsSuccessStatusCode && response.Content.Headers.LastModified != null)
                  {
                     if (response.Content.Headers.LastModified.Value.DateTime > lastModified)
                     {
                        // Online JSON file is newer than the previous, download it
                        File.WriteAllText(AdsFileDownloadCache, await response.Content.ReadAsStringAsync());

                        // Successfully downloaded, update last modified
                        File.WriteAllText(AdsFileDownloadLastModified, response.Content.Headers.LastModified.Value.ToString());
                     }
                  }
            }
            catch
            {
               // Optional: Log exception
            }
            finally
            {
               // Populate the ads list
               PopulateAds();

               DownloadingAds = false;
            }
         }

         private static bool HasWifiConnection()
         {
            try
            {
               return Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi);
            }
            catch
            {
               // Optional: Log exception
            }
            return false;
         }

         private static bool HaveAdsExpired(out DateTime lastModified)
         {
            lastModified = DateTime.MinValue;

            try
            {
               if (!File.Exists(AdsFileDownloadLastModified))
                  return true;
               else
                  using (StreamReader reader = new StreamReader(AdsFileDownloadLastModified))
                  {
                     string lastModifiedString = reader.ReadToEnd();
                     return DateTime.TryParse(lastModifiedString, out lastModified) && (DateTime.Now - lastModified) > AdsDownloadKeepAlive;
                  }
            }
            catch
            {
               // Optional: Log exception
            }

            return false;
         }

         public static async Task Init()
         {
            bool downloadAds = false;
            DateTime lastModified = DateTime.MinValue;

            if (HasWifiConnection())
               if (File.Exists(AdsFileDownloadCache))
               {
                  if (HaveAdsExpired(out lastModified))
                     downloadAds = true;
               }
               else
                  downloadAds = true;
            else
               // Monitor connection
               Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            if (downloadAds)
            {
               try
               {
                  await DownloadAndPopulateAds(lastModified);

                  // Already populated, no need to do twice
                  return;
               }
               catch
               {
                  // Optional: Log exception
               }
            }
            else
               PopulateAds();
         }

         private static void PopulateAds()
         {
            string jsonContent;
            if (File.Exists(AdsFileDownloadCache))
               // Read from the downloaded file
               jsonContent = File.ReadAllText(AdsFileDownloadCache);
            else
               // Read from the embedded resource
               using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(AdsEmbeddedResource))
               using (StreamReader reader = new StreamReader(stream))
                  jsonContent = reader.ReadToEnd();

            // Extract the requested ads
            AdJSON json = JsonConvert.DeserializeObject<AdJSON>(jsonContent);
            foreach (AdApp app in json.apps)
               if (app.app.Equals(DemoUtilities.AppAdName, StringComparison.InvariantCultureIgnoreCase))
                  lock (AdsLock)
                  {
                     // Clear previous
                     if (app.ads != null && app.ads.Count > 0)
                        Ads.Clear();

                     // Add new
                     Ads.AddRange(app.ads.Select(ad => ad.content));

                     // Shuffle
                     for (int i = 0, n = Ads.Count; i < n - 1; i++)
                     {
                        int j = Random.Next(i, n);
                        string temp = Ads[i];
                        Ads[i] = Ads[j];
                        Ads[j] = temp;
                     }
                     break;
                  }
         }

         public static Timer ShowAdvertisement(AdsView view)
         {
            // Create a new timer
            Timer timer = new Timer(AdDuration)
            {
               AutoReset = false
            };

            // Setup the UI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
               lock (AdsLock)
               {
                  // No ads?
                  if (Ads == null || Ads.Count == 0)
                  {
                     // Try again later, might still be downloading
                     try
                     {
                        timer.Start();
                     }
                     catch (ObjectDisposedException)
                     {
                        // May have closed the popup too quickly
                     }
                     return;
                  }

                  // First ad?
                  if (view.AdIndex == -1)
                     view.AdIndex = Random.Next(Ads.Count);

                  // Increment the ad
                  if (++view.AdIndex >= Ads.Count)
                     view.AdIndex = 0;

                  // Update the content
                  view.UpdateContent(Ads[view.AdIndex]);
               }

               // Determine offscreen position
               double screenY = 0;
               Element element = view;
               while (element is VisualElement visualElement && !(element is Xamarin.Forms.Page))
               {
                  screenY += visualElement.Y;
                  element = element.Parent;
               }
               double offscreenTranslationY;
               if (view.VerticalOptions.Alignment == LayoutAlignment.Start)
                  // Go off to the top
                  offscreenTranslationY = -screenY - 5; // A bit of extra padding
               else
                  // Go off to the bottom
                  offscreenTranslationY = DemoUtilities.DisplayHeight - screenY + 5;

               // Animate from offscreen
               uint duration = (uint)Math.Min(Math.Ceiling(offscreenTranslationY / view.Height * AdAnimateDuration), AdDelay);
               view.TranslationY = offscreenTranslationY;
               view.Opacity = 1;
               await view.TranslateTo(0, 0, duration);

               // Animate to offscreen when finished
               timer.Elapsed += async (s, e) => await view.TranslateTo(0, offscreenTranslationY, duration);

               // Start the timer
               try
               {
                  timer.Start();
               }
               catch (ObjectDisposedException)
               {
                  // May have closed the popup too quickly
               }
            });

            return timer;
         }

         #endregion

         #endregion
      }

      #endregion
   }
}