// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Content;
using BCReaderDemo.Droid;
using DataService;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShareImplementation))]
namespace BCReaderDemo.Droid
{
   public class ShareImplementation : IShareContact
   {
      private readonly Context _context;
      public ShareImplementation()
      {
         _context = Android.App.Application.Context;
      }

      public Task Show(string filePath)
      {
         var contentType = "text/x-vcard";
         var activity = MainActivity.Instance;
         Android.Net.Uri fileUri;

         if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.N)
         {
            fileUri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
         }
         else
         {
            fileUri = Android.Support.V4.Content.FileProvider.GetUriForFile(activity, $"{activity.PackageName}.provider", new Java.IO.File(filePath));
         }

         var intent = new Intent(Intent.ActionSend);
         intent.SetType(contentType);
         intent.PutExtra(Intent.ExtraStream, fileUri);
         
         var chooserIntent = Intent.CreateChooser(intent, "Share Business Card");
         chooserIntent.SetFlags(ActivityFlags.ClearTop);
         chooserIntent.SetFlags(ActivityFlags.NewTask);
         _context.StartActivity(chooserIntent);

         return Task.FromResult(true);
      }
      
   }
}