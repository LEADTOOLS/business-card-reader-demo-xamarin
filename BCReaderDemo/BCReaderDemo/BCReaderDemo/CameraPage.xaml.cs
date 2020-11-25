// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools;
using Leadtools.Camera.Xamarin;
using Leadtools.Demos;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   public enum CameraOperationType
   {
      Profile,
      BackImage,
      Normal
   }

   public class PictureTakenEventArgs : EventArgs
   {
      public RasterImage Image { get; set; }
      public Stream Stream { get; set; }
      public CameraOperationType OperationType { get; set; }
      public PictureTakenEventArgs(RasterImage image, Stream stream, CameraOperationType operationType)
      {
         Image = image;
         Stream = stream;
         OperationType = operationType;
      }
   }

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class CameraPage : PopupPage
   {
      public delegate void PictureTakenEventHandler(object source, PictureTakenEventArgs e);
      public event PictureTakenEventHandler PictureTaken;
      public bool AppResumed = false;

      private AutoCapture _autoCapture;
      private CameraOperationType _currentOperation;
      private bool _cameraObjectNotReady = false;

      private bool _enableAutoCapture;
      private bool _hold = false;
      private Stopwatch _captureWatch;
      private Timer _checkHoldTimer;
      private bool _checkStability = false;
      private bool _captureImage = false;
      private bool _autoRotateImage = true;
      private bool cleanup = true;
      private bool firstFrame = true;
      public CameraOperationType CurrentOperation
      {
         get { return _currentOperation; }
         set
         {
            _currentOperation = value;
            _enableAutoCapture = HomePage.CurrentAppData.EnableCameraAutoCapture && _currentOperation != CameraOperationType.Profile;
         }
      }

      public CameraPage(CameraOperationType currentOperation)
      {
         InitPage(currentOperation);
      }

      public CameraPage(CameraOperationType currentOperation, bool autoRotateImage)
      {
         _autoRotateImage = autoRotateImage;
         InitPage(currentOperation);
      }

      private void InitPage(CameraOperationType currentOperation)
      {
         CurrentOperation = currentOperation;
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif
         InitFormsUI();
         _autoCapture = new AutoCapture();

         if (CurrentOperation == CameraOperationType.Normal)
            _autoCapture.CaptureMethod = AutoCapture.AutoCaptureMethod.TextDetection;
         else if (CurrentOperation == CameraOperationType.BackImage)
            _autoCapture.CaptureMethod = AutoCapture.AutoCaptureMethod.DocumentDetection;

         leadCamera.SizeChanged += LeadCamera_SizeChanged;
      }

      private void InitFormsUI()
      {
         overlayGrid.HeightRequest = DemoUtilities.DisplayHeight;

         if (Xamarin.Forms.Device.Idiom == TargetIdiom.Tablet)
         {
            var aspectRatio = 16.0 / 9.0;
            var padding = 20;
            var aspectWidth = DemoUtilities.DisplayHeight / aspectRatio;
            var widthAdjustment = DemoUtilities.DisplayWidth - aspectWidth;
            var outerColumnWidth = (widthAdjustment / 2) - padding;

            c0.Width = new GridLength(outerColumnWidth);
            c8.Width = new GridLength(outerColumnWidth);

#if __ANDROID__
            Device.BeginInvokeOnMainThread(() =>
            {
               row2.Height = new GridLength(1, GridUnitType.Star);
               Grid.SetColumn(overlayGrid, 1);
               Grid.SetColumnSpan(overlayGrid, 7);
            });
#endif
         }
      }

      private void LeadCamera_SizeChanged(object sender, EventArgs e)
      {
         if(_cameraObjectNotReady)
         {
            InitLeadCam();
            _cameraObjectNotReady = false;
         }
      }

      private void InitLeadCam()
      {
         if (leadCamera.Camera == null)
         {
            _cameraObjectNotReady = true;
            return;
         }

         if (!cleanup || AppResumed)
         {
            leadCamera.Camera.Start();
            //reset the clean up flag
            cleanup = true;
            AppResumed = false;

            leadCamera.RotationChanged += LeadCamera_RotationChanged;
            leadCamera.PictureReceived += LeadCamera_PictureReceived;


            if (_enableAutoCapture)
            {
               leadCamera.FrameReceived += LeadCamera_FrameReceived;
            }

            return;
         }

         if (_enableAutoCapture)
         {
            Animation labelFadeAnimation = new Animation();

            Animation fadeOutAnimation = new Animation
            (
               callback: d => autoCaptureLabel.Opacity = d,
               start: 1,
               end: 0.25,
               easing: Easing.SinInOut
            );

            Animation fadeInAnimation = new Animation
            (
               callback: e =>
               {
                  autoCaptureLabel.Opacity = e;
               },
               start: 0.25,
               end: 1,
               easing: Easing.SinInOut
            );

            labelFadeAnimation.Add(0, 0.5, fadeOutAnimation);
            labelFadeAnimation.Add(0.5, 1, fadeInAnimation);

            autoCaptureLabel.Animate("FadeInOut", labelFadeAnimation, length: 1000, easing: Easing.SinInOut, repeat: () => true);
            UpdateCardDetectionStatus();
         }

         leadCamera.RotationChanged += LeadCamera_RotationChanged;
         leadCamera.PictureReceived += LeadCamera_PictureReceived;

         leadCamera.CameraOptions.AutoRotateImage = _autoRotateImage;
         leadCamera.Camera.FocusMode = FocusMode.Continuous;
#if __IOS__
         var ioscam = leadCamera.Camera.NativeCamera as AVFoundation.AVCaptureSession;
         if(ioscam != null)
         {
            var presetValue = ioscam.SessionPreset;
            if(HomePage.CurrentAppData.CameraQuality == Utils.CameraQuality.Medium)
            {
               presetValue = new Foundation.NSString("AVCaptureSessionPreset1920x1080");
            }
            else
            {
               presetValue = new Foundation.NSString("AVCaptureSessionPreset3840x2160");
            }

            try
            {
               foreach (AVFoundation.AVCaptureOutput output in ioscam.Outputs)
               {
                  if (output is AVFoundation.AVCaptureStillImageOutput stillImageOutput)
                  {
                     stillImageOutput.HighResolutionStillImageOutputEnabled = (HomePage.CurrentAppData.CameraQuality == Utils.CameraQuality.High) ? true : false;
                  }
               }
               ioscam.SessionPreset = presetValue;
            }
            catch(Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
         }
#else
         //set capture quality setting
#pragma warning disable CS0618 // Type or member is obsolete
         var droidcam = leadCamera.Camera.NativeCamera as Android.Hardware.Camera;
         var parameters = droidcam.GetParameters();
         var pictureSizes = parameters.SupportedPictureSizes;
         if (pictureSizes != null)
         {
            Android.Hardware.Camera.Size closestMatch = null;
            if (HomePage.CurrentAppData.CameraQuality == Utils.CameraQuality.Medium)
            {
               // Get the first camera size with width bigger than 2000
               var closestMatches = pictureSizes.OrderByDescending(x => x.Width).Where(x => x.Width > 2000);
               if (closestMatches != null && closestMatches.Count() > 0)
                  closestMatch = closestMatches.Last();
            }
            else
            {
               var closestMatches = pictureSizes.OrderByDescending(x => x.Width).Where(x => x.Width > 3800);
               if (closestMatches != null && closestMatches.Count() > 0)
                  closestMatch = closestMatches.Last();
            }

            if (closestMatch == null)
               closestMatch = pictureSizes.OrderByDescending(x => x.Height).First();

            parameters.SetPictureSize(closestMatch.Width, closestMatch.Height);
            droidcam.SetParameters(parameters);
         }
#pragma warning restore CS0618 // Type or member is obsolete
#endif

         autoCaptureGrid.IsVisible = _enableAutoCapture;
         if (_enableAutoCapture)
            leadCamera.FrameReceived += LeadCamera_FrameReceived;
      }

      private void UpdateCardDetectionStatus()
      {
         autoCaptureLabel.Text = HomePage.BcReader.RecognizeQRCode ? "Detecting Business Card & QR..." : "Detecting Business Card...";
         autoCaptureLabel.TextColor = Color.White;
      }
      
      private void LeadCamera_FrameReceived(FrameHandlerEventArgs e)
      {
         if (e.Image == null)
            return;

         if (leadCamera.Camera.FocusMode != FocusMode.Continuous)
            leadCamera.Camera.FocusMode = FocusMode.Continuous;

         if (firstFrame)
         {
#if __ANDROID__
            if (Android.OS.Build.Manufacturer == "samsung")
            {
               leadCamera.Camera.Focus();
            }
#endif // #if __ANDROID__

            firstFrame = false;
         }

         try
         {
            if (!_hold)
               _autoCapture.CheckStability(e.Image);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         if (_hold && _checkStability)
         {
            _checkStability = false;

            bool holdStable = _autoCapture.CheckHoldStability(e.Image);

            if (!holdStable)
            {
               _captureWatch.Stop();
               _captureWatch = null;
               _hold = false;
               _autoCapture.Reset();

               Device.BeginInvokeOnMainThread(() =>
               {
                  UpdateCardDetectionStatus();
               });
            }
            else if (holdStable && _captureWatch.ElapsedMilliseconds > 2000)
            {
               leadCamera.FrameReceived -= LeadCamera_FrameReceived;

               _captureImage = true;
               leadCamera.Camera.FocusMode = FocusMode.Auto;
               leadCamera.FocusCompleted += leadCamera_FocusCompleted;
               leadCamera.Camera.Focus();
            }
         }

         e.Image.Dispose();

         if (_autoCapture.IsStable)
         {
            if (CurrentOperation == CameraOperationType.BackImage)
            {
               leadCamera.FrameReceived -= LeadCamera_FrameReceived;

               _captureImage = true;
               leadCamera.Camera.FocusMode = FocusMode.Auto;
               leadCamera.FocusCompleted += leadCamera_FocusCompleted;
               leadCamera.Camera.Focus();

               return;
            }

            if (_captureWatch == null)
            {
               _captureWatch = Stopwatch.StartNew();
               _autoCapture.Reset();
               _hold = true;

               _checkHoldTimer = new Timer();
               _checkHoldTimer.Interval = 600;
               _checkHoldTimer.Elapsed += _checkHoldTimer_Elapsed;
               _checkHoldTimer.Enabled = true;
               _checkHoldTimer.AutoReset = true;

               Device.BeginInvokeOnMainThread(() =>
               {
                  autoCaptureLabel.Text = "Please Hold On";
                  autoCaptureLabel.TextColor = Color.Yellow;
               });
            }
         }
      }

      private void leadCamera_FocusCompleted(Leadtools.Camera.Xamarin.FocusEventArgs e)
      {
         if (_captureImage && e.Success == true)
         {
            if (_enableAutoCapture)
            {
               leadCamera.FocusCompleted -= leadCamera_FocusCompleted;
               Device.BeginInvokeOnMainThread(() =>
               {
                  closeButton.IsEnabled = false;
                  flashButton.IsEnabled = false;
                  galleryButton.IsEnabled = false;
                  enableBarcodeRecognitionButton.IsEnabled = false;
                  takePhotoButton.IsEnabled = false;
               });
            }

            leadCamera.Camera.TakePicture();

            _captureImage = false;
         }
      }

      private void _checkHoldTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         _checkStability = true;
      }

      protected override void OnAppearing()
      {
         DependencyService.Get<IStatusBar>().HideStatusBar();

         base.OnAppearing();

         UpdateCardDetectionStatus();
         InitLeadCam();
      }

      protected override void OnDisappearing()
      {
         DependencyService.Get<IStatusBar>().ShowStatusBar();
         leadCamera.Stop();
         leadCamera.RotationChanged -= LeadCamera_RotationChanged;
         leadCamera.PictureReceived -= LeadCamera_PictureReceived;
         leadCamera.FrameReceived -= LeadCamera_FrameReceived;

         if (cleanup)
         {
#if __IOS__
            leadCamera.Dispose();
#endif
            base.OnDisappearing();
         }
      }

      private void LeadCamera_RotationChanged(Leadtools.Camera.Xamarin.RotationEventArgs e)
      {
         var rotateToValue = 0;
         if (e.InitialAngle != 0)
         {
            rotateToValue = 360 - e.InitialAngle;
         }

         switch (e.CurrentOrientation)
         {
            case DeviceOrientation.Portrait:
               flashButton.RotateTo(rotateToValue, 250, Easing.CubicInOut);
               galleryButton.RotateTo(rotateToValue, 250, Easing.CubicInOut);
               enableBarcodeRecognitionButton.RotateTo(rotateToValue, 250, Easing.CubicInOut);
               autoCaptureGrid.RotateTo(rotateToValue, 250, Easing.CubicInOut);
               break;
            case DeviceOrientation.LandscapeLeft:
               flashButton.RotateTo(rotateToValue + 90, 250, Easing.CubicInOut);
               galleryButton.RotateTo(rotateToValue + 90, 250, Easing.CubicInOut);
               enableBarcodeRecognitionButton.RotateTo(rotateToValue + 90, 250, Easing.CubicInOut);
               autoCaptureGrid.RotateTo(rotateToValue + 90, 250, Easing.CubicInOut);
               break;
            case DeviceOrientation.LandscapeRight:
               flashButton.RotateTo(rotateToValue - 90, 250, Easing.CubicInOut);
               galleryButton.RotateTo(rotateToValue - 90, 250, Easing.CubicInOut);
               enableBarcodeRecognitionButton.RotateTo(rotateToValue - 90, 250, Easing.CubicInOut);
               autoCaptureGrid.RotateTo(rotateToValue - 90, 250, Easing.CubicInOut);
               break;
            case DeviceOrientation.PortraitUpsideDown:
               flashButton.RotateTo(rotateToValue - 180, 250, Easing.CubicInOut);
               galleryButton.RotateTo(rotateToValue - 180, 250, Easing.CubicInOut);
               enableBarcodeRecognitionButton.RotateTo(rotateToValue - 180, 250, Easing.CubicInOut);
               autoCaptureGrid.RotateTo(rotateToValue - 180, 250, Easing.CubicInOut);
               break;
            default:
               break;
         }
      }

      private void LeadCamera_PictureReceived(Leadtools.Camera.Xamarin.FrameHandlerEventArgs e)
      {
         if (_enableAutoCapture)
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               autoCaptureLabel.AbortAnimation("FadeInOut");
               autoCaptureLabel.Opacity = 1;
               autoCaptureLabel.Text = "Business Card Detected";
            });
         }

         if (CurrentOperation == CameraOperationType.Normal)
         {
            if (e.Image != null)
            {
               CreateUpdateBusinessCardPage(e.Image);

               if (e.Image != null && !e.Image.IsDisposed)
                  e.Image.Dispose();
            }
         }
         else
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               HomePage.Instance.PopCameraPage();
               PictureTaken?.Invoke(this, new PictureTakenEventArgs(e.Image, null, CurrentOperation));
            });
         }
      }

      private void CreateUpdateBusinessCardPage(RasterImage image, Stream stream = null)
      {
         UpdateBusinessCardPage page = null;

         if (stream != null)
            page = new UpdateBusinessCardPage(stream, null, null, -1, null, false);
         else
            page = new UpdateBusinessCardPage(null, image, null, -1, null, false);

         page.ContactSaved += HomePage.Instance.DetailsPage_ContactSaved;

         Device.BeginInvokeOnMainThread(async () =>
         {
            HomePage.Instance.PopCameraPage();
            await PopupNavigation.Instance.PushAsync(page, true);
         });
      }

      private void LeadCamera_Tapped(object sender, EventArgs e)
      {
         leadCamera.Camera.Focus();
      }

      private async void GalleryButton_Tapped(object sender, EventArgs e)
      {
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Photos);
         if (results == null || results[PermissionType.Photos] != PermissionStatus.Granted)
            return;

         cleanup = false;
         try
         {
            Stream stream = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
            if (stream != null)
            {
               if (CurrentOperation == CameraOperationType.Normal)
               {
                  CreateUpdateBusinessCardPage(null, stream);
               }
               else
               {
                  Device.BeginInvokeOnMainThread(() =>
                  {
                     HomePage.Instance.PopCameraPage();
                     PictureTaken?.Invoke(this, new PictureTakenEventArgs(null, stream, CurrentOperation));
                  });
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }


      private void ToggleBarcodeRecognitionButton_Tapped(object sender, EventArgs e)
      {
            Device.BeginInvokeOnMainThread(() =>
            {
                HomePage.BcReader.RecognizeQRCode = !HomePage.BcReader.RecognizeQRCode;
                if (!_hold)
                {
                    UpdateCardDetectionStatus();
                }
            });
      }

      private void CloseButton_Tapped(object sender, EventArgs e)
      {
         HomePage.Instance.PopCameraPage();
      }

      private void FlashButton_Tapped(object sender, EventArgs e)
      {
         if (leadCamera.CameraOptions.FlashMode == Leadtools.Camera.Xamarin.FlashMode.Torch)
         {
            leadCamera.CameraOptions.FlashMode = Leadtools.Camera.Xamarin.FlashMode.Off;
            flashButton.ResourceName = "Icons/flash-off.svg";
         }
         else
         {
            leadCamera.CameraOptions.FlashMode = Leadtools.Camera.Xamarin.FlashMode.Torch;
            flashButton.ResourceName = "Icons/flash-on.svg";
         }
      }

      private void TakePhoto_Tapped(object sender, EventArgs e)
      {
         if (_enableAutoCapture)
         {
            _autoCapture.Reset();
            leadCamera.FrameReceived -= LeadCamera_FrameReceived;
         }

         takePhotoButton.IsEnabled = false;
         flashButton.IsEnabled = false;
         galleryButton.IsEnabled = false;
         enableBarcodeRecognitionButton.IsEnabled = false;

         autoCaptureLabel.AbortAnimation("FadeInOut");
         autoCaptureLabel.IsVisible = false;

         _captureImage = true;
         _enableAutoCapture = false;
         leadCamera.Camera.TakePicture();
      }
   }
}
