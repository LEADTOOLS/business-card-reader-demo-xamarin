// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************

using System.Linq;
using Xamarin.Forms;

namespace BCReaderDemo
{
   public partial class App : Application
   {
      public static double DisplayScreenWidth = 0f;
      public static double DisplayScreenHeight = 0f;
      public static double DisplayScaleFactor = 0f;
      public static double DeviceSafeAreaBottom = 0f;
#if __ANDROID__
      public static Android.App.Activity MainActivity = null;
#endif // #if __ANDROID__

      public App()
      {
         InitializeComponent();

         MainPage = new NavigationPage(new HomePage());
      }

      protected override async void OnStart()
      {
         var results = await Xamarin.Forms.DependencyService.Get<Permissions.IPermissions>().VerifyPermissionsAsync(Permissions.Permission.Storage);
         if (results == null || results[Permissions.Permission.Storage] != Permissions.PermissionStatus.Granted)
            return;

         HomePage.Instance.Initialize();
      }

      protected override void OnSleep()
      {
      }

      protected override void OnResume()
      {
         if (MainPage.Navigation.NavigationStack.Last() is CameraPage cameraPage)
            cameraPage.AppResumed = true;
      }
   }
}
