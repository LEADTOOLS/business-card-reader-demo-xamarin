using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BCReaderDemo;
using BCReaderDemo.Droid;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;
using Java.IO;
using Android.Content.PM;
using System.ComponentModel;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(CameraPage), typeof(CameraPageRenderer))]
namespace BCReaderDemo.Droid
{
   class CameraPageRenderer : PageRenderer, TextureView.ISurfaceTextureListener, Android.Hardware.Camera.IPictureCallback
   {
#pragma warning disable CS0618
      Android.Hardware.Camera camera;
#pragma warning restore CS0618 
      Android.Widget.Button takePhotoButton;
      Android.Widget.Button toggleFlashButton;
      Android.Widget.Button switchCameraButton;
      Android.Views.View view;

      Activity activity;
      CameraFacing cameraType;
      TextureView textureView;
      SurfaceTexture surfaceTexture;
      ISurfaceHolder _holder;
      bool flashOn;
      Android.Content.Res.Orientation currentOrientation;


      public CameraPageRenderer(Context context) : base(context)
      {
      }


      protected override void OnConfigurationChanged(Configuration newConfig)
      {
         RemoveAllViews();
         base.OnConfigurationChanged(newConfig);
         if (newConfig.Orientation == Android.Content.Res.Orientation.Landscape)
         {
            currentOrientation = newConfig.Orientation;
            activity = this.Context as Activity;
            var display = activity.WindowManager.DefaultDisplay;
            view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout_Landscape, this, false);
            cameraType = CameraFacing.Back;
            textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
                textureView.SetForegroundGravity(GravityFlags.Center);
            textureView.SurfaceTextureListener = this;
            SetupEventHandlers();
            AddView(view);                
         }
         else if (newConfig.Orientation == Android.Content.Res.Orientation.Portrait)
         {
            currentOrientation = newConfig.Orientation;
            SetupUserInterface();
            SetupEventHandlers();
            AddView(view);
         }

      }




      protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
      {
         base.OnElementChanged(e);
         if (e.OldElement != null || Element == null)
         {
            return;
         }
         try
         {
            SetupUserInterface();
            SetupEventHandlers();
            AddView(view);

         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine(@"ERROR: ", ex.Message);
         }
      }

      void SetupUserInterface()
      {
         activity = this.Context as Activity;
         activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
         activity.Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
         var display = activity.WindowManager.DefaultDisplay;
         view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);
         cameraType = CameraFacing.Back;
         textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
         textureView.SurfaceTextureListener = this;
      }

      void SetupEventHandlers()
      {
         takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
         takePhotoButton.Click += TakePhotoButtonTapped;

         switchCameraButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.switchCameraButton);
         switchCameraButton.Click += SwitchCameraButtonTapped;

         toggleFlashButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.toggleFlashButton);
         toggleFlashButton.Click += ToggleFlashButtonTapped;
      }

      protected override void OnLayout(bool changed, int l, int t, int r, int b)
      {
         base.OnLayout(changed, l, t, r, b);

         var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
         var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

         view.Measure(msw, msh);
         view.Layout(0, 0, r - l, b - t);

         //Android.Views.View rectangleView = FindViewById<Android.Views.View>(Resource.Id.myRectangleView);

         //int top = (int)(0.05 * textureView.Height);
         //int bottom = (int)(0.80 * textureView.Height);
         //int height = bottom - top;

         //int width = (int)(height * 1.5);
         //int left = (textureView.Width - width) / 2;

         //rectangleView.Left = left;
         //rectangleView.Top = top;
         //rectangleView.Bottom = bottom;
         //rectangleView.Right = left + width;

         //rectangleView.Invalidate();
      }

      public void OnSurfaceTextureUpdated(SurfaceTexture surface)
      {

      }

      public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
      {
#pragma warning disable CS0618 
         camera = Android.Hardware.Camera.Open((int)cameraType);
#pragma warning restore CS0618 
         var parameters = camera.GetParameters();
         parameters.PictureFormat = ImageFormatType.Jpeg;
#pragma warning disable CS0618 
         parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
#pragma warning restore CS0618 
         camera.SetParameters(parameters);
         textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
         surfaceTexture = surface;
         camera.SetPreviewTexture(surfaceTexture);
         PrepareAndStartCamera();
      }

      public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
      {
         camera.StopPreview();
         camera.Release();
         camera = null;

         return true;
      }

      public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
      {
         PrepareAndStartCamera();
      }

        private Android.Hardware.Camera.Size GetBestPictureSize()
        {
            Android.Hardware.Camera.Parameters parameters = camera.GetParameters();
            var display = activity.WindowManager.DefaultDisplay;

            double displayAspectRatio = Math.Max(display.Width, display.Height) / (double)Math.Min(display.Width, display.Height);

            Android.Hardware.Camera.Size maxSize = null;

            foreach (var size in parameters.SupportedPictureSizes)
            {
                int width = Math.Max(size.Width, size.Height);
                int height = Math.Min(size.Width, size.Height);

                double sizeAspectRatio = width / (double)height;

                if (Math.Abs(sizeAspectRatio - displayAspectRatio) < 0.2)
                {
                    if (maxSize == null)
                    {
                        maxSize = size;
                        continue;
                    }

                    if (size.Width > maxSize.Width)
                    {
                        maxSize = size;
                    }
                }
            }

            return maxSize;
        }

        private Android.Hardware.Camera.Size GetBestPreviewSize()
        {
            Android.Hardware.Camera.Parameters parameters = camera.GetParameters();
            var display = activity.WindowManager.DefaultDisplay;

            double displayAspectRatio = Math.Max(display.Width, display.Height) / (double) Math.Min(display.Width, display.Height);

            Android.Hardware.Camera.Size maxSize = null;

            foreach (var size in parameters.SupportedPreviewSizes)
            {
                int width = Math.Max(size.Width, size.Height);
                int height = Math.Min(size.Width, size.Height);

                double sizeAspectRatio = width / (double)height;

                if(Math.Abs(sizeAspectRatio - displayAspectRatio) < 0.2)
                {
                    if(maxSize == null)
                    {
                        maxSize = size;
                        continue;
                    }

                    if (size.Width > maxSize.Width)
                    {
                        maxSize = size;
                    }
                }
            }

            return maxSize;
        }

        void PrepareAndStartCamera()
        {
            camera.StopPreview();

            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;
            Android.Hardware.Camera.CameraInfo info = new Android.Hardware.Camera.CameraInfo();
            Android.Hardware.Camera.GetCameraInfo((int)cameraType, info);

            Android.Hardware.Camera.Size previewSize = GetBestPreviewSize();
            Android.Hardware.Camera.Size pictureSize = GetBestPictureSize();
            Android.Hardware.Camera.Parameters parameters = camera.GetParameters();

            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0: degrees = 0; break;
                case SurfaceOrientation.Rotation90: degrees = 90; break;
                case SurfaceOrientation.Rotation180: degrees = 180; break;
                case SurfaceOrientation.Rotation270: degrees = 270; break;
            }

            if (previewSize.Height > previewSize.Width) 
                parameters.SetPreviewSize(previewSize.Height, previewSize.Width);
            else
                parameters.SetPreviewSize(previewSize.Width, previewSize.Height);

            if (pictureSize.Height > pictureSize.Width)
                parameters.SetPictureSize(pictureSize.Height, pictureSize.Width);
            else
                parameters.SetPictureSize(pictureSize.Width, pictureSize.Height);
            if (info.Facing == CameraFacing.Front)
            {
                degrees = (info.Orientation + degrees) % 360;
                degrees = (360 - degrees) % 360;
                camera.SetDisplayOrientation(degrees);
            }
            else
            {
                degrees = (info.Orientation - degrees + 360) % 360;
                camera.SetDisplayOrientation(degrees);
            }

            parameters.SetRotation(degrees);
            camera.SetParameters(parameters);
            
            //camera.SetParameters(parameters);
          
         camera.StartPreview();
      }

      void ToggleFlashButtonTapped(object sender, EventArgs e)
      {

         flashOn = !flashOn;
         if (flashOn)
         {
            if (cameraType == CameraFacing.Back)
            {
               toggleFlashButton.SetBackgroundResource(Resource.Drawable.FlashButton);
               cameraType = CameraFacing.Back;

               camera.StopPreview();
               camera.Release();
               camera = Android.Hardware.Camera.Open((int)cameraType);
               var parameters = camera.GetParameters();
               parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
               parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
               camera.SetParameters(parameters);
               camera.SetPreviewTexture(surfaceTexture);
               PrepareAndStartCamera();
            }
         }
         else
         {
            toggleFlashButton.SetBackgroundResource(Resource.Drawable.NoFlashButton);
            camera.StopPreview();
            camera.Release();

            camera = Android.Hardware.Camera.Open((int)cameraType);
            var parameters = camera.GetParameters();
            parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
            parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
            camera.SetParameters(parameters);
            camera.SetPreviewTexture(surfaceTexture);
            PrepareAndStartCamera();
         }
      }

      void SwitchCameraButtonTapped(object sender, EventArgs e)
      {
         if (cameraType == CameraFacing.Front)
         {
            cameraType = CameraFacing.Back;

            camera.StopPreview();
            camera.Release();
#pragma warning disable CS0618 
            camera = Android.Hardware.Camera.Open((int)cameraType);
#pragma warning restore CS0618 
            var parameters = camera.GetParameters();
#pragma warning disable CS0618 
            parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
#pragma warning restore CS0618 
            camera.SetParameters(parameters);
            camera.SetPreviewTexture(surfaceTexture);
            PrepareAndStartCamera();
         }
         else
         {
            cameraType = CameraFacing.Front;

            camera.StopPreview();
            camera.Release();
#pragma warning disable CS0618 
            camera = Android.Hardware.Camera.Open((int)cameraType);
#pragma warning restore CS0618 
            camera.SetPreviewTexture(surfaceTexture);

            PrepareAndStartCamera();
         }
      }


      async void TakePhotoButtonTapped(object sender, EventArgs e)
      {
         try
         {
            camera.TakePicture(null, null, this);
            //camera.StopPreview();
         }
         catch (Exception ex)
         {
            int debug = 0;
         }

      }

      private void DrawRectangle(Bitmap image, Rect rect, Android.Graphics.Color color)
      {
         for (int y = rect.Top; y <= rect.Bottom; y++)
         {
            int x1 = rect.Left;
            int x2 = rect.Right;
            if (x2 > image.Width)
            {
               int i = 0;

            }
            if (y < image.Height && x1 < image.Width)
            {
               image.SetPixel(x1, y, Android.Graphics.Color.Red);
            }

            if (y < image.Height && x2 < image.Width)
            {
               image.SetPixel(x2, y, color);
            }

         }

         for (int x = rect.Left; x <= rect.Right; x++)
         {

            int y1 = rect.Top;
            int y2 = rect.Bottom;
            if (y2 > image.Height)
            {
               int ii = 0;

            }
            if (y1 < image.Height && x < image.Width)
            {
               image.SetPixel(x, y1, color);

            }

            if (y2 < image.Height && x < image.Width)
            {
               image.SetPixel(x, y2, color);
            }

         }

         Xamarin.Forms.Point center = new Xamarin.Forms.Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);

         if (center.X > 0 && center.X < image.Width && center.Y > 0 && center.Y < image.Height)
         {
            for (int x = (int)(center.X - 2); x < (int)(center.X + 3); ++x)
            {
               image.SetPixel(x, (int)center.Y, color);
            }

            for (int y = (int)(center.Y - 2); y < (int)(center.Y + 3); ++y)
            {
               image.SetPixel((int)center.X, y, color);
            }
         }
      }

      private void SaveImage(Bitmap image, string fileName)
      {
         Java.IO.File dataDir = Android.OS.Environment.ExternalStorageDirectory;

         byte[] imageBytes;
         MemoryStream imageStream = new MemoryStream();
         Task.Run(async () =>
         {
            await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, imageStream);
         }).Wait();

         imageBytes = imageStream.ToArray();

         FileOutputStream outStream = null;
         if (imageBytes != null)
         {
            try
            {
               outStream = new FileOutputStream(dataDir + "/" + fileName);
               outStream.Write(imageBytes);
               outStream.Close();
            }
            catch (System.IO.FileNotFoundException e)
            {
               System.Console.Out.WriteLine(e.Message);
            }
            catch (System.IO.IOException ie)
            {
               System.Console.Out.WriteLine(ie.Message);
            }
         }
      }

      public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
      {
         try
         {
            camera.StopPreview();
            Android.Views.View rectangleView = FindViewById<Android.Views.View>(Resource.Id.myRectangleView);


            Rect previewRect = new Rect(rectangleView.Left, rectangleView.Top, rectangleView.Right, rectangleView.Bottom);
            Bitmap previewImage = textureView.Bitmap;
            Bitmap tempImage = BitmapFactory.DecodeByteArray(data, 0, data.Length);

            Matrix mm = new Matrix();
                //Bitmap tempStillImage = null;


                //mm.PostRotate(90);

                //tempStillImage = Bitmap.CreateBitmap(tempImage, 0, 0, tempImage.Width, tempImage.Height, mm, true);

                bool isRotatedImage = false;

                if (tempImage.Width > tempImage.Height)
                    isRotatedImage = true;

            DrawRectangle(previewImage, previewRect, Android.Graphics.Color.Yellow);

            //var parameters = camera.GetParameters();
            int width = previewImage.Width;
            int height = previewImage.Height;

            float scaleX = (isRotatedImage ? tempImage.Height : tempImage.Width) / (float)width;
            float scaleY = (isRotatedImage ? tempImage.Width : tempImage.Height) / (float)height;

            //Rect stillImageRect = new Rect((int) (previewRect.Right * scaleX), (int) (previewRect.Left * scaleY), (int) (previewRect.Bottom * scaleX), (int) (previewRect.Top * scaleY));
            //DrawRectangle(stillImage, stillImageRect, Android.Graphics.Color.Yellow);

            Rect stillImageRect = new Rect((int)(previewRect.Left * scaleX), (int)(previewRect.Top * scaleY), (int)(previewRect.Right * scaleX), (int)(previewRect.Bottom * scaleY));
            //DrawRectangle(tempImage, stillImageRect, Android.Graphics.Color.Yellow);
            SaveImage(tempImage, "TempImage.jpg");

            Matrix mmmm = new Matrix();
            mmmm.SetRotate(-90);

            if (isRotatedImage)
            {
                //Rotate rect
                stillImageRect = new Rect((int)(previewRect.Top * scaleY), (int)(previewRect.Left * scaleX), (int)(previewRect.Bottom * scaleY), (int)(previewRect.Right * scaleX));
                mmmm.SetRotate(0);
            }

            Bitmap stillImage = Bitmap.CreateBitmap(tempImage, stillImageRect.Left, stillImageRect.Top, stillImageRect.Width(), stillImageRect.Height(), mmmm, true);
                
            SaveImage(previewImage, "PreviewImage.jpg");
            SaveImage(stillImage, "StillImage.jpg");

            //tempStillImage.Recycle();
            previewImage.Recycle();
                tempImage.Recycle();

                //Bitmap image = BitmapFactory.DecodeByteArray(data, 0, data.Length);



                //Bitmap resized = Bitmap.CreateBitmap(image, (int)(rectangleView.Left * scaleX), (int)(rectangleView.Top * scaleY), (int)(rectangleView.Right * scaleX - rectangleView.Left * scaleX), (int)(rectangleView.Bottom * scaleY - rectangleView.Top * scaleY));

                MemoryStream stream = new MemoryStream();

            Task.Run(async () =>
            {
               await stillImage.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, stream);
            }).Wait();


            stillImage.Recycle();
            //image.Recycle();
            //resized.Recycle();

            byte[] imageBytes = stream.ToArray();

            //byte[] imageBytes = data;

            ImageEventArgs args = new ImageEventArgs();
            args.PictureTaken = true;
            args.imageData = imageBytes;
            ((CameraPage)Element).OnPictureTaken(args);


            activity.Window.AddFlags(WindowManagerFlags.ForceNotFullscreen);
            activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);
            camera.StartPreview();
         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine(@"Something has gone horribly wrong", ex.Message);
         }
      }

      public void OnShutter()
      {
         //throw new NotImplementedException();
      }

      //async void TakePhotoButtonTapped(object sender, EventArgs e)
      //{
      //   //camera.StopPreview();

      //      //camera.TakePicture(this, this, this);


      //   var image = textureView.Bitmap;
      //   Byte[] imageBytes;

      //   try
      //   {

      //      MemoryStream stream = new MemoryStream();
      //          //Task.Run(async () =>
      //          //{
      //          //   await image.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);

      //          //   //Bitmap resized = Bitmap.CreateScaledBitmap(image, (int)(image.Width - (image.Width * 0.25)), (int)(image.Height - (image.Height * 0.25)), false);

      //          //   //resized.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);

      //          //}).Wait();

      //          Android.Views.View rectangleView = FindViewById<Android.Views.View>(Resource.Id.myRectangleView);

      //          //Bitmap resized = Bitmap.CreateScaledBitmap(image, (int)(image.Width - (image.Width * 0.25)), (int)(image.Height - (image.Height * 0.25)), false);
      //          Bitmap resized = Bitmap.CreateBitmap(image, rectangleView.Left, rectangleView.Top, rectangleView.Width, rectangleView.Height);

      //          Task.Run(async () =>
      //      {
      //          //await image.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);

      //          //Bitmap resized = Bitmap.CreateScaledBitmap(image, (int)(image.Width - (image.Width * 0.25)), (int)(image.Height - (image.Height * 0.25)), false);

      //          await resized.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, stream);

      //      }).Wait();


      //      imageBytes = stream.ToArray();
      //      ImageEventArgs args = new ImageEventArgs();
      //      args.PictureTaken = true;
      //      args.imageData = imageBytes;
      //      ((CameraPage)Element).OnPictureTaken(args);


      //      ////ByteArrayOutputStream byteOut = new ByteArrayOutputStream();
      //      //MemoryStream stream = new MemoryStream();
      //      //image.Compress(Bitmap.CompressFormat.Png, 100, stream);
      //      //var imageBytes = stream.ToArray();

      //      ////Bitmap resized = Bitmap.CreateScaledBitmap(image, (int)(image.Width - (image.Width * 0.25)), (int)(image.Height - (image.Height * 0.25)), false);

      //      ////resized.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);

      //      //ImageEventArgs args = new ImageEventArgs();
      //      //args.PictureTaken = true;
      //      //args.imageData = imageBytes;
      //      //((CameraPage)Element).OnPictureTaken(args);


      //      //ImageEventArgs args = new ImageEventArgs();
      //      //args.PictureTaken = true;
      //      //args.imageData = imageBytes;
      //      //((CameraPage)Element).OnPictureTaken(args);

      //      //BarcodeDemo.MainPage rootPage = BarcodeDemo.MainPage.MyMainPage as MainPage;
      //      //rootPage.bytesFromCamera = imageBytes;
      //      //await rootPage.Navigation.PopToRootAsync(true);

      //      //await App.Current.MainPage.Navigation.PushAsync(new ImagePage(imageBytes));

      //      //var absolutePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
      //      //var folderPath = absolutePath;
      //      //var filePath = System.IO.Path.Combine(folderPath, string.Format("photo_{0}.jpg", Guid.NewGuid()));

      //      //var fileStream = new FileStream(filePath, FileMode.Create);
      //      //await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 50, fileStream);
      //      //fileStream.Close();
      //      //image.Recycle();

      //      //var intent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
      //      //var file = new Java.IO.File(filePath);
      //      //var uri = Android.Net.Uri.FromFile(file);
      //      //intent.SetData(uri);
      //      //Forms.Context.SendBroadcast(intent);
      //   }
      //   catch (Exception ex)
      //   {
      //      System.Diagnostics.Debug.WriteLine(@"Something has gone horribly wrong", ex.Message);
      //   }

      //   //camera.StartPreview();
      //}
   }
}