﻿// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using Leadtools.Demos;
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
   public partial class GroupsPage : PopupPage
   {
      private ObservableCollection<GroupsPageItem> _itemsList;
      private ObservableCollection<GroupsPageItem> _filteredItemsList;

      public GroupsPage()
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         _itemsList = new ObservableCollection<GroupsPageItem>();

         foreach (string groupName in HomePage.CurrentAppData.GroupPickerItems.Items)
         {
            ObservableCollection<ContactModel> groupContacts = new ObservableCollection<ContactModel>(HomePage.ContactCollection.Where(x => !string.IsNullOrEmpty(x.Group) && x.Group.Equals(groupName)));

            GroupsPageItem item = new GroupsPageItem() { IsSelected = false, GroupName = groupName, Count = groupContacts.Count };
            _itemsList.Add(item);
         }

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

      private async void RefreshListView(bool updateGroupsCounts = false)
      {
         if (updateGroupsCounts)
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
      }

      private void ResizeGroupsGridToFit()
      {
         Device.BeginInvokeOnMainThread(() =>
         {
            int listViewDesiredHeight = _filteredItemsList.Count * GroupsListView.RowHeight;
            int groupsListViewMaxHeight = (int)(DemoUtilities.DisplayHeight - pageUpperControlsGrid.Height - PlatformsConstants.AdRowHeight - listViewHeaderRow.Height.Value - listViewHintRow.Height.Value);
            GroupsListView.HeightRequest = Math.Min(groupsListViewMaxHeight, GroupsListView.Height - (GroupsListView.Height - listViewDesiredHeight));
         });
      }

      private ObservableCollection<GroupsPageItem> SortGroups()
      {
         return new ObservableCollection<GroupsPageItem>(_itemsList.OrderBy(item => item.GroupName));
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         await PopupNavigation.Instance.PopAsync();
      }

      private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
      {
         RefreshListView();
      }

      public ObservableCollection<GroupsPageItem> SearchGroups(string text)
      {
         ObservableCollection<GroupsPageItem> filteredList = new ObservableCollection<GroupsPageItem>(_itemsList.Where(x => x.GroupName.ToLower().Contains(text.ToLower())));
         return filteredList;
      }

      private async void GroupsListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         GroupsPageItem tappedItem = e.Item as GroupsPageItem;

         GroupContactsPage page = new GroupContactsPage(tappedItem.GroupName);
         page.PageClosing += GroupContactsPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void GroupContactsPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView(true);
      }

      private async void AddNewGroupButton_Tapped(object sender, EventArgs e)
      {
         var page = new CreateNewGroupPage(_itemsList, false);
         page.PageClosing += CreateNewGroupPage_PageClosing;
         await PopupNavigation.Instance.PushAsync(page);
      }

      private void CreateNewGroupPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView(true);
      }

      private async void DeleteGroupButton_Clicked(object sender, EventArgs e)
      {
         MenuItem menuItem = sender as MenuItem;
         GroupsPageItem groupItem = menuItem.CommandParameter as GroupsPageItem;
         if (groupItem != null)
         {
            var delete = await DisplayAlert("Delete Group", "Are you sure you want to delete this group?", "Yes", "Cancel");
            if (delete)
            {
               HomePage.CurrentAppData.GroupPickerItems.DeleteItem(groupItem.GroupName);

               _itemsList.Remove(groupItem);
               _filteredItemsList.Remove(groupItem);
               RefreshListView();
            }
         }
      }

      private void RenameGroupButton_Clicked(object sender, EventArgs e)
      {
         MenuItem menuItem = sender as MenuItem;
         GroupsPageItem groupItem = menuItem.CommandParameter as GroupsPageItem;
         if(groupItem != null)
         {
            groupItem.RenameGroup = true;
            RefreshListView();
         }
      }

      private void RenameAcceptButton_Tapped(object sender, EventArgs e)
      {
         GroupsPageItem groupItem = (GroupsPageItem)(sender as Image).BindingContext;
         if (groupItem != null)
         {
            CustomEntry entry = ((sender as Image).Parent as Grid).Children[0] as CustomEntry;
            UpdateGroupName(groupItem, entry.Text);
         }
      }

      private void RenameCancelButton_Tapped(object sender, EventArgs e)
      {
         GroupsPageItem groupItem = (GroupsPageItem)(sender as Image).BindingContext;
         if (groupItem != null)
         {
            CustomEntry entry = ((sender as Image).Parent as Grid).Children[0] as CustomEntry;
            entry.Unfocus();
         }
      }

      private void Entry_Focused(object sender, FocusEventArgs e)
      {
         // This is just a quick workaround to set the cursor to the end of the Entry field text.
         CustomEntry entry = sender as CustomEntry;
         string text = entry.Text;
         entry.Text = string.Empty;
         entry.Text = text;
      }

      private void Entry_Unfocused(object sender, FocusEventArgs e)
      {
         CustomEntry entry = sender as CustomEntry;
         GroupsPageItem groupItem = (GroupsPageItem)entry.BindingContext;
         if (groupItem != null)
         {
            groupItem.RenameGroup = false;
            RefreshListView();
         }
      }

      private void Entry_Completed(object sender, EventArgs e)
      {
         CustomEntry entry = sender as CustomEntry;
         GroupsPageItem groupItem = (GroupsPageItem)entry.BindingContext;
         if(groupItem != null)
            UpdateGroupName(groupItem, entry.Text);
      }

      private void UpdateGroupName(GroupsPageItem groupItem, string text)
      {
         // dont update the contacts if the user left the group name as is.
         if (!string.IsNullOrEmpty(text) && !text.Equals(groupItem.GroupName))
         {
            // Update group picker string items
            HomePage.CurrentAppData.GroupPickerItems.SetItem(groupItem.GroupName, text);

            // We also need to update all contacts with the old group name to use the new name
            IEnumerable<ContactModel> groupContacts = HomePage.ContactCollection.Where(x => !string.IsNullOrEmpty(x.Group) && x.Group.Equals(groupItem.GroupName));
            foreach (ContactModel contact in groupContacts)
            {
               contact.Group = text;
            }

            // Save the updated contact
            HomePage.Instance.SaveContactList();

            groupItem.GroupName = text;
         }

         // This code will hide the rename cell and return the original one
         groupItem.RenameGroup = false;
         RefreshListView();
      }
   }

   public class ListViewDataTemplateSelector : DataTemplateSelector
   {
      public DataTemplate DefaultTemplate { get; set; }
      public DataTemplate RenameTemplate { get; set; }

      protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
      {
         DataTemplate retTemplate = DefaultTemplate;

         GroupsPageItem groupItem = item as GroupsPageItem;
         if(groupItem != null)
            retTemplate = groupItem.RenameGroup ? RenameTemplate : DefaultTemplate;

         return retTemplate;
      }
   }
}
