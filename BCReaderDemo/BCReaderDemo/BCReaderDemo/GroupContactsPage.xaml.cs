// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
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
   public partial class GroupContactsPage : ContentPage
   {
      public ObservableCollection<Grouping<string, ContactModel>> FilteredGroupContacts { get; set; }
      private string _groupName = null;
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;

      public event EventHandler PageClosing;

      public GroupContactsPage(string groupName)
      {
         InitializeComponent();

         _groupName = groupName;
         groupTitle.Text = groupName + " ";

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

         RefreshListView();
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

         await HomePage.Instance.Navigation.PopAsync();
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
         await HomePage.Instance.Navigation.PushAsync(page);
      }

      private async void ContactsList_ItemLongPressed(object sender, ContactSavedEventArgs e)
      {
         var page = new SelectCardsPage(PredefinedActions.All, _groupName, e.Contact, false, null);
         page.PageClosing += SelectCardsPage_PageClosing;
         await HomePage.Instance.Navigation.PushAsync(page);
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
         await HomePage.Instance.Navigation.PushAsync(page);
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
                  actionsGrid.TranslationX = App.DisplayScreenWidth;
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
         var results = await DependencyService.Get<Permissions.IPermissions>().VerifyPermissionsAsync(Permissions.Permission.Camera);
         if (results == null || results[Permissions.Permission.Camera] != Permissions.PermissionStatus.Granted)
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
