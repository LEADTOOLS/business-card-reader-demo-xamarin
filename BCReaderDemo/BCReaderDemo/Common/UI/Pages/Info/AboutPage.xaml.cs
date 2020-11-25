// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Page;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Pages.Info
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class AboutPage : CustomPage
   {
      #region Config

      private static string LeadHomepage { get; } = "https://www.leadtools.com";
      public static string CopyrightInfo => $"{LeadHomepage}{Environment.NewLine}Copyright \u00a9 1991-2020 All Rights Reserved.{Environment.NewLine}LEAD Technologies, Inc.{Environment.NewLine}Version {DemoUtilities.AppVersion}";

      #endregion

      public AboutPage()
      {
         Resources.Add("TitleText", DemoUtilities.AppTitle);
         InitializeComponent();
         AfterInitializeComponent();
      }

      #region Methods

      #region Events

      private async void VisitWebsiteButton_Tapped(object sender, EventArgs e) => await Browser.OpenAsync($"{LeadHomepage}?{DemoUtilities.QueryString("visit")}");

      #endregion

      #endregion
   }
}