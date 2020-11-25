// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************

using Leadtools.Demos;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Services;
using System.Linq;
using Xamarin.Forms;

namespace BCReaderDemo
{
   public partial class App : Application
   {
      static App()
      {
         // Configure
         DemoUtilities.Configure(
            title: "LEAD Business Card Scanner",
            version: "1.0",
            adName: "default",
            metaName: "xambcreaderapp",
            storeName: "LEADTOOLS BCR",
            shareName: "LEAD Business Card Scanner App",
            shareDescription: "fast business cards scanning and recognition",
            shareLink: "https://www.leadtools.com/apps/bcr",
            shareHashtags: new string[] { "bcr", "businesscardscanner", "business cards scanner", "businesscardsreader", "business cards reader" }
         );
      }

      public App()
      {
         InitializeComponent();

         MainPage = DemoUtilities.MainPage = new HomePage();
      }

      protected override async void OnStart()
      {
         var results = await Xamarin.Forms.DependencyService.Get<IPermissions>().VerifyPermissionsAsync(true, PermissionType.Storage);
         if (results == null || results[PermissionType.Storage] != PermissionStatus.Granted)
            return;

         HomePage.Instance.OnStart();
      }

      protected override void OnSleep()
      {
         HomePage.Instance.OnSleep();
      }

      protected override void OnResume()
      {
         if (PopupNavigation.Instance.PopupStack.LastOrDefault() is CameraPage cameraPage)
            cameraPage.AppResumed = true;

         HomePage.Instance.OnResume();
      }
   }
}
