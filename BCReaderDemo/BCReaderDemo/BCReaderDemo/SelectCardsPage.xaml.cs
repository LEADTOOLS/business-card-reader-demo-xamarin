// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using DataService;
using Leadtools.Demos.Utils;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BCReaderDemo
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SelectCardsPage : PopupPage
   {
      private ObservableCollection<Grouping<string, ContactModel>> _contactsGrouped = null;
      private string _preSelectedGroupName = null;
      private bool _addFromCardsHolder;

      public bool IsSingleActionPage { get; set; }
      public string SingleActionName { get; set; }

      public event EventHandler PageClosing;

      public SelectCardsPage(PredefinedActions action, string preSelectedGroupName, ContactModel defaultSelectedItem, bool addFromCardsHolder, string defaultSearchCardsString)
      {
         InitializeComponent();
#if __IOS__
         HasSystemPadding = false;
#endif

         Init(action, preSelectedGroupName, defaultSelectedItem, addFromCardsHolder, defaultSearchCardsString);
      }

      private void Init(PredefinedActions action, string preSelectedGroupName, ContactModel defaultSelectedItem, bool addFromCardsHolder, string defaultSearchCardsString)
      {
         _addFromCardsHolder = addFromCardsHolder;
         _preSelectedGroupName = preSelectedGroupName;

         // Only show the remove from group action button inside this page if the cards shown are part of preselected group and no preselected action passed.
         if (action == PredefinedActions.All && string.IsNullOrWhiteSpace(preSelectedGroupName))
         {
            actionButtonsGrid.ColumnDefinitions.RemoveAt(actionButtonsGrid.ColumnDefinitions.Count - 1);
            actionButtonsGrid.Children.Remove(removeFromGroupActionButton);
         }

         if (addFromCardsHolder)
            DeselectAllCards();

         if (defaultSelectedItem != null)
            defaultSelectedItem.IsSelected = true;

         IsSingleActionPage = action != PredefinedActions.All;
         if(IsSingleActionPage)
         {
            contactsListRow.Height = new GridLength(1, GridUnitType.Star);
            actionButtonsRow.Height = new GridLength(1, GridUnitType.Auto);
         }

         switch (action)
         {
            case PredefinedActions.Group:
               SingleActionName = string.IsNullOrEmpty(preSelectedGroupName) ? "Add To Group" : "Add To [ " + preSelectedGroupName + " ]";
               actionButton.Clicked += GroupButton_Tapped;
               break;

            case PredefinedActions.Share:
               SingleActionName = "Share";
               actionButton.Clicked += ShareButton_Tapped;
               break;

            case PredefinedActions.SaveToContacts:
               SingleActionName = "Save to contacts";
               actionButton.Clicked += SaveToContactsButton_Tapped;
               break;

            case PredefinedActions.Delete:
               SingleActionName = "Delete";
               actionButton.Clicked += DeleteCardsButton_Tapped;
               break;
         }

         if (!string.IsNullOrEmpty(defaultSearchCardsString))
            mainSearchBar.Text = defaultSearchCardsString;
         else
            RefreshListView(defaultSelectedItem);

         BindingContext = this;
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

      private async void RefreshListView(ContactModel defaultSelectedItem = null)
      {
         if (!string.IsNullOrWhiteSpace(mainSearchBar.Text))
            _contactsGrouped = await Task.Factory.StartNew(() => HomePage.Instance.SearchContacts(mainSearchBar.Text, _preSelectedGroupName, (_addFromCardsHolder) ? true : false));
         else
         {
            // only show the contacts that is not part of the current group
            _contactsGrouped = await Task.Factory.StartNew(() => HomePage.Instance.OrganizeContacts(_preSelectedGroupName, false, (_addFromCardsHolder) ? true : false));
         }

         ContactsList.ItemsSource = _contactsGrouped;

         if(Device.IsInvokeRequired)
         {
            Device.BeginInvokeOnMainThread(() =>
            {
               if (defaultSelectedItem != null)
                  ContactsList.ScrollTo(defaultSelectedItem, ScrollToPosition.Center, true);
               UpdatePageTitle();
               UpdateSelectAllButtonText();
            });
         }
         else
         {
            if (defaultSelectedItem != null)
               ContactsList.ScrollTo(defaultSelectedItem, ScrollToPosition.Center, true);
            UpdatePageTitle();
            UpdateSelectAllButtonText();
         }
      }

      private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
      {
         RefreshListView();
      }

      private void CancelButton_Tapped(object sender, EventArgs e)
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
         DeselectAllCards();

         PageClosing?.Invoke(this, null);

         await PopupNavigation.Instance.PopAsync();
      }

      private void SelectAllButton_Tapped(object sender, EventArgs e)
      {
         int selectedCount = 0;
         foreach (var group in _contactsGrouped)
         {
            foreach (var contact in group)
            {
               contact.IsSelected = (SelectAllButton.Text.Equals("Select All")) ? true : false;
               if (contact.IsSelected)
                  selectedCount++;
            }
         }

         UpdatePageTitle();
         UpdateSelectAllButtonText();
      }

      private void DeselectAllCards()
      {
         foreach (var contact in HomePage.ContactCollection)
         {
            contact.IsSelected = false;
         }

         HomePage.Instance.SaveContactList();
      }

      private void ContactsList_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         ContactModel tappedItem = e.Item as ContactModel;
         if (tappedItem == null) return;

         tappedItem.IsSelected = !tappedItem.IsSelected;

         UpdatePageTitle();
         UpdateSelectAllButtonText();
      }

      private void UpdatePageTitle()
      {
         int selectedItemsCount = 0;
         foreach (var group in _contactsGrouped)
         {
            foreach (var contact in group)
            {
               if (contact.IsSelected)
                  selectedItemsCount++;
            }
         }
         string selectedCardsCountLabel = string.Format("({0})", selectedItemsCount);
         selectedCardsCount.Text = selectedCardsCountLabel;
      }

      private void UpdateSelectAllButtonText()
      {
         SelectAllButton.IsVisible = _contactsGrouped.Count > 0;

         if (IsAllItemsSelected())
            SelectAllButton.Text = "Unselect All";
         else
            SelectAllButton.Text = "Select All";
      }

      private List<ContactModel> GetSelectedItems()
      {
         List<ContactModel> selectedItems = new List<ContactModel>();
         foreach (var group in _contactsGrouped)
         {
            foreach (var contact in group)
            {
               if (contact.IsSelected)
                  selectedItems.Add(contact);
            }
         }

         return selectedItems;
      }

      private bool IsAllItemsSelected()
      {
         int allItemsCount = 0;
         int selectedItemsCount = 0;
         foreach (var group in _contactsGrouped)
         {
            allItemsCount += group.Count;
            foreach (var contact in group)
            {
               if (contact.IsSelected)
                  selectedItemsCount++;
            }
         }

         return (allItemsCount > 0 && selectedItemsCount == allItemsCount);
      }

      private bool IsAnyItemSelected()
      {
         foreach (var group in _contactsGrouped)
         {
            foreach (var contact in group)
            {
               if (contact.IsSelected)
                  return true;
            }
         }

         return false;
      }

      private async void GroupButton_Tapped(object sender, EventArgs e)
      {
         if (!IsAnyItemSelected())
         {
            HomePage.ShowMessage(notificationMessageView, "At least one card should be selected");
            return;
         }

         if(!IsSingleActionPage || string.IsNullOrEmpty(_preSelectedGroupName))
         {
            SelectGroupPage page = new SelectGroupPage(null);
            page.PageClosing += SelectGroupPage_PageClosing;
            await PopupNavigation.Instance.PushAsync(page);
         }
         else
         {
            foreach (var group in _contactsGrouped)
            {
               foreach (var contact in group)
               {
                  if (contact.IsSelected)
                  {
                     contact.Group = _preSelectedGroupName;
                  }
               }
            }

            // Save the updated contact
            HomePage.Instance.SaveContactList();

            // Return to previous page
            OnPageClosing();
         }
      }

      private void SelectGroupPage_PageClosing(object sender, EventArgs e)
      {
         RefreshListView();
      }

      private async void ShareButton_Tapped(object sender, EventArgs e)
      {
         if (!IsAnyItemSelected())
         {
            HomePage.ShowMessage(notificationMessageView, "At least one card should be selected");
            return;
         }

         List<ContactModel> selectedItems = GetSelectedItems();
         await VCardUtils.ShareBusinessCards(selectedItems);
      }

      private async void SaveToContactsButton_Tapped(object sender, EventArgs e)
      {
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Contacts);
         if (results == null || results[PermissionType.Contacts] != PermissionStatus.Granted)
            return;

         if (!IsAnyItemSelected())
         {
            HomePage.ShowMessage(notificationMessageView, "At least one card should be selected");
            return;
         }

         await Task.Factory.StartNew(() =>
         {
            foreach (var group in _contactsGrouped)
            {
               foreach (var contact in group)
               {
                  if (contact.IsSelected)
                     DependencyService.Get<ISaveContact>().SaveContact(contact);
               }
            }
         });

         Device.BeginInvokeOnMainThread(() => HomePage.ShowMessage(notificationMessageView, "Contacts saved to your phone successfully"));
      }

      private async void DeleteCardsButton_Tapped(object sender, EventArgs e)
      {
         if (!IsAnyItemSelected())
         {
            HomePage.ShowMessage(notificationMessageView, "At least one card should be selected");
            return;
         }

         var delete = await DisplayAlert("Delete Contacts", "Are you sure you want to delete these contacts?", "Yes", "Cancel");
         if (delete)
         {
            await Task.Factory.StartNew(() =>
            {
               foreach (var group in _contactsGrouped)
               {
                  foreach (var contact in group)
                  {
                     if (contact.IsSelected)
                     {
                        Actions.DeleteAllBusinessCardFiles(contact);
                        HomePage.ContactCollection.Remove(contact);
                     }
                  }
               }
            });

            RefreshListView();
            HomePage.Instance.SaveContactList();
            HomePage.Instance.RefreshListView();

            Device.BeginInvokeOnMainThread(() =>
            {
               HomePage.ShowMessage(notificationMessageView, "Contacts deleted successfully");
               UpdatePageTitle();
            });
         }
      }

      private async void RemoveFromGroupButton_Tapped(object sender, EventArgs e)
      {
         if (!IsAnyItemSelected())
         {
            HomePage.ShowMessage(notificationMessageView, "At least one card should be selected");
            return;
         }

         await Task.Factory.StartNew(() =>
         {
            foreach (var group in _contactsGrouped)
            {
               foreach (var contact in group)
               {
                  if (contact.IsSelected)
                  {
                     contact.Group = null;
                  }
               }
            }
         });

         RefreshListView();
         HomePage.Instance.SaveContactList();

         Device.BeginInvokeOnMainThread(() =>
         {
            HomePage.ShowMessage(notificationMessageView, "Contacts removed from this group.");
            UpdatePageTitle();
         });
      }
   }
}
