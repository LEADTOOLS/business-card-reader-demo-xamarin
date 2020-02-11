// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Content;
using BCReaderDemo.Droid;
using DataService;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(PicturePickerImplementation))]
namespace BCReaderDemo.Droid
{
   class PicturePickerImplementation : IPicturePicker
   {
      public Task<Stream> GetImageStreamAsync()
      {
         // Define the Intent for getting images
         Intent intent = new Intent();
         intent.SetType("image/*");
         intent.SetAction(Intent.ActionGetContent);
         // Get the MainActivity instance
         MainActivity activity = MainActivity.Instance;
         // Start the picture-picker activity (resumes in MainActivity.cs)
         activity.StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), MainActivity.Instance.GalleryId);
         // Save the TaskCompletionSource object as a MainActivity property
         activity.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();
         // Return Task object
         return activity.PickImageTaskCompletionSource.Task;
      }
   }
}