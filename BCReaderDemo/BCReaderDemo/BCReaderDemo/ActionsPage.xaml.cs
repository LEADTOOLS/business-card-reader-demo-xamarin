// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;

namespace BCReaderDemo
{
   public partial class ActionsPage : Rg.Plugins.Popup.Pages.PopupPage
   {
      const int DATE_SORT_INDEX = 0;
      const int NAME_SORT_INDEX = 1;
      const int COMPANY_SORT_INDEX = 2;
      const int EMAIL_SORT_INDEX = 3;

      public ActionsPage()
      {
         this.InitializeComponent();

         string[] defaultSortIcons = GetDefaultSortIcons();
         dateImage.Source = defaultSortIcons[DATE_SORT_INDEX];
         nameImage.Source = defaultSortIcons[NAME_SORT_INDEX];
         companyImage.Source = defaultSortIcons[COMPANY_SORT_INDEX];
         mailImage.Source = defaultSortIcons[EMAIL_SORT_INDEX];

         MainLayout.Margin = new Thickness(MainLayout.Margin.Left, MainLayout.Margin.Top, MainLayout.Margin.Right, App.DisplayScreenHeight / 8);
      }

      private string[] GetDefaultSortIcons()
      {
         string dateSortIcon = "date_inactive.png";
         string nameSortIcon = "name_inactive.png";
         string companySortIcon = "company_inactive.png";
         string emailSortIcon = "email_inactive.png";
         if (HomePage.CurrentAppData != null)
         {
            switch (HomePage.CurrentAppData.SortBy)
            {
               case Utils.SortBy.Date:
                  dateSortIcon = "date_active.png";
                  break;

               case Utils.SortBy.Name:
                  nameSortIcon = "name_active.png";
                  break;

               case Utils.SortBy.Company:
                  companySortIcon = "company_active.png";
                  break;

               case Utils.SortBy.Email:
                  emailSortIcon = "email_active.png";
                  break;
            }
         }

         return new string[] { dateSortIcon, nameSortIcon, companySortIcon, emailSortIcon };
      }

      private void DeactivateSortImages(Image image)
      {
         Grid grid = image.Parent as Grid;
         foreach (Image child in grid.Children)
         {
            child.Source = child.StyleId + ".png";
         }
      }

      private async void SortButton_Tapped(object sender, EventArgs e)
      {
         Image sourceImage = sender as Image;

         // StyleId represents a unique identifier to identify the tapped image and it also holds the inactive image source name
         // that we use to deselect all images before marking the new one as selected.
         if (sourceImage.StyleId.Contains("date"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Date) return;

            DeactivateSortImages(sourceImage);
            sourceImage.Source = "date_active.png";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Date;
         }
         else if (sourceImage.StyleId.Contains("name"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Name) return;

            DeactivateSortImages(sourceImage);
            sourceImage.Source = "name_active.png";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Name;
         }
         else if (sourceImage.StyleId.Contains("company"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Company) return;

            DeactivateSortImages(sourceImage);
            sourceImage.Source = "company_active.png";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Company;
         }
         else if (sourceImage.StyleId.Contains("email"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Email) return;

            DeactivateSortImages(sourceImage);
            sourceImage.Source = "email_active.png";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Email;
         }

         await PopupNavigation.Instance.PopAsync();
      }

      private async void ActionButton_Tapped(object sender, EventArgs e)
      {
         Image source = sender as Image;
         Page page = null;

         await PopupNavigation.Instance.PopAsync();

         switch (source.StyleId)
         {
            case "group":
               page = new SelectCardsPage(Utils.PredefinedActions.Group, null, null, false, null);
               break;

            case "share":
               page = new SelectCardsPage(Utils.PredefinedActions.Share, null, null, false, null);
               break;

            case "save":
               page = new SelectCardsPage(Utils.PredefinedActions.SaveToContacts, null, null, false, null);
               break;

            case "delete":
               page = new SelectCardsPage(Utils.PredefinedActions.Delete, null, null, false, null);
               break;
         }

         await HomePage.Instance.Navigation.PushAsync(page);
      }
   }
}
