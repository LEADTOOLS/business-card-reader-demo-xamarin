// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Page
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomPage : PopupPage
   {
      public CustomPage(bool showTopMargin = false, LayoutAlignment footerLogoHorizontalOptions = LayoutAlignment.Start, bool showMinimumContents = false, double topMargin = -1, int cornersRadius = 0)
      {
         Animation = new CustomPageAnimation();
         if (!showTopMargin)
            BackgroundColor = Color.Transparent;
         ShowTopMargin = showTopMargin;
         FooterLogoHorizontalOptions = footerLogoHorizontalOptions;
         ShowMinimumContents = showMinimumContents;
         CornersRadius = cornersRadius;
         if(topMargin > 0)
            TopMargin = topMargin;
         if (Device.RuntimePlatform == Device.iOS)
            HasSystemPadding = false;
      }

      #region Internal properties

      private double TopMargin { get; } = GlobalMarginExtension.UnitSize * 4;
      private int CornersRadius { get; } = 0;
      private View BackButton { get; set; }
      private Label TitleExtraButton { get; set; }
      private bool ShowTopMargin { get; }
      private LayoutAlignment FooterLogoHorizontalOptions { get; }
      private bool ShowMinimumContents { get; }

      #endregion

      #region Public properties

      public bool CanClosePage { get; set; } = true;

      #endregion

      #region Methods

      #region OnXXX

      protected override void OnApplyTemplate()
      {
         base.OnApplyTemplate();

         // Handle back button
         if (GetTemplateChild("PageBackButton") is View backButton)
         {
            BackButton = backButton;

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += BackButton_Tapped;
            BackButton.GestureRecognizers.Add(tapGestureRecognizer);
            BackButton.IsVisible = !ShowMinimumContents;
         }

         // Handle page header visibility
         if (GetTemplateChild("PageTitle") is StackLayout pageTitleLayout)
         {
            pageTitleLayout.IsVisible = !ShowMinimumContents;
         }

         // Handle title extra button
         if (GetTemplateChild("PageTitleExtraButton") is Label titleExtraButton)
         {
            TitleExtraButton = titleExtraButton;

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TitleExtraButton_Tapped;
            TitleExtraButton.GestureRecognizers.Add(tapGestureRecognizer);
         }

         // Handle top margin
         if (ShowTopMargin)
         {
            if (GetTemplateChild("PageRoot") is StackLayout pageRoot)
            {
               pageRoot.Margin = new Thickness(0, TopMargin + DemoUtilities.Instance.SafeAreaTop, 0, 0);
               if (CornersRadius > 0)
                  RoundCornersEffect.SetCornerRadius(pageRoot, CornersRadius);
            }
            if (GetTemplateChild("TopPadding") is BoxView topPadding)
               topPadding.HeightRequest = 0;
         }

         if (GetTemplateChild("PageFooter") is SvgImage pageFooter)
         {
            pageFooter.HorizontalOptions = new LayoutOptions(FooterLogoHorizontalOptions, false);
            pageFooter.IsVisible = !ShowMinimumContents;
         }

         // Handle Ads visibility
         if (GetTemplateChild("Ads") is AdsView ads)
         {
            ads.IsVisible = !ShowMinimumContents;
         }
      }

      protected override bool OnBackButtonPressed()
      {
         if (BackButton.IsEnabled)
         {
            BackButton_Tapped(this, EventArgs.Empty);
            return true;
         }
         return base.OnBackButtonPressed();
      }

      protected override async void OnAppearing()
      {
         base.OnAppearing();

         if (!ShowMinimumContents)
         {
            // Delay a bit, so the ad doesn't appear immediately
            await Task.Delay(1000);

            // Start the ads
            if (GetTemplateChild("Ads") is AdsView ads)
            {
               ads.Start();
            }
         }
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         if (!ShowMinimumContents)
         {
            // Stop the ads
            if (GetTemplateChild("Ads") is AdsView ads)
               ads.Stop();
         }
      }

      #endregion

      #region Events

      public event EventHandler PageClosing;
      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         // Prevent double-tapping
         BackButton.IsEnabled = false;

         PageClosing?.Invoke(this, null);

         if (CanClosePage)
         {
            // Go back
            await PopupNavigation.Instance.PopAsync();
         }
         else
            BackButton.IsEnabled = true;
      }

      public event EventHandler TitleExtraButtonTapped;
      private void TitleExtraButton_Tapped(object sender, EventArgs e)
      {
         // Prevent double-tapping
         TitleExtraButton.IsEnabled = false;

         TitleExtraButtonTapped?.Invoke(TitleExtraButton, null);

         TitleExtraButton.IsEnabled = true;
      }

      #endregion

      #region Helpers

      protected void AfterInitializeComponent()
      {
         // Hide the navigation bar
         NavigationPage.SetHasNavigationBar(this, false);
      }

      public new object GetTemplateChild(string name) => base.GetTemplateChild(name);

      #endregion

      #endregion
   }
}