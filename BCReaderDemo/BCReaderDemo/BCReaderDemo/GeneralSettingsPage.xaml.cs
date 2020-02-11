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
   public partial class GeneralSettingsPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;
      private ObservableCollection<SettingsFields> GeneralSettings { get; set; }

      public GeneralSettingsPage()
      {
         InitializeComponent();

         GeneralSettings = new ObservableCollection<SettingsFields>
         {
            new SettingsFields { FieldName = "Auto-Save to Contacts", FieldValue = HomePage.CurrentAppData.AutoSaveToContacts, FieldEnabled = true },
            new SettingsFields { FieldName = "Enable Camera Auto Capture", FieldValue = HomePage.CurrentAppData.EnableCameraAutoCapture, FieldEnabled = true },
            new SettingsFields { FieldName = "Recognize QR Code", FieldValue = HomePage.CurrentAppData.RecognizeQRCode, FieldEnabled = true },
            new SettingsFields { FieldName = "Camera Quality", FieldValue = false, FieldEnabled = true, StringValue = HomePage.CurrentAppData.CameraQuality.ToString() },
         };
         generalSettingsListView.ItemsSource = GeneralSettings;

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
         await HomePage.Instance.Navigation.PopAsync();
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
