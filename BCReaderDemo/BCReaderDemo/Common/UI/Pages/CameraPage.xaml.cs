using Leadtools.Camera.Xamarin;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Pages
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public partial class CameraPage : PopupPage
   {
      public static Color OverlayColor { get; } = Color.FromRgba(235, 242, 255, 225);
      public static Color ViewBackgroundColor { get; } = Color.FromRgb(40, 40, 40);
      public static uint AnimationDuration { get; } = 150;

      private bool CanUseCamera { get; set; } = (Device.RuntimePlatform == Device.UWP) ? false : true;
      private bool ResumeWithCamera { get; set; } = true;
      private bool TakingPhoto { get; set; } = false;

      private CameraView CameraView = null;

      public event EventHandler<CameraPageClosingEventArgs> PageClosing;

      public CameraPage()
      {
         InitializeComponent();

         if (Device.RuntimePlatform == Device.iOS)
            HasSystemPadding = false;

         InitializeCamera();
      }

      protected override async void OnAppearing()
      {
         base.OnAppearing();

         // Show the camera by default
         if (!RasterSupport.KernelExpired && CanUseCamera && ResumeWithCamera)
            await StartCamera();

         // Delay so the ad doesn't appear immediately
         await Task.Delay(1000);

         // Start ads
         Ads.Start();
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         // Kill the camera
         ResumeWithCamera = CameraView == null ? false : CameraView.EnablePreview;
         if (ResumeWithCamera)
            StopCamera();

         // Stop ads
         Ads.Stop();
      }

      protected override bool OnBackButtonPressed()
      {
         OnPageClosing(null);
         return true;
      }

      private async void OnPageClosing(RasterImage image)
      {
         StopCamera();
         PageClosing?.Invoke(this, new CameraPageClosingEventArgs(image));
         await PopupNavigation.Instance.PopAsync();
      }

      private async void InitializeCamera()
      {
         bool canUseGallery = true;

         // Camera control not supported for UWP
         if (Device.RuntimePlatform != Device.UWP)
         {
            // Check camera permissions
            if (!await PermissionType.Camera.VerifyAsync(false))
               CanUseCamera = false;
         }

         // Check gallery access
         if (!await PermissionType.Photos.VerifyAsync(false))
            canUseGallery = false;

         TakePhotoButton.IsEnabled = CanUseCamera;
         TakePhotoButton.Opacity = (CanUseCamera) ? 1.0 : 0.5;
         FlashButton.IsEnabled = CanUseCamera;
         FlashButton.Opacity = (CanUseCamera) ? 1.0 : 0.5;
         GalleryButton.IsEnabled = canUseGallery;
         GalleryButton.Opacity = (canUseGallery) ? 1.0 : 0.5;
      }

      public async Task StartCamera()
      {
         try
         {
            if (Device.RuntimePlatform == Device.UWP || !CanUseCamera)
               return;

            // Already have a camera?
            if (CameraView != null)
            {
               if (!CameraView.EnablePreview)
               {
                  CameraView.EnablePreview = true;
                  CameraView.Start();
               }
               CameraView.Camera.CaptureQuality = CaptureQuality.High;
               return;
            }

            // Create the camera
            CameraView = new CameraView()
            {
               BackgroundColor = ViewBackgroundColor
            };
            CameraView.CameraOptions.AutoRotateImage = true;

            // Hook into the events
            CameraView.PictureReceived += CameraView_PictureReceived;

            // Listen for tap events
            TapGestureRecognizer singleTapRecognizer = new TapGestureRecognizer()
            {
               NumberOfTapsRequired = 1
            };
            singleTapRecognizer.Tapped += CameraView_Tapped;
            CameraView.GestureRecognizers.Add(singleTapRecognizer);

            // Add the camera
            await Device.InvokeOnMainThreadAsync(() => CameraContainer.Content = CameraView);
            CameraView.Camera.CaptureQuality = CaptureQuality.High;
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to use camera: {ex.Message}", "OK");
         }
      }

      public void StopCamera()
      {
         if (CameraView != null && CameraView.EnablePreview)
         {
            CameraView.EnablePreview = false;
            CameraView.Stop();
         }
      }

      private void CameraView_PictureReceived(FrameHandlerEventArgs e)
      {
         if (e.Image != null)
            OnPageClosing(e.Image.Clone());
      }

      private void CameraView_Tapped(object sender, EventArgs e)
      {
         // Focus
         if (CameraView.Camera.IsFocusSupported)
            CameraView.Camera.Focus();
      }

      private void HomeButton_Tapped(object sender, EventArgs e)
      {
         OnPageClosing(null);
      }

      private async void GalleryButton_Tapped(object sender, EventArgs e)
      {
         bool canClickButton = true;
         try
         {
            // Prevent fast clicking
            await Device.InvokeOnMainThreadAsync(() => GalleryButton.IsEnabled = false);

            // Try to load an image
            using (Stream stream = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync())
            {
               if (stream != null)
               {
                  using (RasterImage image = await RasterImageLoader.RasterImageFromStream(stream))
                  {
                     OnPageClosing(image.Clone());
                  }
               }
            }
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to load image: {ex.Message}", "OK");
         }
         finally
         {
            // Allow the user to click again
            if (canClickButton)
               await Device.InvokeOnMainThreadAsync(() => GalleryButton.IsEnabled = true);
         }
      }

      private async void TakePhotoButton_Tapped(object sender, EventArgs e)
      {
         // Wait for previous call to finish
         if (TakingPhoto)
            return;
         TakingPhoto = true;

         try
         {
            if (CameraView.EnablePreview)
            {
               // Take a photo
               CameraView.Camera.TakePicture();

               // Simple tapped effect
               await TakePhotoButton.ScaleTo(1.2, 200, Easing.SinIn);
               await TakePhotoButton.ScaleTo(1, 200, Easing.SinIn);
            }
            else
               await StartCamera();
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to take photo: {ex.Message}", "OK");
         }
         finally
         {
            // Finished
            TakingPhoto = false;
         }
      }

      private async void FlashButton_Tapped(object sender, EventArgs e)
      {
         if (FlashModesContainer.IsVisible)
            await HideFlashModesPanel();
         else
            await ShowFlashModesPanel();
      }

      private async void FlashImageButton_Tapped(object sender, EventArgs e)
      {
         await HideFlashModesPanel();

         string toastMessageText = string.Empty;
         SvgImage button = sender as SvgImage;
         if (button.StyleId.Equals("FlashDisabled"))
         {
            CameraView.CameraOptions.FlashMode = FlashMode.Off;
            toastMessageText = "Flash: Off";
         }
         else if (button.StyleId.Equals("FlashEnabled"))
         {
            CameraView.CameraOptions.FlashMode = FlashMode.On;
            toastMessageText = "Flash: On";
         }
         else if (button.StyleId.Equals("FlashAuto"))
         {
            CameraView.CameraOptions.FlashMode = FlashMode.Auto;
            toastMessageText = "Flash: Auto";
         }
         else if (button.StyleId.Equals("FlashTorch"))
         {
            CameraView.CameraOptions.FlashMode = FlashMode.Torch;
            toastMessageText = "Flash: Torch";
         }

         FlashButton.ResourceName = button.ResourceName;
         await DependencyService.Get<IToast>().Show(toastMessageText, false);
      }

      private void FlashModesContainer_SizeChanged(object sender, EventArgs e)
      {
         FlashModesTranslucentContainer.HeightRequest = (sender as StackLayout).Height + 20;
      }

      public async Task ShowFlashModesPanel()
      {
         await Animator.AnimatePanelAsync(FlashModesContainer, AnimationDirection.BottomToTop, true, AnimationDuration);
      }

      public async Task HideFlashModesPanel()
      {
         await Animator.AnimatePanelAsync(FlashModesContainer, AnimationDirection.BottomToTop, false, AnimationDuration);
      }
   }

   public class CameraPageClosingEventArgs : EventArgs
   {
      public CameraPageClosingEventArgs(RasterImage image)
      {
         Image = image;
      }

      public RasterImage Image { get; private set; }
   }
}
