using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Leadtools.Demos
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DemoUtilities : BindableObject
   {
      #region TODO: LEADTOOLS License

      // TODO: Replace this with your License File contents
      private static string LicContents { get; } = "[License]\n" + "License = <doc><ver>2.0</ver><code>PASTE YOUR LICENSE CONTENTS HERE</code></doc>";
      private static string KeyContents { get; } = "PASTE YOUR DEVELOPER KEY HERE";

      #endregion

      #region Config

      public static string FontFamily { get; }

      static DemoUtilities()
      {
         if (Device.RuntimePlatform == Device.Android)
            FontFamily = "Droid Sans";
         else if (Device.RuntimePlatform == Device.iOS)
            FontFamily = "Helvetica";
         else
            FontFamily = (string)Label.FontFamilyProperty.DefaultValue;
      }

      #endregion

      private DemoUtilities() { }

      #region Bindable properties

      #region SafeAreaBottom

      public static readonly BindableProperty SafeAreaBottomProperty = BindableProperty.Create(
         propertyName: nameof(SafeAreaBottom),
         returnType: typeof(double),
         declaringType: typeof(DemoUtilities),
         defaultValue: 0.0,
         defaultBindingMode: BindingMode.OneWayToSource
      );
      public double SafeAreaBottom { get => GetValue(SafeAreaBottomProperty) is double value ? value : 0.0; private set => SetValue(SafeAreaBottomProperty, value); }
      public static double SafeAreaBottomInst => Instance.SafeAreaBottom;

      #endregion

      #region SafeAreaTop

      public static readonly BindableProperty SafeAreaTopProperty = BindableProperty.Create(
         propertyName: nameof(SafeAreaTop),
         returnType: typeof(double),
         declaringType: typeof(DemoUtilities),
         defaultValue: 0.0,
         defaultBindingMode: BindingMode.OneWayToSource
      );
      public double SafeAreaTop { get => GetValue(SafeAreaTopProperty) is double value ? value : 0.0; private set => SetValue(SafeAreaTopProperty, value); }
      public static double SafeAreaTopInst => Instance.SafeAreaTop;

      #endregion

      #endregion

      #region Internal properties

#if __IOS__
      private static Foundation.NSObject UIWindow_DidBecomeKey_Token { get; set; }
#endif

      #endregion

      #region Public properties

      public static DemoUtilities Instance { get; } = new DemoUtilities();
      public static Page MainPage { get; set; }

      public static double DisplayDensity { get; private set; }
      public static double DisplayWidth { get; private set; }
      public static double DisplayHeight { get; private set; }

      public static string CacheDir => FileSystem.CacheDirectory;

      public static string AppTitle { get; private set; }
      public static string AppVersion { get; private set; }
      public static string AppAdName { get; private set; }
      public static string AppAdID { get; private set; }
      public static string AppMetaName { get; private set; }
      public static string AppStoreName { get; private set; }
      public static string AppShareName { get; private set; }
      public static string AppShareDescription { get; private set; }
      public static string AppShareLink { get; private set; }
      public static string[] AppShareHashtags { get; private set; }
      
      #endregion

      #region Methods

      #region Events

#if __IOS__
      private static void UIWindow_DidBecomeKey(object sender, Foundation.NSNotificationEventArgs e)
      {
         // Notification no longer required
         UIWindow_DidBecomeKey_Token?.Dispose();
         UIWindow_DidBecomeKey_Token = null;

         // Read safe area from active window
         UIKit.UIEdgeInsets safeAreaInsets = UIKit.UIApplication.SharedApplication.KeyWindow.SafeAreaInsets;
         Instance.SafeAreaBottom = safeAreaInsets.Bottom;
         Instance.SafeAreaTop = safeAreaInsets.Top;
      }
#endif

      #endregion

      #region Helpers

      public static void Configure(string title, string version, string adName, string metaName, string storeName, string shareName, string shareDescription, string shareLink, string[] shareHashtags)
      {
         AppTitle = title;
         AppVersion = version;
         AppAdName = adName;
         AppMetaName = metaName;
         AppStoreName = storeName;
         AppShareName = shareName;
         AppShareDescription = shareDescription;
         AppShareLink = shareLink;
         AppShareHashtags = shareHashtags;

         // Some platforms don't support accessing the ID on the main thread
         Task.Run(() => ConfigureAdID());
      }

      private static void ConfigureAdID()
      {
         try
         {
#if __ANDROID__
            Google.Ads.Identifier.AdvertisingIdClient.Info info = Google.Ads.Identifier.AdvertisingIdClient.GetAdvertisingIdInfo(Android.App.Application.Context);
            if (!info.IsLimitAdTrackingEnabled)
               AppAdID = info.Id;
#elif __IOS__
            AdSupport.ASIdentifierManager manager = AdSupport.ASIdentifierManager.SharedManager;
            if (manager.IsAdvertisingTrackingEnabled)
               AppAdID = manager.AdvertisingIdentifier.AsString();
#elif __UWP__
            AppAdID = Windows.System.UserProfile.AdvertisingManager.AdvertisingId;
#endif
         }
         catch
         {
            // Optional: Report error
         }
      }

#if __ANDROID__
      public static void Init(Android.App.Activity mainActivity)
#else
      public static void Init()
#endif
      {
#if __ANDROID__
         // Configure the native library path
         Platform.LibraryPath = mainActivity.ApplicationInfo.NativeLibraryDir;
#endif

         // Configure the runtime platform
         Platform.RuntimePlatform = Device.RuntimePlatform;

         // Read display properties
         var displayInfo = DeviceDisplay.MainDisplayInfo;
         DisplayDensity = displayInfo.Density;
         DisplayWidth = displayInfo.Width / DisplayDensity;
         DisplayHeight = displayInfo.Height / DisplayDensity;

#if __IOS__
         // Determine the safe area, once the window is set
         UIWindow_DidBecomeKey_Token = UIKit.UIWindow.Notifications.ObserveDidBecomeKey(UIWindow_DidBecomeKey);
#endif
      }

      public static bool SetLicense(Page mainPage, bool silent = false)
      {
         // Need Page as parameter instead of using MainPage as this code may execute within the Page's constructor
         RasterSupport.Initialize(mainPage);

         if (RasterSupport.KernelExpired)
            try
            {
               byte[] licBytes = System.Text.Encoding.UTF8.GetBytes(LicContents);
               RasterSupport.SetLicense(licBytes, KeyContents);
            }
            catch (Exception ex)
            {
               Debug.WriteLine(ex.Message);
            }

         if (RasterSupport.KernelExpired && !silent)
         {
            string msg = "Your license file is missing, invalid or expired. LEADTOOLS will not function. Please contact LEAD Sales for information on obtaining a valid license.";
            MainThread.BeginInvokeOnMainThread(async () => await mainPage.DisplayAlert("Error", msg, "OK"));
         }

         return !RasterSupport.KernelExpired;
      }

      public static string QueryString(string source, bool includeID = true) => $"utm_source={AppMetaName}&utm_medium=mobileapp&utm_campaign={AppMetaName}-{source}&SrcOrigin={AppMetaName}-{source}{(includeID && !string.IsNullOrEmpty(AppAdID) ? $"&did={AppAdID}" : "")}";

      #endregion

      #endregion
   }
}
