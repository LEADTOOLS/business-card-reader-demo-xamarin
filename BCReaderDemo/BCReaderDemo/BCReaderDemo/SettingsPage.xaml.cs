// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SettingsPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;
      private ObservableCollection<SettingsFields> Settings { get; set; }

      public SettingsPage()
      {
         InitializeComponent();

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
      }

      protected override void OnAppearing()
      {
         base.OnAppearing();

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

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         if (_adsHiddenTimer != null)
         {
            _adsHiddenTimer.Stop();
            _adsHiddenTimer.Enabled = false;
         }
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await HomePage.Instance.Navigation.PopAsync();
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
            var page = Activator.CreateInstance(item.AssociatedPageType) as Page;
            await HomePage.Instance.Navigation.PushAsync(page, true);
         }
      }
   }
}
