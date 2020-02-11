// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Extentions;
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using CarouselView.FormsPlugin.Abstractions;
using DataService;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.Controls;
using Leadtools.Demos.Utils;
using Leadtools.Forms.Commands;
using Leadtools.ImageProcessing.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class UpdateBusinessCardPage : ContentPage
   {
      public static UpdateBusinessCardPage Instance { get; private set; }

      private ImageViewer _imageViewer = null;
      private ImageViewer _backImageViewer = null;
      private BoxView _overlayBox;
      private string _thumbnailPath;
      private string _picturePath;
      private string _backImagePath;
      private RasterImage _rasterImage;
      private RasterImage _backImage;
      private RasterImage _rasterThumbnail;
      private int _index;
      private bool _wait = true;
      private Stream _cameraStream = null;
      private bool _existing = false;
      private bool _cardProcessed = false;
      private bool _processingCard = false;
      private List<string> _tempPaths = new List<string>();
      private BusinessCardReader _bcReader;
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;
      private int _numberOfFailedInitializations = 0;

      public ContactCodePath SaveType { get; set; }
      public ImageViewer ImageViewer
      {
         get { return _imageViewer; }
      }
      public Grid ContentLayoutGrid
      {
         get { return contentLayout; }
      }
      public MyCarouselViewControl CarouselView
      {
         get { return carouselView; }
      }

      public bool ContactModified { get; set; }

      public event EventHandler<ContactSavedEventArgs> ContactSaved;
      public ContactModel ContactItem { get; set; }
      public RasterImage DeskewedImage { get => _rasterImage; set => _rasterImage = value; }
      public RasterImage BackImage
      {
         get { return _backImage; }
         set
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               setAsMainLabel.IsVisible = (value != null);
               setAsMainLabel.IsEnabled = (value != null);

               addBackImageGrid.IsVisible = (value == null);
               addBackImageGrid.IsEnabled = (value == null);
               _backImage = value;
               _backImageViewer.Image = value;
               if (value != null)
                  _backImageViewer.Zoom(ControlSizeMode.FitAlways, 1.0, _backImageViewer.DefaultZoomOrigin);
            });
         }
      }
      public BoxView FieldOverlay { get => _overlayBox; }

      public Action<object> BackButtonPressed { get; set; }

      public void OnContactSaved(ContactSavedEventArgs args)
      {
         ContactSaved?.Invoke(this, args);
      }

      public enum ContactCodePath
      {
         Edit,
         Gallery,
         Camera
      }

      public event EventHandler PageClosing;

      public UpdateBusinessCardPage(Stream cameraStream, RasterImage image, ContactModel contactItem, int indexInCollection, string picturePath, bool existing)
      {
         InitializeComponent();

         Instance = this;
         _index = indexInCollection;

         if (image != null)
         {
            SaveType = ContactCodePath.Camera;
            _rasterImage = image.Clone();
         }
         else if (cameraStream != null)
         {
            SaveType = ContactCodePath.Gallery;
            _cameraStream = cameraStream;
         }
         else if (contactItem != null)
         {
            SaveType = ContactCodePath.Edit;

            //populate the fields that we already have 
            _picturePath = contactItem.Picture;
            _thumbnailPath = contactItem.Thumbnail;
            _backImagePath = contactItem.BackImage;

            ContactModified = false;
            ContactItem = contactItem.Clone();

            RetakeButton.IsEnabled = false;
            RetakeButton.IsVisible = false;
         }

         _existing = existing;

         Init();
      }

      private void Init()
      {
         try
         {
            _imageViewer = new ImageViewer();
            _imageViewer.ViewHorizontalAlignment = ControlAlignment.Center;
            _imageViewer.ViewVerticalAlignment = ControlAlignment.Center;
            _imageViewer.BackgroundColor = Color.Gray;
            _imageViewer.VerticalOptions = LayoutOptions.FillAndExpand;
            _imageViewer.HorizontalOptions = LayoutOptions.FillAndExpand;
            _imageViewer.Margin = new Thickness(0, 0);
            _imageViewer.AutoDisposeImages = false;

            Grid contentGrid = carouselView.ItemsSource.GetItem(0) as Grid;
            imageViewerContainer = contentGrid;
            imageViewerContainer.Children.Add(_imageViewer);

            _backImageViewer = new ImageViewer();
            _backImageViewer.ViewHorizontalAlignment = ControlAlignment.Center;
            _backImageViewer.ViewVerticalAlignment = ControlAlignment.Center;
            _backImageViewer.BackgroundColor = Color.Gray;
            _backImageViewer.VerticalOptions = LayoutOptions.FillAndExpand;
            _backImageViewer.HorizontalOptions = LayoutOptions.FillAndExpand;
            _backImageViewer.Margin = new Thickness(0, 0);
            _backImageViewer.AutoDisposeImages = false;

            Grid grid = carouselView.ItemsSource.GetItem(1) as Grid;
            backImageViewerContainer = grid.Children.GetItem(0) as Grid;
            backImageViewerContainer.Children.Add(_backImageViewer);

            addBackImageGrid = grid.Children[1] as Grid;

            setAsMainLabel = grid.Children[2] as Label;
            var setAsMainLabel_TapGexture = new TapGestureRecognizer();
            setAsMainLabel_TapGexture.Tapped += SetAsMainLabel_Tapped;
            setAsMainLabel.GestureRecognizers.Add(setAsMainLabel_TapGexture);

            carouselView.PositionSelected += CarouselView_PositionSelected;
            carouselView.GestureRecognizers.Add(new TapGestureRecognizer
            {
               Command = new Command(async () =>
               {
                  if (carouselView.Position == 0)
                  {
                     ImageViewerPage imageViewerPage = new ImageViewerPage(ContactItem, _rasterImage, ImageType.FrontImage);
                     imageViewerPage.ImageModified += ImageViewerPage_ImageModified;

                     await HomePage.Instance.Navigation.PushAsync(imageViewerPage);
                  }
                  else if (carouselView.Position == 1 && BackImage != null)
                  {
                     if (!setAsMainLabel.Bounds.Contains(carouselView.LastTapPosition))
                     {
                        ImageViewerPage imageViewerPage = new ImageViewerPage(ContactItem, BackImage, ImageType.BackImage);
                        imageViewerPage.ImageModified += ImageViewerPage_ImageModified;

                        await HomePage.Instance.Navigation.PushAsync(imageViewerPage);
                     }
                  }
               })
            });

            Image backsideImage = addBackImageGrid.Children[0] as Image;
            var backsideImage_TapGexture = new TapGestureRecognizer();
            backsideImage_TapGexture.Tapped += backsideImage_Tapped;
            backsideImage.GestureRecognizers.Add(backsideImage_TapGexture);

            autoSaveCheckbox.Checked = HomePage.CurrentAppData.AutoSaveToContacts;

            // Add the translucent overlay box that shows focus over the selected business card field (invisible by default)
            _overlayBox = new BoxView();
            _overlayBox.Color = Color.Red;
            _overlayBox.Opacity = 0.25;
            _overlayBox.IsVisible = false;
            _overlayBox.Margin = new Thickness(0);
            _overlayBox.WidthRequest = 0;
            _overlayBox.HeightRequest = 0;
            _imageViewer.Children.Add(_overlayBox);

            BackButtonPressed = new Action<object>((e) => { CheckDiscardChanges(); });

            _bcReader = HomePage.BcReader;

            _adsHiddenTimer = new System.Timers.Timer(HomePage.AD_HIDDEN_DURATION);
            _adsHiddenTimer.AutoReset = true;
            _adsHiddenTimer.Elapsed += (sender, e) =>
            {
               _adsHiddenTimer.Enabled = false;
               _adsHiddenTimer.Stop();
               _adsVisibleTimer = AdHelper.ShowAdvertisement(advertisementLayout);
               _adsVisibleTimer.Elapsed += (sender1, e1) =>
               {
                  _adsVisibleTimer.Enabled = false;
                  _adsVisibleTimer = null;
                  _adsHiddenTimer.Enabled = true;
                  _adsHiddenTimer.Start();
               };
            };

            if (_existing)
               BindingContext = new ContactDetailsViewModel(this);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);

            if (_bcReader == null && _numberOfFailedInitializations < 1)
            {
               _numberOfFailedInitializations++;
               Init();
            }
         }
      }

      protected override void OnAppearing()
      {
         base.OnAppearing();

         if (_bcReader != null && !_cardProcessed && !_processingCard)
         {
            ProcessCard(_cameraStream, _rasterImage, ContactItem, _picturePath, _existing);
         }
         else
         {
            if (_adsHiddenTimer != null && (_adsVisibleTimer == null || (_adsVisibleTimer != null && !_adsVisibleTimer.Enabled)))
            {
               _adsHiddenTimer.Enabled = false;
               _adsHiddenTimer.Stop();
               _adsVisibleTimer = AdHelper.ShowAdvertisement(advertisementLayout);
               _adsVisibleTimer.Elapsed += (sender1, e1) =>
               {
                  _adsVisibleTimer.Enabled = false;
                  _adsVisibleTimer = null;
                  _adsHiddenTimer.Enabled = true;
                  _adsHiddenTimer.Start();
               };
            }
         }
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         if (_adsHiddenTimer != null)
         {
            _adsHiddenTimer.Stop();
            _adsHiddenTimer.Enabled = false;
         }
      }

      public async void ProcessCard(Stream cameraStream, RasterImage image, ContactModel contactItem, string picturePath, bool existing)
      {
         _processingCard = true;
         Device.BeginInvokeOnMainThread(() =>
         {
            WaitUI();
            if (_adsHiddenTimer != null && (_adsVisibleTimer == null || (_adsVisibleTimer != null && !_adsVisibleTimer.Enabled)))
            {
               _adsHiddenTimer.Enabled = false;
               _adsHiddenTimer.Stop();
               _adsVisibleTimer = AdHelper.ShowAdvertisement(busyAdvertisementLayout);
               _adsVisibleTimer.Elapsed += (sender1, e1) =>
               {
                  _adsVisibleTimer.Enabled = false;
                  _adsVisibleTimer = null;
                  _adsHiddenTimer.Enabled = true;
                  _adsHiddenTimer.Start();
               };
            }
         });

         try
         {
            if (_rasterImage == null)
            {
               await Task.Run(async () =>
               {
                  if (!string.IsNullOrEmpty(picturePath))
                  {
                     try
                     {
                        _rasterImage = await RasterImageLoader.RasterImageFromFile(picturePath);

                        if (!string.IsNullOrEmpty(_backImagePath))
                           BackImage = await RasterImageLoader.RasterImageFromFile(_backImagePath);
                     }
                     catch (Exception ex)
                     {
                        Console.WriteLine(ex.Message);
                     }

                     if (existing) return;
                  }
                  else if (cameraStream != null)
                  {
                     try
                     {
                        _rasterImage = await RasterImageLoader.RasterImageFromStream(cameraStream);
                     }
                     catch (Exception ex)
                     {
                        Console.WriteLine(ex.Message);
                     }
                  }
               });
            }

            //skip this if editing existing contact
            if (!existing)
            {
               BCProcessStatus status = BCProcessStatus.Success;

               await Task.Run(() =>
               {
                  status = _bcReader.Process(_rasterImage);
               });

               if (status == BCProcessStatus.GlareDetected)
               {
                  await DisplayAlert("Error", "Glare detected in image.\nPlease retake image.", "OK");
               }
               else if (status == BCProcessStatus.BlurDetected)
               {
                  await DisplayAlert("Error", "Blur detected in image.\nPlease retake image.", "OK");
               }
               else if (status == BCProcessStatus.Failed)
               {
                  await DisplayAlert("Error", "Failed to recognize image.\nPlease retake image.", "OK");
               }

               if (_bcReader.Results != null && _bcReader.Results.Count > 0)
               {
                  ContactItem = GetResults(_bcReader.Results);
               }
               else
               {
                  ContactItem = new ContactModel();
               }

               this.BindingContext = new ContactDetailsViewModel(this);

               Task.Run(() =>
               {
                  _rasterThumbnail = _rasterImage.CreateThumbnail(300, 200, _rasterImage.BitsPerPixel, RasterViewPerspective.TopLeft, RasterSizeFlags.Resample);
               }).Wait();
            }
         }
         catch (Exception)
         {
            await DisplayAlert("Error", "Failed to recognize image.\nPlease retake image.", "OK");
         }
         finally
         {
            if (_bcReader != null && (_bcReader.Results == null || _bcReader.Results.Count == 0) && ContactItem == null)
            {
               ContactItem = new ContactModel();

               this.BindingContext = new ContactDetailsViewModel(this);

               if(_rasterThumbnail == null && _rasterImage != null)
               {
                  Task.Run(() =>
                  {
                     _rasterThumbnail = _rasterImage.CreateThumbnail(300, 200, _rasterImage.BitsPerPixel, RasterViewPerspective.TopLeft, RasterSizeFlags.Resample);
                  }).Wait();
               }
            }

            // Fit the image inside image viewer control
            Device.BeginInvokeOnMainThread(() =>
            {
               if(_rasterImage != null && _imageViewer != null)
               {
                  _imageViewer.Image = _rasterImage;
                  _imageViewer.Zoom(ControlSizeMode.FitAlways, 1.0, _imageViewer.DefaultZoomOrigin);
               }

               WaitUI();

               if(_adsHiddenTimer != null)
               {
                  _adsHiddenTimer.Enabled = false;
                  _adsHiddenTimer.Stop();
                  _adsVisibleTimer = AdHelper.ShowAdvertisement(advertisementLayout);
                  _adsVisibleTimer.Elapsed += (sender1, e1) =>
                  {
                     _adsVisibleTimer.Enabled = false;
                     _adsVisibleTimer = null;
                     _adsHiddenTimer.Enabled = true;
                     _adsHiddenTimer.Start();
                  };
               }
            });

            _cardProcessed = true;
            _processingCard = false;
         }
      }

      private void CleanUp()
      {
         this.BindingContext = null;

         DisposeImage(ref _rasterImage);
         DisposeImage(ref _backImage);
         DisposeImage(ref _rasterThumbnail);

         _imageViewer = null;
         _backImageViewer = null;
         if (carouselView != null)
         {
            carouselView.ItemsSource = null;
            carouselView = null;
         }

         if (imageViewerContainer != null)
         {
            imageViewerContainer.Children.Clear();
            imageViewerContainer = null;
         }

         if(backImageViewerContainer != null)
         {
            backImageViewerContainer.Children.Clear();
            backImageViewerContainer = null;
         }
      }

      public async void AddProfileImage(ContactModel contactItem, RasterImage image, Stream imageStream)
      {
         string filePath;

         if (string.IsNullOrEmpty(contactItem.Picture))
         {
            filePath = Path.Combine(HomePage.PROFILE_PICS_DIR, $"profile_{Guid.NewGuid()}.jpeg");
         }
         else
         {
            string imageFileName = Path.GetFileNameWithoutExtension(contactItem.Picture).Replace("image_", "");
            filePath = Path.Combine(HomePage.PROFILE_PICS_DIR, $"profile_{imageFileName}.jpeg");
         }

         if (imageStream != null)
         {
            await DependencyService.Get<IPictureSaver>().SaveImage(imageStream, filePath, PictureSaveResolution.Low);
         }
         else if (image != null)
         {
            LeadSize size = RasterImageHelper.GetImageSize(image.Width, image.Height, PictureSaveResolution.Low);
            ResizeInterpolateCommand resizeInterpolateCommand = new ResizeInterpolateCommand(size.Width, size.Height, ResizeInterpolateCommandType.Resample);
            resizeInterpolateCommand.Run(image);

            using (var codecs = new RasterCodecs())
            {
               codecs.Save(image, filePath, RasterImageFormat.Jpeg, 0);
            }
         }

         _tempPaths.Add(filePath);

         contactItem.ProfileImage = filePath;

         ContactModified = true;

         Device.BeginInvokeOnMainThread(() => this.BindingContext = new ContactDetailsViewModel(this));
      }

      public async void AddBackImage(ContactModel contactItem, RasterImage backImage, Stream imageStream)
      {
         string backImagePath;
         RasterImage tempBackImage = null;

         if (string.IsNullOrEmpty(contactItem.Picture))
         {
            backImagePath = Path.Combine(HomePage.APP_DIR, $"back_{Guid.NewGuid()}.jpeg");
         }
         else
         {
            string imageFileName = Path.GetFileNameWithoutExtension(contactItem.Picture).Replace("image_", "");
            backImagePath = Path.Combine(HomePage.APP_DIR, $"back_{imageFileName}.jpeg");
         }

         if (imageStream != null)
         {
            await DependencyService.Get<IPictureSaver>().SaveImage(imageStream, backImagePath, PictureSaveResolution.Medium);

            try
            {
               tempBackImage = await RasterImageLoader.RasterImageFromFile(backImagePath);
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
         }
         else if (backImage != null)
         {
            LeadSize size = RasterImageHelper.GetImageSize(backImage.Width, backImage.Height, PictureSaveResolution.Medium);
            ResizeInterpolateCommand resizeInterpolateCommand = new ResizeInterpolateCommand(size.Width, size.Height, ResizeInterpolateCommandType.Resample);
            resizeInterpolateCommand.Run(backImage);

            tempBackImage = backImage;

            using (var codecs = new RasterCodecs())
            {
               codecs.Save(backImage, backImagePath, RasterImageFormat.Jpeg, 0);
            }
         }

         _tempPaths.Add(backImagePath);

         contactItem.BackImage = backImagePath;

         _backImagePath = backImagePath;

         ContactModified = true;

         if(SaveType == ContactCodePath.Camera || SaveType == ContactCodePath.Gallery)
         {
            Task.Run(() => RecognizeBackImage(tempBackImage)).Wait();
         }
         else
         {
            BackImage = tempBackImage;
         }
      }

      private async void RecognizeBackImage(RasterImage rasterImage)
      {
         Device.BeginInvokeOnMainThread(() => WaitUI());

         try
         {
            BCProcessStatus status = BCProcessStatus.Success;

            await Task.Run(() =>
            {
               status = _bcReader.Process(rasterImage);
            });

            if (status == BCProcessStatus.GlareDetected)
            {
               Device.BeginInvokeOnMainThread(() => DisplayAlert("Error", "Glare detected in back image.", "OK"));
               return;
            }
            else if (status == BCProcessStatus.BlurDetected)
            {
               Device.BeginInvokeOnMainThread(() => DisplayAlert("Error", "Blur detected in back image.", "OK"));
               return;
            }
            else if (status == BCProcessStatus.Failed)
               return;

            if (_bcReader.Results != null)
            {
               FillBackImageResults(_bcReader.Results);
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
         finally
         {
            BackImage = rasterImage;
            Device.BeginInvokeOnMainThread(() =>
            {
               this.BindingContext = new ContactDetailsViewModel(this);
               WaitUI();
            });
         }
      }

      private void FillBackImageResults(List<KeyValuePair<string, BCResult>> results)
      {
         ContactModel contact = new ContactModel();

         bool barcodeResults = false;

         foreach (var result in _bcReader.Results)
         {
            if (result.Value.Value.Contains(MECardKeys.MECARD_KEY))
            {
               contact = MECardUtils.ParseMECardToContact(result.Value.Value, LeadRect.Empty);
               barcodeResults = true;
               break;
            }
            else if (result.Value.Value.Contains(VCardUtils.BEGIN_KEY) && result.Value.Value.Contains(VCardUtils.END_KEY))
            {
               contact = VCardUtils.VCardStringToContact(result.Value.Value, LeadRect.Empty);
               barcodeResults = true;
               break;
            }
         }

         if (barcodeResults)
         {
            if (!string.IsNullOrEmpty(contact.Name.Text))
               ContactItem.Name = contact.Name;

            if (contact.PhoneNumbers.Count > 0)
            {
               foreach (PhoneField barcodeField in contact.PhoneNumbers)
               {
                  for(int i = ContactItem.PhoneNumbers.Count - 1; i >= 0; --i)
                  {
                     if(ContactItem.PhoneNumbers[i].Type == barcodeField.Type)
                     {
                        ContactItem.PhoneNumbers.RemoveAt(i);
                     }
                  }

                  ContactItem.PhoneNumbers.Add(barcodeField);
               }
            }

            if (contact.Emails.Count > 0)
            {
               ContactItem.Emails.Clear();
               foreach (EmailField field in contact.Emails)
                  ContactItem.Emails.Add(field);
            }

            if (contact.Companies.Count > 0)
            {
               ContactItem.Companies.Clear();
               foreach (ContactField field in contact.Companies)
                  ContactItem.Companies.Add(field);
            }

            if (contact.Websites.Count > 0)
            {
               contact.Websites.Clear();
               foreach (ContactField field in contact.Websites)
                  ContactItem.Websites.Add(field);
            }

            if (contact.JobTitles.Count > 0)
            {
               contact.JobTitles.Clear();
               foreach (ContactField field in contact.JobTitles)
                  ContactItem.JobTitles.Add(field);
            }

            return;
         }

         foreach (var result in _bcReader.Results)
         {
            switch (result.Key)
            {
               case "Title":
                  ContactItem.JobTitles.Add(new ContactField(result.Value.Value, LeadRect.Empty));
                  break;
               case "Company":
                  ContactItem.Companies.Add(new ContactField(result.Value.Value, LeadRect.Empty));
                  break;
               case "Address":
                  ContactItem.Addresses.Add(new ContactField(result.Value.Value, LeadRect.Empty));
                  break;
               case "Website":
                  ContactItem.Websites.Add(new ContactField(result.Value.Value, LeadRect.Empty));
                  break;
               case "Office":
               case "Phone":
                  ContactItem.PhoneNumbers.Add(new PhoneField(result.Value.Value, LeadRect.Empty, PhoneType.Work));
                  break;
               case "Fax":
                  ContactItem.PhoneNumbers.Add(new PhoneField(result.Value.Value, LeadRect.Empty, PhoneType.WorkFax));
                  break;
               case "Mobile":
                  ContactItem.PhoneNumbers.Add(new PhoneField(result.Value.Value, LeadRect.Empty, PhoneType.WorkMobile));
                  break;
               case "Email":
                  ContactItem.Emails.Add(new EmailField(result.Value.Value, LeadRect.Empty, EmailType.Work));
                  break;
               case "QRCode":
                  ParseQRCode(ContactItem, result.Value);
                  break;
               default:
                  break;

            }
         }

         return;
      }

      private void SetAsMainLabel_Tapped(object sender, EventArgs e)
      {
         Device.BeginInvokeOnMainThread(() =>
         {
            RasterImage tempImage = _rasterImage;
            _rasterImage = BackImage;
            BackImage = tempImage;

            _imageViewer.Image = _rasterImage;
            _imageViewer.Zoom(ControlSizeMode.FitAlways, 1.0, _imageViewer.DefaultZoomOrigin);

            carouselView.Position = 0;

            ContactItem.FocusDisabled = true;
            ContactModified = true;

            if (SaveType == ContactCodePath.Camera || SaveType == ContactCodePath.Gallery)
            {
               if (File.Exists(ContactItem.BackImage))
                  File.Delete(ContactItem.BackImage);

               string backImagePath = Path.Combine(HomePage.APP_DIR, $"back_{Guid.NewGuid()}.jpeg");
               using (var codecs = new RasterCodecs())
               {
                  codecs.Save(BackImage, backImagePath, RasterImageFormat.Jpeg, 0);
               }

               _backImagePath = backImagePath;
               ContactItem.BackImage = _backImagePath;

            }
            else
            {
               _picturePath = ContactItem.BackImage;
               _backImagePath = ContactItem.Picture;

               ContactItem.Picture = _picturePath;
               ContactItem.BackImage = _backImagePath;
            }
         });
      }

      private async void backsideImage_Tapped(object sender, EventArgs e)
      {
         var results = await DependencyService.Get<Permissions.IPermissions>().VerifyPermissionsAsync(Permissions.Permission.Camera);
         if (results == null || results[Permissions.Permission.Camera] != Permissions.PermissionStatus.Granted)
            return;

         CameraPage cameraPage = new CameraPage(CameraOperationType.BackImage);
         cameraPage.PictureTaken += this.CameraPictureTaken;

         HomePage.Instance.PushCameraPage(cameraPage);
      }

      private void ImageViewerPage_ImageModified(object source, ImageModifiedEventArgs e)
      {
         if (e.ImageOperations == ImageOperations.Delete)
         {
            if (e.ImageType == ImageType.BackImage)
            {
               BackImage = null;

               ContactModified = true;
            }
            else if (e.ImageType == ImageType.ProfileImage)
            {
               this.BindingContext = new ContactDetailsViewModel(this);

               ContactModified = true;
            }
         }
         else
         {
            if (e.ImageType == ImageType.FrontImage)
            {

               Device.BeginInvokeOnMainThread(async () =>
               {
                  try
                  {
                     _rasterImage = await RasterImageLoader.RasterImageFromFile(ContactItem.Picture);
                  }
                  catch (Exception ex)
                  {
                     Console.WriteLine(ex.Message);
                  }

                  _imageViewer.Image = _rasterImage;
                  _imageViewer.Zoom(ControlSizeMode.FitAlways, 1.0, _imageViewer.DefaultZoomOrigin);

                  ContactItem.FocusDisabled = true;

                  this.BindingContext = new ContactDetailsViewModel(this);
               });
            }
            else if (e.ImageType == ImageType.BackImage)
            {
               Device.BeginInvokeOnMainThread(async () =>
               {
                  try
                  {
                     BackImage = await RasterImageLoader.RasterImageFromFile(ContactItem.BackImage);
                  }
                  catch (Exception ex)
                  {
                     Console.WriteLine(ex.Message);
                  }
               });
            }
            else
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  this.BindingContext = new ContactDetailsViewModel(this);
               });
            }

            ContactModified = true;
         }
      }

      protected override void OnSizeAllocated(double width, double height)
      {
#if __ANDROID__
         // A hack to show the CarouselView 
         if(carouselView != null && carouselView.Bounds.Width == -1)
         {
            carouselView.Layout(new Rectangle(0, 0, width, height * 0.25));
         }
#endif // #if __ANDROID__

         base.OnSizeAllocated(width, height);
      }

      protected override bool OnBackButtonPressed()
      {
         CheckDiscardChanges();

         return true;
      }

      private async void OnPageClosing(bool navigateToHomePage = false)
      {
         PageClosing?.Invoke(this, null);

         if(navigateToHomePage)
            HomePage.Instance.NavigateToHomePage();
         else
            await HomePage.Instance.Navigation.PopAsync();
      }

      public void CameraPictureTaken(object sender, PictureTakenEventArgs e)
      {
         if (e.OperationType == CameraOperationType.Profile)
         {
            AddProfileImage(ContactItem, e.Image, e.Stream);
         }
         else
         {
            AddBackImage(ContactItem, e.Image, e.Stream);
         }
      }

      private void ParseQRCode(ContactModel contact, BCResult result)
      {
         Regex websiteRegex = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$", RegexOptions.IgnoreCase);
         Regex emailRegex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
         Regex mobileNumber = new Regex(@"\+?\d+");

         if (websiteRegex.IsMatch(result.Value))
         {
            contact.Websites.Add(new ContactField(result.Value, result.Bounds));
         }
         else if (emailRegex.IsMatch(result.Value))
         {
            string emailStr = result.Value.ToLower().Replace("mailto:", "");
            contact.Emails.Add(new EmailField(emailStr, result.Bounds, EmailType.Work));
         }
         else if (mobileNumber.IsMatch(result.Value))
         {
            contact.PhoneNumbers.Add(new PhoneField(mobileNumber.Match(result.Value).Value, result.Bounds, PhoneType.WorkMobile));
         }
         else
         {
            contact.QRCode = new ContactField(result.Value, result.Bounds);
         }
      }

      private ContactModel GetResults(List<KeyValuePair<string, BCResult>> results)
      {
         ContactModel contact = new ContactModel();

         foreach (var result in _bcReader.Results)
         {
            if (result.Key == "QRCode")
            {
               if (result.Value.Value.Contains(MECardKeys.MECARD_KEY))
               {
                  contact = MECardUtils.ParseMECardToContact(result.Value.Value, result.Value.Bounds);
                  break;
               }
               else if (result.Value.Value.Contains(VCardUtils.BEGIN_KEY) && result.Value.Value.Contains(VCardUtils.END_KEY))
               {
                  contact = VCardUtils.VCardStringToContact(result.Value.Value, result.Value.Bounds);
                  break;
               }
            }

            switch (result.Key)
            {
               case "Name":
                  contact.Name = new ContactField(result.Value.Value, result.Value.Bounds);
                  break;
               case "Title":
                  contact.JobTitles.Add(new ContactField(result.Value.Value, result.Value.Bounds));
                  break;
               case "Company":
                  contact.Companies.Add(new ContactField(result.Value.Value, result.Value.Bounds));
                  break;
               case "Address":
                  contact.Addresses.Add(new ContactField(result.Value.Value, result.Value.Bounds));
                  break;
               case "Website":
                  contact.Websites.Add(new ContactField(result.Value.Value, result.Value.Bounds));
                  break;
               case "Office":
               case "Phone":
                  contact.PhoneNumbers.Add(new PhoneField(result.Value.Value, result.Value.Bounds, PhoneType.Work));
                  break;
               case "Fax":
                  contact.PhoneNumbers.Add(new PhoneField(result.Value.Value, result.Value.Bounds, PhoneType.WorkFax));
                  break;
               case "Mobile":
                  contact.PhoneNumbers.Add(new PhoneField(result.Value.Value, result.Value.Bounds, PhoneType.WorkMobile));
                  break;
               case "Email":
                  contact.Emails.Add(new EmailField(result.Value.Value, result.Value.Bounds, EmailType.Work));
                  break;
               case "QRCode":
                  ParseQRCode(contact, result.Value);
                  break;
               default:
                  break;

            }
         }

         if (string.IsNullOrEmpty(contact.Name.Text))
            contact.Name = new ContactField(" ", LeadRect.Empty);

         contact.Date = DateTime.Now;

         ContactModified = true;

         return contact;
      }

      private void DisposeImage(ref RasterImage image)
      {
         if (image != null && !image.IsDisposed)
         {
            image.Dispose();
            image = null;
         }
      }

      private void RetakeButton_Tapped(object sender, EventArgs e)
      {
         Grid senderControl = sender as Grid;
         (senderControl.GestureRecognizers[0] as TapGestureRecognizer).Tapped -= RetakeButton_Tapped;

         CleanUp();

         OnPageClosing();
         HomePage.Instance.RetakeBusinessCard();
      }

      private async void SaveButton_Tapped(object sender, EventArgs e)
      {
         if (HomePage.CurrentAppData.AutoSaveToContacts)
         {
            var results = await DependencyService.Get<Permissions.IPermissions>().VerifyPermissionsAsync(Permissions.Permission.Contacts);
            if (results == null || results[Permissions.Permission.Contacts] != Permissions.PermissionStatus.Granted)
               return;
         }

         Grid senderControl = sender as Grid;
         (senderControl.GestureRecognizers[0] as TapGestureRecognizer).Tapped -= SaveButton_Tapped;

         ContactModel ContactToAdd = await CreateNewContactItemAsync();

         //check if auto save is on
         if (HomePage.CurrentAppData.AutoSaveToContacts)
         {
            DependencyService.Get<ISaveContact>().SaveContact(ContactToAdd);
         }

         if (SaveType == ContactCodePath.Camera || SaveType == ContactCodePath.Gallery)
         {
            HomePage.ContactCollection.Add(ContactToAdd);
         }
         else
         {
            HomePage.ContactCollection.Insert(_index, ContactToAdd);
            HomePage.ContactCollection.RemoveAt(_index + 1);
         }

         ContactSavedEventArgs args = new ContactSavedEventArgs()
         {
            Contact = ContactToAdd
         };

         OnContactSaved(args);

         CleanUp();
         OnPageClosing();
      }

      public async Task<ContactModel> CreateNewContactItemAsync()
      {
         //before we create the contact save the images to the app dir
         Device.BeginInvokeOnMainThread(() => WaitUI("Saving..."));
         await Task.Factory.StartNew(() =>
         {
            _rasterThumbnail = _rasterImage.CreateThumbnail(300, 200, _rasterImage.BitsPerPixel, RasterViewPerspective.TopLeft, RasterSizeFlags.Resample);
            using (var codecs = new RasterCodecs())
            {
               if(string.IsNullOrWhiteSpace(_thumbnailPath))
                  _thumbnailPath = Path.Combine(HomePage.THUMBS_DIR, $"thumb_{Guid.NewGuid()}.jpeg");
               codecs.Save(_rasterThumbnail, _thumbnailPath, RasterImageFormat.Jpeg, 0);

               if (SaveType == ContactCodePath.Camera || SaveType == ContactCodePath.Gallery)
               {
                  _picturePath = Path.Combine(HomePage.APP_DIR, $"image_{Guid.NewGuid()}.jpeg");

                  if (!string.IsNullOrEmpty(_backImagePath))
                  {
                     string backImageGuid = Path.GetFileNameWithoutExtension(_backImagePath).Replace("back_", "");
                     _picturePath = Path.Combine(HomePage.APP_DIR, $"image_{backImageGuid}.jpeg");
                  }

                  codecs.Save(_rasterImage, _picturePath, RasterImageFormat.Jpeg, 0);
                  if (_backImage != null)
                     codecs.Save(_backImage, _backImagePath, RasterImageFormat.Jpeg, 0);
               }
            }
         });

         Device.BeginInvokeOnMainThread(() => WaitUI("Saving..."));
         ContactItem.Thumbnail = _thumbnailPath;
         ContactItem.Picture = _picturePath;
         ContactItem.ImageWidth = _rasterImage != null ? _rasterImage.ImageWidth : ContactItem.ImageWidth;
         ContactItem.ImageHeight = _rasterImage != null ? _rasterImage.ImageHeight : ContactItem.ImageHeight;

         return ContactItem;
      }

      private void WaitUI(string text = "Processing")
      {
         if (_wait)
         {
            contentLayout.Opacity = 0.2;
            contentLayout.IsEnabled = false;
            activityIndicatorText.Text = text;
            activityIndicatorText.IsVisible = true;
            waitingOverlayView.IsVisible = true;
            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;
            _wait = !_wait;
         }
         else
         {
            waitingOverlayView.IsVisible = false;
            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
            activityIndicatorText.IsVisible = false;
            contentLayout.Opacity = 1;
            contentLayout.IsEnabled = true;
            _wait = !_wait;
         }
      }

      private void BackButton_Tapped(object sender, EventArgs e)
      {
         CheckDiscardChanges();
      }

      private void CheckDiscardChanges()
      {
         if (ContactModified)
         {
            Device.BeginInvokeOnMainThread(async () =>
            {
               bool accept = await this.DisplayAlert("Discard Changes", "Changes will be lost. Continue?", "Yes", "No");
               if (accept)
               {
                  if (_tempPaths.Count > 0)
                  {
                     foreach (string path in _tempPaths)
                     {
                        if (File.Exists(path))
                           File.Delete(path);
                     }
                  }

                  CleanUp();
                  OnPageClosing();
               }
            });
         }
         else
         {
            CleanUp();
            OnPageClosing();
         }
      }

      private void CheckboxImage_CheckedChanged(object sender, Utils.CheckedChangedEventArgs e)
      {
         HomePage.CurrentAppData.AutoSaveToContacts = e.IsChecked;
      }

      private void DeleteButton_Tapped(object sender, EventArgs e)
      {
         Device.BeginInvokeOnMainThread(async () =>
         {
            bool accept = await this.DisplayAlert("Delete Business Card", "Delete this business card?", "Yes", "No");
            if (accept)
            {
               // Clean up
               if (_tempPaths.Count > 0)
               {
                  foreach (string path in _tempPaths)
                  {
                     if (File.Exists(path))
                        File.Delete(path);
                  }
               }

               ContactModel contactItem = this.ContactItem;

               if (_index != -1)
               {
                  contactItem = HomePage.ContactCollection[_index];

                  HomePage.ContactCollection.RemoveAt(_index);
                  HomePage.ContactsGrouped = Task.Run(() => HomePage.Instance.OrganizeContacts()).Result;
                  HomePage.Instance.MainContactsList.ItemsSource = HomePage.ContactsGrouped;
                  HomePage.Instance.SaveContactList();
               }

               Actions.DeleteAllBusinessCardFiles(contactItem);

               OnPageClosing(true);
            }
         });
      }

      private void CarouselView_PositionSelected(object sender, PositionSelectedEventArgs e)
      {
#if __IOS__
         if(_rasterImage != null)
         {
            _imageViewer.Zoom(ControlSizeMode.Fit, 1.0, _imageViewer.DefaultZoomOrigin);
         }
#endif // #if __IOS__
      }
   }

   public class MyCarouselViewControl : CarouselViewControl
   {
      public MyCarouselViewControl() : base()
      {
      }

      public Point LastTapPosition
      {
         get;
         set;
      }
   }
}
