// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using Leadtools.Demos;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GroupContactsPage : PopupPage
   {
      public ObservableCollection<Grouping<string, ContactModel>> FilteredGroupContacts { get; set; }
      private string _groupName = null;

      public event EventHandler PageClosing;

      public GroupContactsPage(string groupName)
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         _groupName = groupName;
         groupTitle.Text = groupName + " ";

         RefreshListView();
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

      private void BackButton_Tapped(object sender, EventArgs e)
      {
         OnPageClosing();
      }

      protected override bool OnBackButtonPressed()
      {
         OnPageClosing();
         return true;
      }

      private async void OnPageClosing()
      {
         PageClosing?.Invoke(this, null);

         await PopupNavigation.Instance.PopAsync();
      }

      private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
      {
         RefreshListView();
      }

      private async void RefreshListView()
      {
         if (string.IsNullOrWhiteSpace(mainSearchBar.Text))
         {
            FilteredGroupContacts = await Task.Factory.StartNew(() => HomePage.Instance.OrganizeContacts(_groupName));
         }
         else
         {
            FilteredGroupContacts = await Task.Factory.StartNew(() => HomePage.Instance.SearchContacts(mainSearchBar.Text, _groupName));
         }

         ContactsList.ItemsSource = FilteredGroupContacts;
         Device.BeginInvokeOnMainThread(() => { UpdatePageTitle(); });
      }

      private void UpdatePageTitle()
      {
         int count = 0;
         foreach (var group in FilteredGroupContacts)
         {
            count += group.Count;
         }

         string str = string.Format("({0})", count);
         contactsCount.Text = str;
      }

      private async void AddFromCardsHolderButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         var page = new SelectCardsPage(Utils.PredefinedActions.Group, _groupName, null, true, null);
         page.PageClosing += SelectCardsPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private async void ContactsList_ItemLongPressed(object sender, LongPressedEventArgs e)
      {
         ContactModel itemData = (e.Item != null) ? e.Item as ContactModel : FilteredGroupContacts[e.Section][e.Row];
         var page = new SelectCardsPage(PredefinedActions.All, _groupName, itemData, false, null);
         page.PageClosing += SelectCardsPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void SelectCardsPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private async void ContactsList_Tapped(object sender, ItemTappedEventArgs e)
      {
         HideQuickActionControls();
         ContactModel item = e.Item as ContactModel;
         var index = HomePage.ContactCollection.IndexOf(item);
         var page = new PreviewBusinessCardPage(item, index);
         page.PageClosing += PreviewBusinessCardPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void PreviewBusinessCardPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private bool HideQuickActionControls()
      {
         bool ret = false;
         IEnumerable<ContactModel> items = HomePage.ContactCollection.Where(x => !string.IsNullOrEmpty(x.Group) && x.Group.Equals(_groupName) && x.ShowQuickActions);
         if (items != null && items.Count() > 0)
         {
            foreach (ContactModel contact in items)
            {
               contact.ShowQuickActions = false;
            }
            ret = true;
         }

         return ret;
      }

      private void CardQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         Grid parentGrid = sender as Grid;
         if (parentGrid != null)
         {
            ContactModel contact = (ContactModel)parentGrid.BindingContext;
            if (contact != null)
               contact.ShowQuickActions = true;

            // Slide from right to left effect for the quick actions grid
            Grid itemGrid = parentGrid.Children.OfType<Grid>().FirstOrDefault();
            if (itemGrid != null && itemGrid.Children.Count > 1)
            {
               Grid actionsGrid = itemGrid.Children[1] as Grid;
               if (actionsGrid != null)
               {
                  actionsGrid.TranslationX = DemoUtilities.DisplayWidth;
                  actionsGrid.TranslateTo(-1, 0);
               }
            }
         }
      }

      private void PhoneCallQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.DialPhone(contact, this);
         }
      }

      private void EmailQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.ComposeEmail(contact, this);
         }
      }

      private void ShareQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            Actions.ShareContact(contact);
         }
      }

      private void RemoveFromGroupQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         ContactModel contact = (ContactModel)((Image)sender).BindingContext;
         if (contact != null)
         {
            contact.Group = null;
            RefreshListView();
         }
      }

      private async void CameraAddButton_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
         if (results == null || results[PermissionType.Camera] != PermissionStatus.Granted)
            return;

         HomePage.Instance.ContactSavedAction = new Action<ContactModel>((contact) =>
         {
            contact.Group = _groupName;
            RefreshListView();
         });

         var page = new CameraPage(CameraOperationType.Normal);

         HomePage.Instance.PushCameraPage(page);
      }

      private void MainGrid_Tapped(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }

      private void ContactsList_Clicked(object sender, EventArgs e)
      {
         HideQuickActionControls();
      }
   }
}
