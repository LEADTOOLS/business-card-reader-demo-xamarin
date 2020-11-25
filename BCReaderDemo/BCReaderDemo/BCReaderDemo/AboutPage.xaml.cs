// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class AboutPage : PopupPage
   {
      public AboutPage()
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

      private void LeadtoolsWebsiteHyperlink_Tapped(object sender, EventArgs e)
      {
         Actions.VisitWebsite((sender as Label).Text, this);
      }
   }
}
