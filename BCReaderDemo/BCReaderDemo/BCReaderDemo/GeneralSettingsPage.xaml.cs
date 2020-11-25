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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GeneralSettingsPage : PopupPage
   {
      private ObservableCollection<SettingsFields> GeneralSettings { get; set; }

      public GeneralSettingsPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         GeneralSettings = new ObservableCollection<SettingsFields>
         {
            new SettingsFields { FieldName = "Auto-Save to Contacts", FieldValue = HomePage.CurrentAppData.AutoSaveToContacts, FieldEnabled = true },
            new SettingsFields { FieldName = "Enable Camera Auto Capture", FieldValue = HomePage.CurrentAppData.EnableCameraAutoCapture, FieldEnabled = true },
            new SettingsFields { FieldName = "Recognize QR Code", FieldValue = HomePage.CurrentAppData.RecognizeQRCode, FieldEnabled = true },
            new SettingsFields { FieldName = "Camera Quality", FieldValue = false, FieldEnabled = true, StringValue = HomePage.CurrentAppData.CameraQuality.ToString() },
         };
         generalSettingsListView.ItemsSource = GeneralSettings;
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

      private void ListViewItemSwitch_Toggled(object sender, ToggledEventArgs e)
      {
         Switch switchControl = (Switch)sender;

         switch (switchControl.ClassId)
         {
            case "Auto-Save to Contacts":
               HomePage.CurrentAppData.AutoSaveToContacts = switchControl.IsToggled;
               break;

            case "Enable Camera Auto Capture":
               HomePage.CurrentAppData.EnableCameraAutoCapture = switchControl.IsToggled;
               break;

            case "Recognize QR Code":
               HomePage.CurrentAppData.RecognizeQRCode = switchControl.IsToggled;
               break;
         }
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await PopupNavigation.Instance.PopAsync();
      }

      private async void CameraQuality_Tapped(object sender, EventArgs e)
      {
         string quality = await DisplayActionSheet(null, "Cancel", null, new string[] { "Medium", "High" });
         if (!String.IsNullOrEmpty(quality) && !"Cancel".Equals(quality))
         {
            if (quality.Equals(CameraQuality.Medium.ToString()))
               HomePage.CurrentAppData.CameraQuality = CameraQuality.Medium;
            else
               HomePage.CurrentAppData.CameraQuality = CameraQuality.High;

            GeneralSettings[GeneralSettings.Count - 1].StringValue = HomePage.CurrentAppData.CameraQuality.ToString();
         }
      }
   }

   public class GeneralSettingsListViewDataTemplateSelector : DataTemplateSelector
   {
      public DataTemplate DefaultTemplate { get; set; }
      public DataTemplate CameraQualityTemplate { get; set; }

      protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
      {
         DataTemplate retTemplate = DefaultTemplate;

         SettingsFields listViewItem = item as SettingsFields;
         if (listViewItem != null)
            retTemplate = listViewItem.FieldName.Equals("Camera Quality") ? CameraQualityTemplate : DefaultTemplate;

         return retTemplate;
      }
   }
}
