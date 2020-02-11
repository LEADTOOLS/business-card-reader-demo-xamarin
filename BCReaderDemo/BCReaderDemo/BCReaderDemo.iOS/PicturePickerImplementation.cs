﻿// *************************************************************
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
using Xamarin.Forms;

[assembly: Dependency(typeof(PicturePickerImplementation))]
namespace BCReaderDemo.iOS
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   class PicturePickerImplementation : IPicturePicker
   {
      TaskCompletionSource<Stream> taskCompletionSource;
      UIImagePickerController imagePicker;

      public Task<Stream> GetImageStreamAsync()
      {
         try
         {
            // Create and define UIImagePickerController
            imagePicker = new UIImagePickerController
            {
               SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
               MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };
            // Set event handlers
            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;
            // Present UIImagePickerController;
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentModalViewController(imagePicker, true);
            // Return Task object
            taskCompletionSource = new TaskCompletionSource<Stream>();
            return taskCompletionSource.Task;
         }
         catch (Exception ex) { Console.WriteLine(ex.Message); return null; }
      }

      void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
      {
         UIImage image = args.EditedImage ?? args.OriginalImage;
         if (image != null)
         {
            // Convert UIImage to .NET Stream object
            NSData data = image.AsJPEG(1);
            Stream stream = data.AsStream();
            // Set the Stream as the completion of the Task
            taskCompletionSource.SetResult(stream);
         }
         else
         {
            taskCompletionSource.SetResult(null);
         }
         imagePicker.DismissModalViewController(true);
      }

      void OnImagePickerCancelled(object sender, EventArgs args)
      {
         taskCompletionSource.SetResult(null);
         imagePicker.DismissModalViewController(true);
      }
   }
}
