// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.UI.Page;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Pages.Info
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class InfoPage : CustomPage
   {
      public InfoPage()
      {
         InitializeComponent();
         AfterInitializeComponent();
         NavigationList.ItemsSource = new NavigationListViewEntry[]
         {
            new NavigationListViewEntry("About Us", AboutUs_Tapped),
            new NavigationListViewEntry("Recommend to Friends", RecommendToFriends_Tapped),
            new NavigationListViewEntry("Rate it Now", RateItNow_Tapped),
            new NavigationListViewEntry("Follow Us", FollowUs_Tapped),
            new NavigationListViewEntry("Contact Us", ContactUs_Tapped),
            new NavigationListViewEntry("Privacy", Privacy_Tapped),
         };
      }

      #region Methods

      #region Helpers

      private async Task AboutUs_Tapped() => await PopupNavigation.Instance.PushAsync(new AboutPage());
      private async Task RecommendToFriends_Tapped() => await PopupNavigation.Instance.PushAsync(new RecommendPage());
      private async Task RateItNow_Tapped()
      {
         bool succeeded = await FeedbackPage.RateItNow();
         if(!succeeded)
            await DisplayAlert("Error", "Invalid feedback endpoint URL", "OK");
      }
      private async Task FollowUs_Tapped() => await PopupNavigation.Instance.PushAsync(new FollowPage());
      private async Task ContactUs_Tapped()
      {
         try
         {
            string command = string.Format("mailto:{0}?subject={1}&body={2}", "support@leadtools.com", DemoUtilities.AppShareName, string.Empty);
            await Launcher.OpenAsync(new Uri(new Uri(command).AbsoluteUri));
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to compose email: {ex.Message}", "OK");
         }
      }
      private async Task Privacy_Tapped() => await Browser.OpenAsync($"https://www.leadtools.com/corporate/privacy?{DemoUtilities.QueryString("privacy")}");

      #endregion

      #endregion
   }
}