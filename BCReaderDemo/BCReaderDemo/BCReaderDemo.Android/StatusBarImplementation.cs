// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Views;
using BCReaderDemo.Droid;
using DataService;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarImplementation))]
namespace BCReaderDemo.Droid
{
   public class StatusBarImplementation : IStatusBar
   {
      public StatusBarImplementation()
      {
      }

      WindowManagerFlags _originalFlags;

      public void HideStatusBar()
      {
         if (Device.IsInvokeRequired)
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               var activity = MainActivity.Instance;
               var attrs = activity.Window.Attributes;
               _originalFlags = attrs.Flags;
               attrs.Flags |= Android.Views.WindowManagerFlags.Fullscreen;
               activity.Window.Attributes = attrs;
            });
         }
         else
         {
            var activity = MainActivity.Instance;
            var attrs = activity.Window.Attributes;
            _originalFlags = attrs.Flags;
            attrs.Flags |= Android.Views.WindowManagerFlags.Fullscreen;
            activity.Window.Attributes = attrs;
         }
      }

      public void ShowStatusBar()
      {
         if (Device.IsInvokeRequired)
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               var activity = MainActivity.Instance;
               var attrs = activity.Window.Attributes;
               attrs.Flags = _originalFlags;
               activity.Window.Attributes = attrs;
            });
         }
         else
         {
            var activity = MainActivity.Instance;
            var attrs = activity.Window.Attributes;
            attrs.Flags = _originalFlags;
            activity.Window.Attributes = attrs;
         }
      }
   }
}