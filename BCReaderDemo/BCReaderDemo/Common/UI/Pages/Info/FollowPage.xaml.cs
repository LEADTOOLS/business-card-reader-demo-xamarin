// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Page;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Pages.Info
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class FollowPage : CustomPage
   {
      public FollowPage()
      {
         InitializeComponent();
         AfterInitializeComponent();
         OptionsList.ItemsSource = new FollowOption[]
         {
            new FollowOption("LIKE US ON FACEBOOK", "https://www.facebook.com/LEADTOOLS", Color.FromRgb(64, 93, 154), "Icons/fb-ico.svg"),
            new FollowOption("LIKE US ON LINKEDIN", "https://www.linkedin.com/company/2538408", Color.FromRgb(24, 127, 184), "Icons/linked-in-ico.svg"),
            new FollowOption("FOLLOW US ON TWITTER", "https://twitter.com/LEADTOOLS", Color.FromRgb(41, 169, 225), "Icons/twitter-ico.svg"),
            new FollowOption("WATCH US ON YOUTUBE", "https://www.youtube.com/LEADTOOLSSDK", Color.FromRgb(195, 39, 26), "Icons/youtube-ico.svg")
         };
      }

      #region Methods

      #region Events

      private async void OptionsList_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         if (e.Item is FollowOption option)
            await Browser.OpenAsync(option.Url);
      }

      #endregion

      #endregion

      #region FollowOption

      private class FollowOption
      {
         public FollowOption(string labelText, string url, Color textColor, string imageResourceName)
         {
            LabelText = labelText;
            Url = url;
            TextColor = textColor;
            ImageResourceName = imageResourceName;
         }

         #region Public properties

         public string ImageResourceName { get; }
         public string LabelText { get; }
         public Color TextColor { get; }
         public string Url { get; }

         #endregion
      }

      #endregion
   }
}