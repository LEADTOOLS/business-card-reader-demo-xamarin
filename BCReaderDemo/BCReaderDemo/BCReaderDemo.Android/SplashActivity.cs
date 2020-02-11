// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace BCReaderDemo.Droid
{
   [Activity(Label = "LEAD BCR", Theme = "@style/Theme.Splash", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = true, NoHistory = true)]
   public class SplashActivity : Activity
   {
      protected override void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         this.StartActivity(typeof(MainActivity));
      }
   }
}
