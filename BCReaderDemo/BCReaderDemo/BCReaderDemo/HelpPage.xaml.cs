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
   public partial class HelpPage : PopupPage
   {
      public HelpPage()
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

      private void ContactUsButton_Tapped(object sender, EventArgs e)
      {
         Actions.ComposeEmail("comments@leadtools.com", "Business Card Scanner", string.Empty, this);
      }

      private async void AboutButton_Tapped(object sender, EventArgs e)
      {
         AboutPage page = new AboutPage();
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void PrivacyButton_Tapped(object sender, EventArgs e)
      {
         Actions.VisitWebsite("https://www.leadtools.com/corporate/privacy", this);
      }
   }
}
