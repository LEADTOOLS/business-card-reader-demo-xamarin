// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Leadtools.Demos;
using Leadtools.Demos.Droid.Utils;

namespace BCReaderDemo.Droid
{
   [Activity(Label = "LEAD BCR", Icon = "@drawable/icon", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.SensorPortrait, ResizeableActivity = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.SmallestScreenSize | ConfigChanges.ScreenLayout)]
   public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
   {
      public static MainActivity Instance { get; private set; }

      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         Instance = this;

         TabLayoutResource = Resource.Layout.Tabbar;
         ToolbarResource = Resource.Layout.Toolbar;

         Rg.Plugins.Popup.Popup.Init(this, bundle);
         Xamarin.Essentials.Platform.Init(this, bundle);
         global::Xamarin.Forms.Forms.Init(this, bundle);
         global::CarouselView.FormsPlugin.Android.CarouselViewRenderer.Init();

         // Initialize the shared components
         Leadtools.Demos.Droid.Assembly.Use(this);

         LoadApplication(new App());
      }

      public override void OnBackPressed()
      {
         // Not handling return value
         Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
      }

      protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
      {
         PicturePickerImplementation.OnActivityResult(requestCode, resultCode, intent);
         base.OnActivityResult(requestCode, resultCode, intent);
      }

      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
      {
         PermissionsImplementation.Instance.OnRequestPermissionsResult(requestCode, permissions, grantResults);
         Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
         base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }
   }
}

