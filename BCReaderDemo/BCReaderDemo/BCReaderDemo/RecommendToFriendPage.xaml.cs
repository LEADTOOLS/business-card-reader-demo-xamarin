// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class RecommendToFriendPage : PopupPage
   {
      private string _recommendToFriendMessageBody = "I’ve been using this awesome Business Card Scanner App and thought you’d be interested. The app extracts all the information on any business card and saves it to a virtual card holder with lightning speed and allows you to quickly grab, share, add to contacts, and store the information easily on your phone. The best part? It’s completely FREE. Try for yourself here:" + Environment.NewLine + "https://www.leadtools.com/apps/bcr";

      public RecommendToFriendPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif
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

      private void FacebookLayout_Tapped(object sender, EventArgs e)
      {
         Actions.VisitWebsite(HomePage.FacebookUrl, this);
      }

      private void TwitterLayout_Tapped(object sender, EventArgs e)
      {
         Actions.VisitWebsite(HomePage.TwitterUrl, this);
      }

      private void SmsLayout_Tapped(object sender, EventArgs e)
      {
         Actions.ComposeSms(string.Empty, _recommendToFriendMessageBody, this);
      }

      private void EmailLayout_Tapped(object sender, EventArgs e)
      {
         Actions.ComposeEmail(string.Empty, "Recommend you to try LEADTOOLS Business Card Scanner", _recommendToFriendMessageBody, this);
      }
   }
}
