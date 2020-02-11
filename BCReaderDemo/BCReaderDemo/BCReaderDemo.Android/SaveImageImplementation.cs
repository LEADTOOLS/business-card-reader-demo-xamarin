// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Graphics;
using BCReaderDemo.Droid;
using BCReaderDemo.Utils;
using DataService;
using System;
using System.IO;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(SaveImageImplementation))]
namespace BCReaderDemo.Droid
{
   class SaveImageImplementation : IPictureSaver
   {
      public Task<bool> SaveImage(Stream stream, string filePath, PictureSaveResolution resolution)
      {
         var ImageTask = new TaskCompletionSource<bool>();
         
         try
         {
            Java.IO.File file = new Java.IO.File(filePath);
            if (file.Exists()) file.Delete();

            Bitmap myBitmap = BitmapFactory.DecodeStream(stream);
                        
            Leadtools.LeadSize size = RasterImageHelper.GetImageSize(myBitmap.Width, myBitmap.Height, resolution);

            int width = size.Width;
            int height = size.Height;

            Bitmap resized = Bitmap.CreateScaledBitmap(myBitmap, width, height, true);
            myBitmap.Recycle();
            StreamWriter streamwriter = new StreamWriter(file.AbsolutePath);
            Stream anotherStream = streamwriter.BaseStream;
            resized.Compress(Bitmap.CompressFormat.Jpeg, 90, anotherStream);
            anotherStream.Flush();
            anotherStream.Close();
            ImageTask.SetResult(true);
            resized.Recycle();
            return ImageTask.Task;
         }
         catch (Exception)
         {
            ImageTask.SetResult(false);
            return ImageTask.Task;
         }
      }
   }
}