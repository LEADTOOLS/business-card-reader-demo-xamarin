// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Utils;
using Leadtools.Demos.UI.Elements;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class FieldsOfInterestPage : PopupPage
   {
      private ObservableCollection<SettingsFields> FieldsOfInterest { get; set; }

      public FieldsOfInterestPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

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
         StackLayout stackLayout = (lv.TemplatedItems[index] as CustomViewCell).View as StackLayout;
         Switch switchControl = stackLayout.Children.OfType<Switch>().FirstOrDefault();
         return switchControl;
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await PopupNavigation.Instance.PopAsync();
      }
   }
}
