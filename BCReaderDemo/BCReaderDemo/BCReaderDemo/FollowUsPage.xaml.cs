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
   public partial class FollowUsPage : PopupPage
   {
      private ObservableCollection<FollowUsFields> FollowUs { get; set; }

      public FollowUsPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         FollowUs = new ObservableCollection<FollowUsFields>
         {
            new FollowUsFields { FieldName = "LIKE US ON FACEBOOK"         , FieldTextColor = CustomColors.FacebookTextColor  , FieldLink = HomePage.FacebookUrl  , FieldIcon = "Icons/fb-ico.svg" },
            new FollowUsFields { FieldName = "CONNECT WITH US ON LINKED-IN", FieldTextColor = CustomColors.LinkedInTextColor  , FieldLink = HomePage.LinkedInUrl  , FieldIcon = "Icons/linked-in-ico.svg" },
            new FollowUsFields { FieldName = "FOLLOW US ON TWITTER"        , FieldTextColor = CustomColors.TwitterTextColor   , FieldLink = HomePage.TwitterUrl   , FieldIcon = "Icons/twitter-ico.svg" },
            new FollowUsFields { FieldName = "WATCH US ON YOUTUBE"         , FieldTextColor = CustomColors.YoutubeTextColor   , FieldLink = HomePage.YoutubeUrl   , FieldIcon = "Icons/youtube-ico.svg" },
         };
         followUsListView.ItemsSource = FollowUs;
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

      private async void FollowUsListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         FollowUsFields item = e.Item as FollowUsFields;
         await Launcher.OpenAsync(new Uri(item.FieldLink));
      }
   }
}
