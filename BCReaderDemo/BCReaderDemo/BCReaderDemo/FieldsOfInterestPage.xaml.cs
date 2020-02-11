// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class FieldsOfInterestPage : ContentPage
   {
      private System.Timers.Timer _adsHiddenTimer = null;
            private System.Timers.Timer _adsVisibleTimer = null;
      private ObservableCollection<SettingsFields> FieldsOfInterest { get; set; }

      public FieldsOfInterestPage()
      {
         InitializeComponent();

         FieldsOfInterest = new ObservableCollection<SettingsFields>
         {
            new SettingsFields { FieldName = "Email", FieldValue = HomePage.CurrentAppData.OptionalFields.Email, FieldEnabled = true },
            new SettingsFields { FieldName = "Company", FieldValue = HomePage.CurrentAppData.OptionalFields.Company, FieldEnabled = true },
            new SettingsFields { FieldName = "Job Title", FieldValue = HomePage.CurrentAppData.OptionalFields.JobTitle, FieldEnabled = true },
            new SettingsFields { FieldName = "Website", FieldValue = HomePage.CurrentAppData.OptionalFields.Website, FieldEnabled = true },
            new SettingsFields { FieldName = "Address", FieldValue = HomePage.CurrentAppData.OptionalFields.Address, FieldEnabled = true },
            new SettingsFields { FieldName = "Note", FieldValue = HomePage.CurrentAppData.OptionalFields.Note, FieldEnabled = true },
            new SettingsFields { FieldName = "Group", FieldValue = HomePage.CurrentAppData.OptionalFields.Group, FieldEnabled = true },
            new SettingsFields { FieldName = "Event", FieldValue = HomePage.CurrentAppData.OptionalFields.Event, FieldEnabled = true },
            new SettingsFields { FieldName = "Referral", FieldValue = HomePage.CurrentAppData.OptionalFields.Referral, FieldEnabled = true },
            new SettingsFields { FieldName = "Reminder", FieldValue = HomePage.CurrentAppData.OptionalFields.Reminder, FieldEnabled = true }
         };
         fieldsOfInterestListView.ItemsSource = FieldsOfInterest;

         // Resize list views to fit its contents
         fieldsOfInterestListView.HeightRequest = FieldsOfInterest.Count * (fieldsOfInterestListView.RowHeight + 1);

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
            case "Email":
               HomePage.CurrentAppData.OptionalFields.Email = switchControl.IsToggled;
               break;
            case "Company":
               HomePage.CurrentAppData.OptionalFields.Company = switchControl.IsToggled;
               break;
            case "Job Title":
               HomePage.CurrentAppData.OptionalFields.JobTitle = switchControl.IsToggled;
               break;
            case "Website":
               HomePage.CurrentAppData.OptionalFields.Website = switchControl.IsToggled;
               break;
            case "Address":
               HomePage.CurrentAppData.OptionalFields.Address = switchControl.IsToggled;
               break;
            case "Note":
               HomePage.CurrentAppData.OptionalFields.Note = switchControl.IsToggled;
               break;
            case "Group":
               HomePage.CurrentAppData.OptionalFields.Group = switchControl.IsToggled;
               break;
            case "Event":
               HomePage.CurrentAppData.OptionalFields.Event = switchControl.IsToggled;
               break;
            case "Referral":
               HomePage.CurrentAppData.OptionalFields.Referral = switchControl.IsToggled;
               break;
            case "Reminder":
               HomePage.CurrentAppData.OptionalFields.Reminder = switchControl.IsToggled;
               break;
         }
      }

      private void disableAllButton_Tapped(object sender, EventArgs e)
      {
         for (int i = 0; i < FieldsOfInterest.Count; i++)
         {
            Switch switchControl = GetListViewItemSwitch(fieldsOfInterestListView, i);
            if (switchControl != null)
               switchControl.IsToggled = false;
         }
      }

      private void enableAllButton_Tapped(object sender, EventArgs e)
      {
         for (int i = 0; i < FieldsOfInterest.Count; i++)
         {
            Switch switchControl = GetListViewItemSwitch(fieldsOfInterestListView, i);
            if (switchControl != null)
               switchControl.IsToggled = true;
         }
      }

      private Switch GetListViewItemSwitch(ListView lv, int index)
      {
         StackLayout stackLayout = (lv.TemplatedItems[index] as ViewCell).View as StackLayout;
         Switch switchControl = stackLayout.Children.OfType<Switch>().FirstOrDefault();
         return switchControl;
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await HomePage.Instance.Navigation.PopAsync();
      }
   }
}
