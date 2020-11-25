// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace BCReaderDemo.Droid
{
   [Activity(Label = "LEAD BCR", Theme = "@style/Theme.Splash", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorPortrait, MainLauncher = true, NoHistory = true)]
   public class SplashActivity : Activity
   {
      protected override void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         StartActivity(typeof(MainActivity));
      }
   }
}
