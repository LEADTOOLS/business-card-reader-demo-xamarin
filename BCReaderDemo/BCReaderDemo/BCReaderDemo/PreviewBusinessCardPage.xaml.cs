// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using CarouselView.FormsPlugin.Abstractions;
using DataService;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.Demos;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using Leadtools.ImageProcessing.Core;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace BCReaderDemo
{
   public class FadeTriggerAction : TriggerAction<VisualElement>
   {
      public FadeTriggerAction() { }

      public int StartsFrom { set; get; }

      protected override void Invoke(VisualElement visual)
      {
         visual.Animate("fadeTo", new Animation((d) =>
         {
            var val = StartsFrom == 1 ? d : 1 - d;
            visual.Opacity = val;
         }),
         16,
         500,
         Easing.Linear);
      }
   }

   class ContactFieldItem : BindableObject
   {

      public static readonly BindableProperty ShowQuickActionsProperty = BindableProperty.CreateAttached(
                    "ShowQuickActions",
                    typeof(bool),
                    typeof(ContactFieldItem),
                    false);

      public string Key { get; set; }
      public string Value { get; set; }
      public bool ShowActionsButton { get; set; } = false;
      public bool ShowQuickActions
      {
         get
         {
            return (bool)GetValue(ShowQuickActionsProperty);
         }
         set
         {
            SetValue(ShowQuickActionsProperty, value);
         }
      }

      public ContactFieldItem() { }

      public ContactFieldItem(string key, string value)
      {
         Key = key;
         Value = value;
      }
   }

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class PreviewBusinessCardPage : PopupPage
   {
      private int _contactIndex;
      private ObservableCollection<ContactFieldItem> ContactFields = new ObservableCollection<ContactFieldItem>();

      public static readonly BindableProperty ContactItemProperty =
         BindableProperty.CreateAttached("ContactItem",
            typeof(ContactModel),
            typeof(PreviewBusinessCardPage),
            new ContactModel());

      public ContactModel ContactItem
      {
         get { return (ContactModel)GetValue(ContactItemProperty); }
         set { SetValue(ContactItemProperty, value); }
      }

      public event EventHandler PageClosing;

      public PreviewBusinessCardPage(ContactModel contactItem, int indexInCollection)
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         BindingContext = this;

         ContactItem = contactItem;
         _contactIndex = indexInCollection;

         if (profileImageView == null)
         {
            Grid grid = (Grid)carouselView.ItemsSource.GetItem(0);

            baseFrontImageView = grid.Children[0] as Image;

            grid = (Grid)grid.Children.Where((a) => a is Grid).GetItem(0);
            profileImageView = (SvgImage)grid.Children[0];
         }

         RoundCornersEffect.SetCornerRadius(profileImageView, (int)(profileImageView.HeightRequest / 2));

         Grid frontImageGrid = (carouselView.ItemsSource.GetItem(1) as Grid);

         frontImageView = frontImageGrid.Children[0] as Image;

         backImageContainer = (carouselView.ItemsSource.GetItem(2) as Grid);

         Grid backImageViewGrid = backImageContainer.Children[0] as Grid;

         backImageView = backImageViewGrid.Children[0] as Image;

         addBackImageContainer = backImageContainer.Children[1] as Grid;

         frontImageView.GestureRecognizers.Add(new TapGestureRecognizer
         {
            Command = new Command(async () =>
            {
               HideQuickActionControls();
               ImageViewerPage imageViewerPage = new ImageViewerPage(ContactItem, ContactItem.Picture, ImageType.FrontImage);

               imageViewerPage.ImageModified += ImageViewerPage_ImageModified;

               await PopupNavigation.Instance.PushAsync(imageViewerPage);
            })
         });

         backImageView.GestureRecognizers.Add(new TapGestureRecognizer
         {
            Command = new Command(async () =>
            {
               HideQuickActionControls();
               ImageViewerPage imageViewerPage = new ImageViewerPage(ContactItem, ContactItem.BackImage, ImageType.BackImage);

               imageViewerPage.ImageModified += ImageViewerPage_ImageModified;

               await PopupNavigation.Instance.PushAsync(imageViewerPage);
            })
         });

         profileImageView.GestureRecognizers.Add(new TapGestureRecognizer
         {
            Command = new Command(async () =>
            {
               if (String.IsNullOrEmpty(ContactItem.ProfileImage))
               {
                  var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
                  if (results == null || results[PermissionType.Camera] != PermissionStatus.Granted)
                     return;

                  CameraPage cameraPage = new CameraPage(CameraOperationType.Profile);
                  cameraPage.PictureTaken += this.CameraPictureTaken;

                  HomePage.Instance.PushCameraPage(cameraPage);
               }
               else
               {
                  HideQuickActionControls();
                  ImageViewerPage imageViewerPage = new ImageViewerPage(ContactItem, ContactItem.ProfileImage, ImageType.ProfileImage);

                  imageViewerPage.ImageModified += ImageViewerPage_ImageModified;

                  await PopupNavigation.Instance.PushAsync(imageViewerPage);
               }

            })
         });

         Image backsideImage = addBackImageContainer.Children[0] as Image;
         backsideImage.GestureRecognizers.Add(new TapGestureRecognizer
         {
            Command = new Command(async () =>
            {
               var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
               if (results == null || results[PermissionType.Camera] != PermissionStatus.Granted)
                  return;

               CameraPage cameraPage = new CameraPage(CameraOperationType.BackImage);
               cameraPage.PictureTaken += this.CameraPictureTaken;

               HomePage.Instance.PushCameraPage(cameraPage);
            })
         });

         if (!String.IsNullOrEmpty(ContactItem.BackImage))
         {
            SetBackImage();
         }

         if(!String.IsNullOrEmpty(ContactItem.ProfileImage))
         {
            profileImageView.Source = ContactItem.ProfileImage;
         }

         ContactModelToFields();
         ContactListView.ItemsSource = ContactFields;
      }

      protected override async void OnAppearing()
      {
         base.OnAppearing();

         // Delay a bit, so the ad doesn't appear immediately
         await Task.Delay(1000);

         // Start the ads
         Ads.Start();
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         // Stop the ads
         Ads.Stop();
      }

      private void ImageViewerPage_ImageModified(object source, ImageModifiedEventArgs e)
      {
         ContactItem = e.ContactItem;

         if (e.ImageOperations == ImageOperations.Replace)
         {
            if (e.ImageType == ImageType.FrontImage)
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  frontImageView.Source = null;
                  frontImageView.Source = ContactItem.Picture;

                  baseFrontImageView.Source = null;
                  baseFrontImageView.Source = ContactItem.Picture;
               });
            }
            else if (e.ImageType == ImageType.BackImage)
            {
               SetBackImage();
            }
            else
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  profileImageView.Source = null;
                  profileImageView.Source = ContactItem.ProfileImage;
               });
            }
         }
         else
         {
            if (e.ImageType == ImageType.BackImage)
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  addBackImageContainer.IsEnabled = true;
                  addBackImageContainer.IsVisible = true;
               });
            }
            else
            {
               profileImageView.ResourceName = "Icons/avatar.svg";
            }
         }

         HomePage.Instance.DetailsPage_ContactSaved(this, new ContactSavedEventArgs(ContactItem));
      }

      private void ContactModelToFields()
      {
         ContactFields.Clear();
         ContactFields.Add(new ContactFieldItem("NAME", ContactItem.Name.Text));

         foreach (PhoneField phoneNumber in ContactItem.PhoneNumbers)
         {
            if (phoneNumber.Type == PhoneType.HomeMobile || phoneNumber.Type == PhoneType.WorkMobile)
            {
               ContactFields.Add(new ContactFieldItem
               {
                  Key = phoneNumber.Type == PhoneType.HomeMobile ? "HOME MOBILE" : "WORK MOBILE",
                  Value = phoneNumber.Number,
                  ShowActionsButton = true,
               });
            }
            else if (phoneNumber.Type == PhoneType.HomeFax || phoneNumber.Type == PhoneType.WorkFax)
            {
               ContactFields.Add(new ContactFieldItem(phoneNumber.Type == PhoneType.HomeFax ? "HOME FAX" : "WORK FAX", phoneNumber.Number));
            }
            else if (phoneNumber.Type == PhoneType.Home)
            {
               ContactFields.Add(new ContactFieldItem("HOME NUMBER", phoneNumber.Number));
            }
            else
            {
               ContactFields.Add(new ContactFieldItem("PHONE NUMBER", phoneNumber.Number));
            }
         }

         foreach (EmailField email in ContactItem.Emails)
         {
            ContactFields.Add(new ContactFieldItem
            {
               Key = email.Type == EmailType.Personal ? "PERSONAL EMAIL" : "WORK EMAIL",
               Value = email.Email,
            });
         }

         if (!String.IsNullOrEmpty(ContactItem.Company.Text))
         {
            ContactFields.Add(new ContactFieldItem("COMPANY", ContactItem.Company.Text));
         }

         foreach (ContactField website in ContactItem.Websites)
         {
            ContactFields.Add(new ContactFieldItem("WEBSITE", website.Text));
         }

         if (ContactItem.Addresses != null && ContactItem.Addresses.Count > 0)
            ContactFields.Add(new ContactFieldItem("ADDRESS", ContactItem.Addresses[0].Text));

         if (!String.IsNullOrEmpty(ContactItem.Group))
         {
            ContactFields.Add(new ContactFieldItem("GROUP", ContactItem.Group));
         }

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

         if (e.Image != null && !e.Image.IsDisposed)
            e.Image.Dispose();
      }

      private void PhoneCallQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         Image image = (Image)sender;
         ContactFieldItem item = image.BindingContext as ContactFieldItem;

         if (item != null)
         {
            Actions.DialPhone(item.Value, this);
         }
      }

      private void MessageQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         Image image = (Image)sender;
         ContactFieldItem item = image.BindingContext as ContactFieldItem;

         if (item != null)
         {
            Actions.ComposeSms(item.Value, string.Empty, this);
         }
      }

      private bool HideQuickActionControls()
      {
         bool ret = false;
         IEnumerable<ContactFieldItem> items = ContactFields.Where(x => x.ShowQuickActions);

         if (items != null && items.Count() > 0)
         {
            foreach (ContactFieldItem contact in items)
            {
               contact.ShowQuickActions = false;
            }
            ret = true;
         }

         return ret;
      }

      public async void AddProfileImage(ContactModel contactItem, RasterImage image, Stream imageStream)
      {
         string imageFileName = Path.GetFileNameWithoutExtension(contactItem.Picture).Replace("image_", "");
         string filePath = Path.Combine(HomePage.PROFILE_PICS_DIR, $"profile_{imageFileName}.jpeg");

         if (imageStream != null)
         {
            await DependencyService.Get<IPictureSaver>().SaveImage(imageStream, filePath, PictureSaveResolution.Low, false);
         }
         else if (image != null)
         {
            LeadSize size = ImageSizeHelper.GetImageSize(image.Width, image.Height, PictureSaveResolution.Low);
            ResizeInterpolateCommand resizeInterpolateCommand = new ResizeInterpolateCommand(size.Width, size.Height, ResizeInterpolateCommandType.Resample);
            resizeInterpolateCommand.Run(image);

            using (var codecs = new RasterCodecs())
            {
               codecs.Save(image, filePath, RasterImageFormat.Jpeg, 0);
            }
         }

         contactItem.ProfileImage = filePath;

         Device.BeginInvokeOnMainThread(() => profileImageView.Source = filePath);

         HomePage.Instance.SaveContactList();
      }

      public async void AddBackImage(ContactModel contactItem, RasterImage backImage, Stream imageStream)
      {
         string imageFileName = Path.GetFileNameWithoutExtension(contactItem.Picture).Replace("image_", "");
         string backImagePath = Path.Combine(HomePage.APP_DIR, $"back_{imageFileName}.jpeg");

         if (imageStream != null)
         {
            await DependencyService.Get<IPictureSaver>().SaveImage(imageStream, backImagePath, PictureSaveResolution.Medium, false);
         }
         else if (backImage != null)
         {
            LeadSize size = ImageSizeHelper.GetImageSize(backImage.Width, backImage.Height, PictureSaveResolution.Medium);
            ResizeInterpolateCommand resizeInterpolateCommand = new ResizeInterpolateCommand(size.Width, size.Height, ResizeInterpolateCommandType.Resample);
            resizeInterpolateCommand.Run(backImage);

            using (var codecs = new RasterCodecs())
            {
               codecs.Save(backImage, backImagePath, RasterImageFormat.Jpeg, 0);
            }
         }

         contactItem.BackImage = backImagePath;

         HomePage.Instance.SaveContactList();

         SetBackImage();
      }

      private void SetBackImage()
      {
         Device.BeginInvokeOnMainThread(() =>
         {
            addBackImageContainer.IsVisible = false;
            backImageView.Source = ContactItem.BackImage;
         });
      }

      private void PhoneQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContentView senderContent = (ContentView)sender;

         if ((senderContent.BindingContext as ContactFieldItem).ShowQuickActions)
            return;

         if ((senderContent.BindingContext as ContactFieldItem).ShowActionsButton == false)
            return;

         // Slide from right to left effect for the quick actions grid
         Grid parentGrid = senderContent.Parent as Grid;

         if (parentGrid != null)
         {
            Grid itemGrid = parentGrid.Children.OfType<Grid>().FirstOrDefault();

            (senderContent.BindingContext as ContactFieldItem).ShowQuickActions = true;

            if (itemGrid != null && itemGrid.Children.Count > 1)
            {
               Grid actionsGrid = itemGrid.Children[1] as Grid;

               if (actionsGrid != null)
               {
                  actionsGrid.TranslationX = DemoUtilities.DisplayWidth;
                  actionsGrid.TranslateTo(-1, 0);
               }
            }
         }
      }

      private async void ContactListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         HideQuickActionControls();
         ContactFieldItem item = e.Item as ContactFieldItem;
         if (item != null)
         {
            if (item.Key.Contains("MOBILE"))
               return;

            if (item.Key.Contains("EMAIL"))
               Actions.ComposeEmail(item.Value, string.Empty, string.Empty, this);
            else if (item.Key.Contains("WEBSITE"))
               Actions.VisitWebsite(item.Value, this);
            else if (item.Key.Contains("NUMBER"))
               Actions.DialPhone(item.Value, this);
            else if (item.Key.Contains("ADDRESS"))
            {
               // Apple store asks to have the location permissions asked for even for firing the maps app, but play store doesn't, so just do it for iOS.
               if(Device.RuntimePlatform == Device.iOS)
               {
                  var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Location);
                  if (results == null || results[PermissionType.Location] != PermissionStatus.Granted)
                     return;
               }

               Actions.LocateAddressOnMap(item.Value, this);
            }
         }
      }

      private void ContactListView_Clicked(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }

      private void ShareButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         Actions.ShareContact(ContactItem);
      }

      private async void EditButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         UpdateBusinessCardPage page = new UpdateBusinessCardPage(null, null, ContactItem, _contactIndex, ContactItem.Picture, true);
         page.PageClosing += UpdateBusinessCardPage_PageClosing;
         page.ContactSaved += UpdateBusinessCardPage_ContactSaved;

         await PopupNavigation.Instance.PushAsync(page);
      }

      private void UpdateBusinessCardPage_PageClosing(object sender, EventArgs e)
      {
         // Check if this card was deleted inside the UpdateBusinessCardPage, if yes then send PageClosing event to the subscribers.
         if(!HomePage.ContactCollection.Contains(ContactItem))
            OnPageClosing();
      }

      private void UpdateBusinessCardPage_ContactSaved(object sender, ContactSavedEventArgs e)
      {
         HomePage.Instance.DetailsPage_ContactSaved(sender, e);

         ContactItem = HomePage.ContactCollection[_contactIndex];

         ContactListView.ItemsSource = null;
         ContactModelToFields();
         ContactListView.ItemsSource = ContactFields;

         if (!string.IsNullOrEmpty(ContactItem.BackImage))
         {
            SetBackImage();
         }
         else
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               addBackImageContainer.IsEnabled = true;
               addBackImageContainer.IsVisible = true;
            });
         }

         if (!string.IsNullOrEmpty(ContactItem.ProfileImage))
         {
            Device.BeginInvokeOnMainThread(() => profileImageView.Source = ContactItem.ProfileImage);
         }
         else
         {
            Device.BeginInvokeOnMainThread(() => profileImageView.ResourceName = "Icons/avatar.svg");
         }
      }

      private async void ContactButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Contacts);
         if (results == null || results[PermissionType.Contacts] != PermissionStatus.Granted)
            return;

         DependencyService.Get<ISaveContact>().SaveContact(ContactItem);

         Device.BeginInvokeOnMainThread(() => HomePage.ShowMessage(notificationMessageView, "Contact saved to your phone successfully"));
      }

      private void BackButton_Tapped(object sender, EventArgs e)
      {
         OnPageClosing();
      }

      protected override bool OnBackButtonPressed()
      {
         OnPageClosing();
         return true;
      }

      private async void OnPageClosing()
      {
         PageClosing?.Invoke(this, null);

         carouselView.ItemsSource = null;
         backImageContainer.Children.Clear();
         addBackImageContainer.Children.Clear();
         this.BindingContext = null;

         // Check if this card was deleted inside the UpdateBusinessCardPage, then we already popped this page out.
         if (HomePage.ContactCollection.Contains(ContactItem))
            await PopupNavigation.Instance.PopAsync();
      }

      private void MainGrid_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }
   }
}
