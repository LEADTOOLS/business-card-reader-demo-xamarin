// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using CarouselView.FormsPlugin.iOS;
using Foundation;
using Leadtools.Demos;
using UIKit;

namespace BCReaderDemo.iOS
{
   // The UIApplicationDelegate for the application. This class is responsible for launching the 
   // User Interface of the application, as well as listening (and optionally responding) to 
   // application events from iOS.
   [Register("AppDelegate")]
   public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
   {
      //
      // This method is invoked when the application has loaded and is ready to run. In this 
      // method you should instantiate the window, load the UI into it and then make the window
      // visible.
      //
      // You have 17 seconds to return from this method, or iOS will terminate your application.
      //
      public override bool FinishedLaunching(UIApplication app, NSDictionary options)
      {
         Leadtools.Converters.Assembly.Use();
         Leadtools.Controls.iOS.Assembly.Use();
         Leadtools.Camera.Xamarin.iOS.Assembly.Use();

         Rg.Plugins.Popup.Popup.Init();

         global::Xamarin.Forms.Forms.Init();

         // Initialize the shared components
         Leadtools.Demos.iOS.Assembly.Use();

         CarouselViewRenderer.Init();
         LoadApplication(new App());

         return base.FinishedLaunching(app, options);
      }
   }
}
