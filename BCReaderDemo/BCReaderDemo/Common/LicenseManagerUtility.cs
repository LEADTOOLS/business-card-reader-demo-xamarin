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
   public static class LicenseManagerUtility
   {
      // TODO: Replace this with your License File contents
      private static string LicContents { get; } = "[License]\n" + "License = <doc><ver>2.0</ver><code>PASTE YOUR LICENSE CONTENTS HERE</code></doc>";
      private static string KeyContents { get; } = "PASTE YOUR DEVELOPER KEY HERE";

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
   }
}
