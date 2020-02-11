// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class AboutPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;

      public AboutPage()
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

      private void LeadtoolsWebsiteHyperlink_Tapped(object sender, EventArgs e)
      {
         Actions.VisitWebsite((sender as Label).Text, this);
      }
   }
}
