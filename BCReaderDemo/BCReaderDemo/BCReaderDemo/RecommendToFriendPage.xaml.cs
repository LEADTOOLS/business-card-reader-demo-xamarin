// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class RecommendToFriendPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;
      private string _recommendToFriendMessageBody = "I’ve been using this awesome Business Card Scanner App and thought you’d be interested. The app extracts all the information on any business card and saves it to a virtual card holder with lightning speed and allows you to quickly grab, share, add to contacts, and store the information easily on your phone. The best part? It’s completely FREE. Try for yourself here:" + Environment.NewLine + "https://www.leadtools.com/apps/bcr";

      public RecommendToFriendPage()
      {
         InitializeComponent();

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
