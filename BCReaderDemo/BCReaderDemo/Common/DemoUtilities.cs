// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Diagnostics;
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
            FontFamily = "Segoe UI";
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
      public double SafeAreaBottom { get => GetValue(SafeAreaBottomProperty) is double value ? value : 0.0; internal set => SetValue(SafeAreaBottomProperty, value); }
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
      public double SafeAreaTop { get => GetValue(SafeAreaTopProperty) is double value ? value : 0.0; internal set => SetValue(SafeAreaTopProperty, value); }
      public static double SafeAreaTopInst => Instance.SafeAreaTop;

      #endregion

      #endregion

      #region Internal properties

      #endregion

      #region Public properties

      public static DemoUtilities Instance { get; } = new DemoUtilities();
      public static Page MainPage { get; set; }

      public static double DisplayDensity { get; internal set; }
      public static double DisplayWidth { get; internal set; }
      public static double DisplayHeight { get; internal set; }

      public static string CacheDir => FileSystem.CacheDirectory;

      public static string AppTitle { get; private set; }
      public static string AppVersion { get; private set; }
      public static string AppAdName { get; private set; }
      public static string AppAdID { get; internal set; }
      public static string AppMetaName { get; private set; }
      public static string AppStoreName { get; private set; }
      public static string AppShareName { get; private set; }
      public static string AppShareDescription { get; private set; }
      public static string AppShareLink { get; private set; }
      public static string[] AppShareHashtags { get; private set; }
      
      #endregion

      #region Methods

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
      }

      internal static void Init()
      {
         // Configure the runtime platform
         Platform.RuntimePlatform = Device.RuntimePlatform;

         // Read display properties
         var displayInfo = DeviceDisplay.MainDisplayInfo;
         DisplayDensity = displayInfo.Density;

         DisplayWidth = displayInfo.Width / DisplayDensity;
         DisplayHeight = displayInfo.Height / DisplayDensity;
      }

      public static string QueryString(string source, bool includeID = true) => $"utm_source={AppMetaName}&utm_medium=mobileapp&utm_campaign={AppMetaName}-{source}&srcorigin={AppMetaName}-{source}{(includeID && !string.IsNullOrEmpty(AppAdID) ? $"&did={AppAdID}" : "")}".ToLower();

      #endregion

      #endregion
   }
}
