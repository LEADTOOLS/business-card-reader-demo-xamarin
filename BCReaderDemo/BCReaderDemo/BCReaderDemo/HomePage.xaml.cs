// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
#if __ANDROID__
using Android.Content.Res;
#endif
#if __IOS__
using Foundation;
#endif
using Leadtools.Forms.Commands;
using Leadtools.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using BCReaderDemo.Models;
using System.Xml.Serialization;
using BCReaderDemo.Utils;
using Xamarin.Forms.Xaml;
using Leadtools.Barcode;
using Rg.Plugins.Popup.Services;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using Leadtools.Demos;
using Rg.Plugins.Popup.Pages;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class HomePage : ContentPage
   {
      public static string APP_DIR;
      public static string DROID_RUNTIME;
      public static string FONTS_DIR;
      public static string THUMBS_DIR;
      public static string PROFILE_PICS_DIR;
      public IOcrEngine _ocrEngine;
      public static bool IsLoading = false;
      public static string FacebookUrl = "https://www.facebook.com/LEADTOOLS";
      public static string LinkedInUrl = "https://www.linkedin.com/company/2538408";
      public static string TwitterUrl = "https://twitter.com/LEADTOOLS";
      public static string YoutubeUrl = "https://youtube.com/user/leadtools";
      public static string _appUrlOnAppStore = "https://www.leadtools.com/apps/bcr";
      public static ObservableCollection<ContactModel> ContactCollection { get; set; }
      public static ObservableCollection<Grouping<string, ContactModel>> ContactsGrouped { get; set; }
      public static BusinessCardReader BcReader;

      public ListView MainContactsList
      {
         get { return ContactsList; }
      }

      public SearchBar MainSearchBar
      {
         get { return mainSearchBar; }
      }

      public static AppData CurrentAppData;

      public static HomePage Instance { get; private set; }

      public Action<ContactModel> ContactSavedAction { get; set; }

      public HomePage()
      {
         InitializeComponent();
         Init();
         Instance = this;
      }

      private void Init()
      {
         // Add tap recognizer to Actions button
         var actionsButton_TapGexture = new TapGestureRecognizer();
         actionsButton_TapGexture.Tapped += ActionsButton_Tapped;
         ActionsButton.GestureRecognizers.Add(actionsButton_TapGexture);

         // Add tap recognizer to Camera button
         var cameraButton_TapGexture = new TapGestureRecognizer();
         cameraButton_TapGexture.Tapped += CameraButton_Tapped;
         CameraButton.GestureRecognizers.Add(cameraButton_TapGexture);

         // Add tap recognizer to Settings button
         TapGestureRecognizer settingsButton_TapGesture = new TapGestureRecognizer();
         settingsButton_TapGesture.Tapped += SettingsButton_Tapped;
         SettingsButton.GestureRecognizers.Add(settingsButton_TapGesture);

         BindingContext = this;

         try
         {
            Leadtools.Platform.RuntimePlatform = Device.RuntimePlatform;
            Leadtools.Core.Assembly.Use();
            Leadtools.Svg.Assembly.Use();
            Leadtools.Controls.Assembly.Use();
            Leadtools.Camera.Xamarin.Assembly.Use();
            Leadtools.Controls.Assembly.Use();
            Leadtools.Ocr.LEADEngine.Assembly.Use();
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      public void Initialize()
      {
         try
         {
            IsLoading = true;

            // Set license
            if (!LicenseManagerUtility.SetLicense(this))
               return;

            GetAppDirectory();

            BcReader = new BusinessCardReader(null, new BarcodeEngine());

            Task.Run(() =>
            {
               CopyDependencies(APP_DIR);
               StartOcrEngine();
            });

            CurrentAppData = new AppData();
            CurrentAppData.Load(Path.Combine(APP_DIR, AppData.DATA_FILE_NAME));
            CurrentAppData.PropertyChanged += CurrentAppData_PropertyChanged;

            //init collection and bind the list source to it
            LoadContactList();

            RefreshListView();
            DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
         }
         finally
         {
            IsLoading = false;
         }
      }

      public async void OnStart()
      {
         Initialize();

         // Initialize ads
         await AdsView.Init();

         // Delay so the ad doesn't appear immediately
         await Task.Delay(1000);

         // Start ads
         Ads.Start();
      }

      public void OnSleep()
      {
         // Stop ads
         Ads.Stop();
      }

      public void OnResume()
      {
         // Start ads again
         Ads.Start();
      }

      private bool doubleBackToExitPressedTwice = false;
      protected override bool OnBackButtonPressed()
      {
         if (doubleBackToExitPressedTwice) return false; // QUIT

         ShowMessage(notificationMessageView, "Tap Back again to exit");
         doubleBackToExitPressedTwice = true;

         Device.StartTimer(TimeSpan.FromSeconds(2), () =>
         {
            doubleBackToExitPressedTwice = false; // reset those 2 seconds
            return false;
         });

         return true; // true - Don't process BACK by system
      }

      public async void RefreshListView()
      {
         if (string.IsNullOrWhiteSpace(mainSearchBar.Text))
            ContactsGrouped = await Task.Factory.StartNew(() => OrganizeContacts());
         else
            ContactsGrouped = await Task.Factory.StartNew(() => SearchContacts(mainSearchBar.Text));

         Device.BeginInvokeOnMainThread(() =>
         {
            ContactsList.ItemsSource = ContactsGrouped;
            UpdatePageTitle();
         });
      }

      private void UpdatePageTitle()
      {
         string cardsCount = string.Format("({0})", ContactCollection.Count);
         savedCardsCount.Text = cardsCount;
      }

      private async void GroupButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         GroupsPage page = new GroupsPage();
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void CurrentAppData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         if(e.PropertyName == nameof(AppData.SortBy))
         {
            RefreshListView();
         }
         else if(e.PropertyName == nameof(AppData.RecognizeQRCode))
         {
            BcReader.RecognizeQRCode = CurrentAppData.RecognizeQRCode;
         }
         else if(e.PropertyName == nameof(AppData.EnableBarcodeDoublePass))
         {
            BcReader.EnableQRDoublePass = CurrentAppData.EnableBarcodeDoublePass;
         }
         else if(e.PropertyName == nameof(AppData.EnableBarcodePreprocessing))
         {
            BcReader.EnableQRPreprocessing = CurrentAppData.EnableBarcodePreprocessing;
         }
      }

      public ObservableCollection<Grouping<string, ContactModel>> SearchContacts(string text, string groupName=null, bool onlyShowContactsNotPartOfTheGroup = false)
      {
         Func<ContactModel, bool> searchCritieria = new Func<ContactModel, bool>((contact) =>
         {
            string lowerText = text.ToLower();

            bool checkText(string str)
            {
               if (!string.IsNullOrEmpty(groupName))
               {
                  if (!String.IsNullOrEmpty(str) && str.ToLower().Contains(lowerText) && ((onlyShowContactsNotPartOfTheGroup) ? (string.IsNullOrEmpty(contact.Group) || (!string.IsNullOrEmpty(contact.Group) && !contact.Group.Equals(groupName))) : (!string.IsNullOrEmpty(contact.Group) && contact.Group.Equals(groupName))))
                     return true;
               }
               else
               {
                  if (!String.IsNullOrEmpty(str) && str.ToLower().Contains(lowerText))
                     return true;
               }

               return false;
            }

            if (checkText(contact.Name.Text))
               return true;
            if (checkText(contact.Company.Text))
               return true;
            if (checkText(contact.Event))
               return true;
            if (checkText(contact.Notes))
               return true;
            if (checkText(contact.Referral))
               return true;
            if (checkText(contact.Group))
               return true;

            foreach (EmailField email in contact.Emails)
            {
               if(checkText(email.Email))
                  return true;
            }

            return false;
         });

         TaskCompletionSource<ObservableCollection<Grouping<string, ContactModel>>> task = new TaskCompletionSource<ObservableCollection<Grouping<string, ContactModel>>>();
         var sorted = from contact in ContactCollection
                      where searchCritieria(contact)
                      orderby contact.Name.Text
                      group contact by contact.NameSort 
                      into countryGroup
                      select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
         var sortedCollection = new ObservableCollection<Grouping<string, ContactModel>>(sorted);
         task.SetResult(sortedCollection);
         return task.Task.Result;
      }

      public ObservableCollection<Grouping<string, ContactModel>> OrganizeContacts(string groupName = null, bool forceSortingByName=false, bool onlyShowContactsNotPartOfTheGroup = false)
      {
         SortBy sortBy = (forceSortingByName) ? SortBy.Name : HomePage.CurrentAppData.SortBy;
         TaskCompletionSource<ObservableCollection<Grouping<string, ContactModel>>> task = new TaskCompletionSource<ObservableCollection<Grouping<string, ContactModel>>>();
         IEnumerable<Grouping<string, ContactModel>> sorted = null;

         switch (sortBy)
         {
            default:
            case SortBy.Name:
               if (!string.IsNullOrEmpty(groupName))
                  sorted = from contact in ContactCollection where ((onlyShowContactsNotPartOfTheGroup) ? (string.IsNullOrEmpty(contact.Group) || (!string.IsNullOrEmpty(contact.Group) && !contact.Group.Equals(groupName))) : (!string.IsNullOrEmpty(contact.Group) && contact.Group.Equals(groupName))) orderby contact.Name.Text group contact by contact.NameSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               else
                  sorted = from contact in ContactCollection orderby contact.Name.Text group contact by contact.NameSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               break;
            case SortBy.Company:
               if (!string.IsNullOrEmpty(groupName))
                  sorted = from contact in ContactCollection where ((onlyShowContactsNotPartOfTheGroup) ? (string.IsNullOrEmpty(contact.Group) || (!string.IsNullOrEmpty(contact.Group) && !contact.Group.Equals(groupName))) : (!string.IsNullOrEmpty(contact.Group) && contact.Group.Equals(groupName))) orderby contact.Company.Text group contact by contact.CompanySort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               else
                  sorted = from contact in ContactCollection orderby contact.Company.Text group contact by contact.CompanySort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               break;
            case SortBy.Date:
               if (!string.IsNullOrEmpty(groupName))
                  sorted = from contact in ContactCollection where ((onlyShowContactsNotPartOfTheGroup) ? (string.IsNullOrEmpty(contact.Group) || (!string.IsNullOrEmpty(contact.Group) && !contact.Group.Equals(groupName))) : (!string.IsNullOrEmpty(contact.Group) && contact.Group.Equals(groupName))) orderby contact.Date group contact by contact.DateSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               else
                  sorted = from contact in ContactCollection orderby contact.Date group contact by contact.DateSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               break;
            case SortBy.Email:
               if (!string.IsNullOrEmpty(groupName))
                  sorted = from contact in ContactCollection where ((onlyShowContactsNotPartOfTheGroup) ? (string.IsNullOrEmpty(contact.Group) || (!string.IsNullOrEmpty(contact.Group) && !contact.Group.Equals(groupName))) : (!string.IsNullOrEmpty(contact.Group) && contact.Group.Equals(groupName))) orderby ((contact.Emails.Count > 0) ? contact.Emails[0].Email : "?") group contact by contact.EmailSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               else
                  sorted = from contact in ContactCollection orderby ((contact.Emails.Count > 0) ? contact.Emails[0].Email : "?") group contact by contact.EmailSort into countryGroup select new Grouping<string, ContactModel>(countryGroup.Key, countryGroup);
               break;
         }

         var sortedCollection = new ObservableCollection<Grouping<string, ContactModel>>(sorted);
         task.SetResult(sortedCollection);

         return task.Task.Result;
      }

      private async void StartOcrEngine()
      {
         string OCR_RUNTIME_DIR = "";

#if __IOS__
         OCR_RUNTIME_DIR = Path.Combine(NSBundle.MainBundle.BundlePath, "content/Resources/LEADTOOLS/OcrLEADRuntime/");
#else
#if __ANDROID__
         OCR_RUNTIME_DIR = DROID_RUNTIME;
#endif
#endif
         try
         {
#if LEADTOOLS_V21_OR_LATER
            _ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.LEAD);
#else
            _ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.LEAD, false);
#endif // #if LEADTOOLS_V21_OR_LATER
            _ocrEngine.Startup(null, null, null, OCR_RUNTIME_DIR);
            IOcrSettingManager settingsManager = _ocrEngine.SettingManager;

            if (_ocrEngine.SettingManager.IsSettingNameSupported("Recognition.RecognitionModuleTradeoff"))
               _ocrEngine.SettingManager.SetEnumValue("Recognition.RecognitionModuleTradeoff", "Accurate");

            if (_ocrEngine.SettingManager.IsSettingNameSupported("Recognition.Threading.MaximumThreads"))
               _ocrEngine.SettingManager.SetIntegerValue("Recognition.Threading.MaximumThreads", 0);

            if (_ocrEngine.SettingManager.IsSettingNameSupported("Recognition.DetectColors"))
               _ocrEngine.SettingManager.SetBooleanValue("Recognition.DetectColors", false);

            if (_ocrEngine.SettingManager.IsSettingNameSupported("Recognition.Preprocess.MobileImagePreprocess"))
               _ocrEngine.SettingManager.SetBooleanValue("Recognition.Preprocess.MobileImagePreprocess", true);

            if (_ocrEngine.SettingManager.IsSettingNameSupported("Recognition.Fonts.RecognizeFontAttributes"))
               _ocrEngine.SettingManager.SetBooleanValue("Recognition.Fonts.RecognizeFontAttributes", false);

            BcReader.OcrEngine = _ocrEngine;
            BcReader.RecognizeQRCode = CurrentAppData.RecognizeQRCode;
            BcReader.EnableQRDoublePass = true;
            BcReader.EnableQRPreprocessing = true;

         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Error starting OCR engine: {ex.Message}", "OK");
         }
      }

      private void CopyDependencies(string appPath)
      {
         try
         {
#if __ANDROID__
            String[] languageFiles = Droid.MainActivity.Instance.Assets.List("LEADTOOLS/OcrLEADRuntime");
            String[] shadowFonts = Droid.MainActivity.Instance.Assets.List("LEADTOOLS/ShadowFonts");
            string runtimeDir = $"{appPath}/../OcrRuntime";
            string fontsDir = $"{appPath}/../ShadowFonts";
            if (!Directory.Exists(fontsDir)) Directory.CreateDirectory(fontsDir);
            if (!Directory.Exists(runtimeDir)) Directory.CreateDirectory(runtimeDir);
            foreach (var file in languageFiles)
            {
               var filePath = $"{runtimeDir}/{file}";
               if (File.Exists(filePath)) continue;
               var stream = Droid.MainActivity.Instance.Assets.Open($"LEADTOOLS/OcrLEADRuntime/{file}", Access.Buffer);
               using (FileStream outStream = new FileStream(filePath, FileMode.CreateNew))
               {
                  stream.CopyTo(outStream);
               }
            }
            foreach (var font in shadowFonts)
            {
               var filePath = $"{fontsDir}/{font}";
               if (File.Exists(filePath)) continue;
               var stream = Droid.MainActivity.Instance.Assets.Open($"LEADTOOLS/ShadowFonts/{font}", Access.Buffer);
               using (FileStream outStream = new FileStream(filePath, FileMode.CreateNew))
               {
                  stream.CopyTo(outStream);
               }
            }

            DROID_RUNTIME = runtimeDir;
            FONTS_DIR = fontsDir;
#elif __IOS__
            FONTS_DIR = Path.Combine(NSBundle.MainBundle.BundlePath, "content/Resources/LEADTOOLS/ShadowFonts");
#endif
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private async void ActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         overlayGrid.IsVisible = true;
         var page = new ActionsPage();
         page.Disappearing += PopupPage_Disappearing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void PopupPage_Disappearing(object sender, EventArgs e)
      {
         Rg.Plugins.Popup.Pages.PopupPage page = sender as Rg.Plugins.Popup.Pages.PopupPage;
         if(page != null)
            page.Disappearing -= PopupPage_Disappearing;

         if (overlayGrid.IsVisible)
            overlayGrid.IsVisible = false;
      }

      public async void RetakeBusinessCard()
      {
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
         if (results == null || results[PermissionType.Camera] != PermissionStatus.Granted)
            return;

         HomePage.BcReader.RecognizeQRCode =  HomePage.CurrentAppData.RecognizeQRCode;
         var page = new CameraPage(CameraOperationType.Normal);
         PushCameraPage(page);
      }

      private void CameraButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         RetakeBusinessCard();
      }

      private async void SettingsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         var page = new SettingsPage();
         await PopupNavigation.Instance.PushAsync(page, true);
      }

      public async void PushCameraPage(PopupPage page)
      {
         await PopupNavigation.Instance.PushAsync(page);
      }

      public async void NavigateToHomePage()
      {
         await PopupNavigation.Instance.PopAllAsync();
      }

      public async void PopCameraPage()
      {
         if(Device.IsInvokeRequired)
            Device.BeginInvokeOnMainThread(async () => { await PopupNavigation.Instance.PopAsync(); });
         else
            await PopupNavigation.Instance.PopAsync();
      }

      public void DetailsPage_ContactSaved(object sender, ContactSavedEventArgs e)
      {
         RefreshListView();
         SaveContactList();
         ContactSavedAction?.Invoke(e.Contact);
      }

      private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
      {
         RefreshListView();
      }

      public void SaveContactList()
      {
         XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ContactModel>));

         using (StreamWriter writer = new StreamWriter($"{APP_DIR}/LeadContactItems.xml"))
         {
            if (ContactCollection != null && ContactCollection.Count > 0)
            {
               serializer.Serialize(writer, ContactCollection);
            }
         }
      }

      public void LoadContactList()
      {
         try
         {

            if (File.Exists($"{APP_DIR}/LeadContactItems.xml"))
            {
               XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ContactModel>));
               using (StreamReader reader = new StreamReader($"{APP_DIR}/LeadContactItems.xml"))
               {
                  ObservableCollection<ContactModel> collection = (ObservableCollection<ContactModel>)serializer.Deserialize(reader);
                  if (collection == null || collection.Count == 0)
                     ContactCollection = new ObservableCollection<ContactModel>();
                  else
                  {
                     // The contacts were saved with previous APP_DIR path which contains unique GUID in the path which won't
                     // exist when load it from another app instance, so we need to replace the previously saved directory with
                     // the current session APP_DIR path.
                     foreach (ContactModel contact in collection)
                     {
                        contact.Picture = Path.Combine(APP_DIR, Path.GetFileName(contact.Picture));
                        contact.Thumbnail = Path.Combine(THUMBS_DIR, Path.GetFileName(contact.Thumbnail));
                        contact.ProfileImage = (!string.IsNullOrEmpty(contact.ProfileImage)) ? Path.Combine(PROFILE_PICS_DIR, Path.GetFileName(contact.ProfileImage)) : string.Empty;
                     }
                     ContactCollection = collection;
                  }
               }
            }
            else
            {
               ContactCollection = new ObservableCollection<ContactModel>();
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            ContactCollection = new ObservableCollection<ContactModel>();
         }
      }

      private bool HideQuickActionControls()
      {
         bool ret = false;
         if (ContactCollection == null) return false;

         IEnumerable<ContactModel> items = ContactCollection.Where(x => x.ShowQuickActions);
         if (items != null && items.Count() > 0)
         {
            foreach (ContactModel contact in items)
            {
               contact.ShowQuickActions = false;
            }
            ret = true;
         }

         return ret;
      }

      public async void ContactsList_Tapped(object sender, ItemTappedEventArgs e)
      {
         HideQuickActionControls();
         ContactModel item = e.Item as ContactModel;
         var index = ContactCollection.IndexOf(item);
         PreviewBusinessCardPage page = new PreviewBusinessCardPage(item, index);
         page.PageClosing += PreviewBusinessCardPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void PreviewBusinessCardPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private void GetAppDirectory()
      {
#if __ANDROID__
         Java.IO.File basedir = Android.OS.Environment.ExternalStorageDirectory;
         Java.IO.File leadDir = new Java.IO.File($"{basedir}/Leadtools");
         if (!leadDir.Exists()) leadDir.Mkdir();
         Java.IO.File appDir = new Java.IO.File($"{leadDir}/BCReaderDemo");
         if (!appDir.Exists()) appDir.Mkdir();
         APP_DIR = appDir.AbsolutePath;
         Java.IO.File droidThumbsDir = new Java.IO.File($@"{APP_DIR}/Thumbnails");
         if (!droidThumbsDir.Exists()) droidThumbsDir.Mkdir();
         THUMBS_DIR = droidThumbsDir.AbsolutePath;
         Java.IO.File droidProfilePicsDir = new Java.IO.File($@"{APP_DIR}/ProfilePictures");
         if (!droidProfilePicsDir.Exists()) droidProfilePicsDir.Mkdir();
         PROFILE_PICS_DIR = droidProfilePicsDir.AbsolutePath;
#elif __IOS__
         var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         var appDir = System.IO.Path.Combine(documents, "BCReaderDemo");
         if (!Directory.Exists(appDir)) Directory.CreateDirectory(appDir);
         APP_DIR = appDir;
         var thumbsDir = System.IO.Path.Combine(APP_DIR, "Thumbnails");
         if (!NSFileManager.DefaultManager.FileExists(thumbsDir))
            NSFileManager.DefaultManager.CreateDirectory(thumbsDir, false, null);
         THUMBS_DIR = thumbsDir;

         var profilePicsDir = System.IO.Path.Combine(APP_DIR, "ProfilePictures");
         if (!NSFileManager.DefaultManager.FileExists(profilePicsDir))
            NSFileManager.DefaultManager.CreateDirectory(profilePicsDir, false, null);
         PROFILE_PICS_DIR = profilePicsDir;
#endif // #if __ANDROID__
      }

      public static void ShowMessage(ContentView notificationMessageView, string message = "")
      {
         Label notificationMessageLabel = notificationMessageView.Content as Label;
         notificationMessageLabel.Text = message;
         notificationMessageView.IsVisible = true;
         notificationMessageView.Opacity = 1;

         if (!string.IsNullOrEmpty(notificationMessageLabel.Text))
            notificationMessageView.FadeTo(0, 3000);
      }

      public async void ShowError(string message, string title = null, bool exit = false)
      {
         if (title == null) title = "Error";
         await DisplayAlert(title, message, "OK");
         if (exit)
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
      }

      private void CardQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         Grid parentGrid = sender as Grid;
         if (parentGrid != null)
         {
            ContactModel contact = (ContactModel)parentGrid.BindingContext;
            if (contact != null)
               contact.ShowQuickActions = true;

            // Slide from right to left effect for the quick actions grid
            Grid itemGrid = parentGrid.Children.OfType<Grid>().FirstOrDefault();
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

      private void PhoneCallQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.DialPhone(contact, this);
         }
      }

      private void EmailQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.ComposeEmail(contact, this);
         }
      }

      private void ShareQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.ShareContact(contact);
         }
      }

      private void DeleteQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.DeleteContact(contact, this);
         }
      }

      private async void ContactsList_ItemLongPressed(object sender, LongPressedEventArgs e)
      {
         ContactModel contact = (e.Item != null) ? e.Item as ContactModel : ContactsGrouped[e.Section][e.Row];
         var page = new SelectCardsPage(Utils.PredefinedActions.All, null, contact, false, mainSearchBar.Text);
         page.PageClosing += SelectCardsPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void ContactsList_Clicked(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }

      private void SelectCardsPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private void MainGrid_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }
   }
}
