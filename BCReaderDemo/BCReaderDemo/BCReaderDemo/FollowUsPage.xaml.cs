// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using System;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class FollowUsPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;
      private ObservableCollection<FollowUsFields> FollowUs { get; set; }

      public FollowUsPage()
      {
         InitializeComponent();

         FollowUs = new ObservableCollection<FollowUsFields>
         {
            new FollowUsFields { FieldName = "LIKE US ON FACEBOOK"         , FieldTextColor = CustomColors.FacebookTextColor  , FieldLink = HomePage.FacebookUrl  , FieldIcon = "facebook.png" },
            new FollowUsFields { FieldName = "CONNECT WITH US ON LINKED-IN", FieldTextColor = CustomColors.LinkedInTextColor  , FieldLink = HomePage.LinkedInUrl  , FieldIcon = "linkedin.png" },
            new FollowUsFields { FieldName = "FOLLOW US ON TWITTER"        , FieldTextColor = CustomColors.TwitterTextColor   , FieldLink = HomePage.TwitterUrl   , FieldIcon = "twitter.png" },
            new FollowUsFields { FieldName = "WATCH US ON YOUTUBE"         , FieldTextColor = CustomColors.YoutubeTextColor   , FieldLink = HomePage.YoutubeUrl   , FieldIcon = "youtube.png" },
         };
         followUsListView.ItemsSource = FollowUs;

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

      private async void FollowUsListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         FollowUsFields item = e.Item as FollowUsFields;
         await Launcher.OpenAsync(new Uri(item.FieldLink));
      }
   }
}
