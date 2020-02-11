// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using DataService;
using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using BCReaderDemo.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(SaveImageImplementation))]
namespace BCReaderDemo.iOS
{
   class SaveImageImplementation : IPictureSaver
   {
      public Task<bool> SaveImage(Stream stream, string filePath, PictureSaveResolution resolution)
      {
         TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
         try
         {
            UIImage myImage = new UIImage(NSData.FromStream(stream));
            Leadtools.LeadSize size = RasterImageHelper.GetImageSize((int)myImage.Size.Width, (int)myImage.Size.Height, resolution);

            myImage = myImage.Scale(new CoreGraphics.CGSize(size.Width, size.Height));
            bool res = myImage.AsJPEG().Save(filePath, false);

            taskCompletionSource.SetResult(res);
            return taskCompletionSource.Task;
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            taskCompletionSource.SetResult(false);
            return taskCompletionSource.Task;
         }
      }
   }
}