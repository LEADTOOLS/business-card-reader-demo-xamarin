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
   public partial class SelectGroupPage : ContentPage
   {
      private ObservableCollection<GroupsPageItem> _itemsList;
      private ObservableCollection<GroupsPageItem> _filteredItemsList;
      private string _selectedGroupName = null;
      private System.Timers.Timer _adsHiddenTimer = null;
      private System.Timers.Timer _adsVisibleTimer = null;

      public event EventHandler PageClosing;

      public SelectGroupPage(ObservableCollection<GroupsPageItem> itemsList)
      {
         InitializeComponent();

         if (itemsList == null)
         {
            _itemsList = new ObservableCollection<GroupsPageItem>();

            foreach (string groupName in HomePage.CurrentAppData.GroupPickerItems.Items)
            {
               ObservableCollection<ContactModel> groupContacts = new ObservableCollection<ContactModel>(HomePage.ContactCollection.Where(x => !string.IsNullOrEmpty(x.Group) && x.Group.Equals(groupName)));

               GroupsPageItem item = new GroupsPageItem() { IsSelected = false, GroupName = groupName, Count = groupContacts.Count };
               _itemsList.Add(item);
            }
         }
         else
            _itemsList = itemsList;

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

      private async void RefreshListView(bool updateGroupsCounts = false)
      {
         if(updateGroupsCounts)
         {
            foreach (GroupsPageItem group in _itemsList)
            {
               ObservableCollection<ContactModel> groupContacts = new ObservableCollection<ContactModel>(HomePage.ContactCollection.Where(x => !string.IsNullOrEmpty(x.Group) && x.Group.Equals(group.GroupName)));
               group.Count = groupContacts.Count;
            }
         }

         // order collection items by group name
         _filteredItemsList = SortGroups();

         if (!string.IsNullOrWhiteSpace(mainSearchBar.Text))
            _filteredItemsList = await Task.Factory.StartNew(() => SearchGroups(mainSearchBar.Text));

         GroupsListView.ItemsSource = null;
         GroupsListView.ItemsSource = _filteredItemsList;

         ResizeGroupsGridToFit();

         IEnumerable<GroupsPageItem> selectedItems = _filteredItemsList.Where(x => x.IsSelected);
         if(selectedItems != null && selectedItems.Count() > 0)
         {
            _selectedGroupName = selectedItems.ElementAt(0).GroupName;
            Device.BeginInvokeOnMainThread(() =>
            {
               GroupsListView.ScrollTo(selectedItems.ElementAt(0), ScrollToPosition.MakeVisible, true);
            });
         }
      }

      private void ResizeGroupsGridToFit()
      {
         Device.BeginInvokeOnMainThread(() =>
         {
            int listViewDesiredHeight = _filteredItemsList.Count * GroupsListView.RowHeight;
            int groupsListViewMaxHeight = (int)(App.DisplayScreenHeight - pageUpperControlsGrid.Height - PlatformsConstants.AdRowHeight - listViewHeaderRow.Height.Value);
            GroupsListView.HeightRequest = Math.Min(groupsListViewMaxHeight, GroupsListView.Height - (GroupsListView.Height - listViewDesiredHeight));
         });
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

      private async void OnPageClosing(bool closePage = true)
      {
         PageClosing?.Invoke(this, null);

         if (closePage)
            await HomePage.Instance.Navigation.PopAsync();
      }

      private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
      {
         RefreshListView();
      }

      public ObservableCollection<GroupsPageItem> SearchGroups(string text)
      {
         ObservableCollection<GroupsPageItem> filteredList = new ObservableCollection<GroupsPageItem>(_itemsList.Where(x => x.GroupName.Contains(text)));
         return filteredList;
      }

      private ObservableCollection<GroupsPageItem> SortGroups()
      {
         return new ObservableCollection<GroupsPageItem>(_itemsList.OrderBy(item => item.GroupName));
      }

      private void GroupsListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         ListView lv = sender as ListView;

         ClearPreviouslySelectedGroups();
         GroupsPageItem tappedItem = e.Item as GroupsPageItem;
         tappedItem.IsSelected = true;
         _selectedGroupName = tappedItem.GroupName;
      }

      private void ClearPreviouslySelectedGroups()
      {
         foreach(GroupsPageItem item in _itemsList)
         {
            item.IsSelected = false;
         }
      }

      private async void AddNewGroupButton_Tapped(object sender, EventArgs e)
      {
         var page = new CreateNewGroupPage(_itemsList, true);
         page.PageClosing += CreateNewGroupPage_PageClosing;
         await HomePage.Instance.Navigation.PushAsync(page);
      }

      private void CreateNewGroupPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private async void AddToSelectedGroupButton_Clicked(object sender, EventArgs e)
      {
         if(string.IsNullOrEmpty(_selectedGroupName))
         {
            Device.BeginInvokeOnMainThread(() => HomePage.ShowMessage(notificationMessageView, "No group selected"));
            return;
         }

         foreach (var group in HomePage.ContactsGrouped)
         {
            foreach (var contact in group)
            {
               if (contact.IsSelected)
               {
                  contact.Group = _selectedGroupName;
               }
            }
         }

         // Save the updated contact
         HomePage.Instance.SaveContactList();

         RefreshListView(true);

         GroupContactsPage page = new GroupContactsPage(_selectedGroupName);
         page.PageClosing += GroupContactsPage_PageClosing;
         await HomePage.Instance.Navigation.PushAsync(page);
         Navigation.RemovePage(this);
      }

      private void GroupContactsPage_PageClosing(object sender, EventArgs e)
      {
         OnPageClosing(false);
      }
   }
}
