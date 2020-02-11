// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using System.IO;
using System.Threading.Tasks;

namespace BCReaderDemo.Droid
{
   [Activity(Label = "LEAD BCR", Icon = "@drawable/icon", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
   public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
   {
      public static MainActivity Instance { get; private set; }
      public readonly int CameraId = 2000;
      public readonly int GalleryId = 1000;
      public TaskCompletionSource<Stream> PickImageTaskCompletionSource;
      public string outputFile;
      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         Rg.Plugins.Popup.Popup.Init(this, bundle);

         Instance = this;
         App.MainActivity = this;

         TabLayoutResource = Resource.Layout.Tabbar;
         ToolbarResource = Resource.Layout.Toolbar;

         App.DisplayScreenWidth = (double)Resources.DisplayMetrics.WidthPixels / (double)Resources.DisplayMetrics.Density;
         App.DisplayScreenHeight = (double)Resources.DisplayMetrics.HeightPixels / (double)Resources.DisplayMetrics.Density;
         App.DisplayScaleFactor = (double)Resources.DisplayMetrics.Density;

         global::Xamarin.Forms.Forms.Init(this, bundle);
         global::CarouselView.FormsPlugin.Android.CarouselViewRenderer.Init();

         LoadApplication(new App());
      }

      public override void OnBackPressed()
      {
         if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
         {
         }
         else
         {
         }
      }

      protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
      {
         if (requestCode == CameraId)
         {
            if (resultCode == Result.Ok)
            {
               Android.Net.Uri uri = Android.Net.Uri.FromFile(new Java.IO.File(outputFile));
               var stream = ContentResolver.OpenInputStream(uri);

               PickImageTaskCompletionSource.SetResult(stream);
            }
            if (resultCode == Result.Canceled)
            {
               PickImageTaskCompletionSource.SetResult(null);
            }
         }
         else if (requestCode == GalleryId)
         {
            if ((resultCode == Result.Ok) && (intent != null))
            {
               Android.Net.Uri uri = intent.Data;
               Stream stream = ContentResolver.OpenInputStream(uri);

               PickImageTaskCompletionSource.SetResult(stream);
            }
            else
            {
               PickImageTaskCompletionSource.SetResult(null);
            }
         }
      }

      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
      {
         PermissionsImplementation.Instance.OnRequestPermissionsResult(requestCode, permissions, grantResults);
         base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }

      private static Bitmap RotateImageIfRequired(Context context, Bitmap img, Android.Net.Uri selectedImage)
      {
         System.IO.Stream input = Android.App.Application.Context.ContentResolver.OpenInputStream(selectedImage);
         Android.Media.ExifInterface ei;
         if (Build.VERSION.SdkInt > BuildVersionCodes.M)
            ei = new Android.Media.ExifInterface(input);
         else
            ei = new Android.Media.ExifInterface(selectedImage.Path);
         int orientation = ei.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, (int)Android.Media.Orientation.Normal);
         switch (orientation)
         {
            case (int)Android.Media.Orientation.Rotate90:
               return RotateImage(img, 90);
            case (int)Android.Media.Orientation.Rotate180:
               return RotateImage(img, 180);
            case (int)Android.Media.Orientation.Rotate270:
               return RotateImage(img, 270);
            default:
               return img;
         }
      }

      private static Bitmap RotateImage(Bitmap img, int degree)
      {
         Matrix matrix = new Matrix();
         matrix.PostRotate(degree);
         Bitmap rotatedImg = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
         img.Recycle();
         return rotatedImg;
      }
   }
}

