// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SettingsPage : PopupPage
   {
      private ObservableCollection<SettingsFields> Settings { get; set; }

      public SettingsPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         Settings = new ObservableCollection<SettingsFields>
         {
            new SettingsFields { FieldName = "GENERAL"               , AssociatedPageType = typeof(GeneralSettingsPage) },
            new SettingsFields { FieldName = "FIELDS OF INTEREST"    , AssociatedPageType = typeof(FieldsOfInterestPage) },
            new SettingsFields { FieldName = "RECOMMEND TO FRIENDS"  , AssociatedPageType = typeof(RecommendToFriendPage) },
            new SettingsFields { FieldName = "RATE IT NOW"           , AssociatedPageType = typeof(object) },
            new SettingsFields { FieldName = "FOLLOW US"             , AssociatedPageType = typeof(FollowUsPage) },
            new SettingsFields { FieldName = "HELP"                  , AssociatedPageType = typeof(HelpPage) },
         };
         settingsListView.ItemsSource = Settings;
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

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await PopupNavigation.Instance.PopAsync();
      }

      private async void SettingsListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         SettingsFields item = e.Item as SettingsFields;

         if(item.AssociatedPageType == typeof(object))
         {
            // This is the "RATE IT NOW" item, so direct the user to the app page on the AppStore or PlayStore to rate the app.
            await Launcher.OpenAsync(new Uri(HomePage._appUrlOnAppStore));
         }
         else
         {
            var page = Activator.CreateInstance(item.AssociatedPageType) as PopupPage;
            await PopupNavigation.Instance.PushAsync(page, true);
         }
      }
   }
}
