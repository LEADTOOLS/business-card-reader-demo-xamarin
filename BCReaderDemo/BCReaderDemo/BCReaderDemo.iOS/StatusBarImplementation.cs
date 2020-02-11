// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using DataService;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarImplementation))]

namespace BCReaderDemo.iOS
{
   public class StatusBarImplementation : IStatusBar
   {
      public StatusBarImplementation()
      {
      }

      public void HideStatusBar()
      {
         UIApplication.SharedApplication.StatusBarHidden = true;
      }

      public void ShowStatusBar()
      {
         UIApplication.SharedApplication.StatusBarHidden = false;
      }
   }
}