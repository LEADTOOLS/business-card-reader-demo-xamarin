// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos;
using Leadtools.Demos.UI.Elements;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;

namespace BCReaderDemo
{
   public partial class ActionsPage : PopupPage
   {
      const int DATE_SORT_INDEX = 0;
      const int NAME_SORT_INDEX = 1;
      const int COMPANY_SORT_INDEX = 2;
      const int EMAIL_SORT_INDEX = 3;

      public ActionsPage()
      {
         this.InitializeComponent();

         string[] defaultSortIcons = GetDefaultSortIcons();
         dateImage.ResourceName = defaultSortIcons[DATE_SORT_INDEX];
         nameImage.ResourceName = defaultSortIcons[NAME_SORT_INDEX];
         companyImage.ResourceName = defaultSortIcons[COMPANY_SORT_INDEX];
         mailImage.ResourceName = defaultSortIcons[EMAIL_SORT_INDEX];

         MainLayout.Margin = new Thickness(MainLayout.Margin.Left, MainLayout.Margin.Top, MainLayout.Margin.Right, DemoUtilities.DisplayHeight / 8);
      }

      private string[] GetDefaultSortIcons()
      {
         string dateSortIcon = "Icons/date.svg";
         string nameSortIcon = "Icons/name.svg";
         string companySortIcon = "Icons/company.svg";
         string emailSortIcon = "Icons/email.svg";
         if (HomePage.CurrentAppData != null)
         {
            switch (HomePage.CurrentAppData.SortBy)
            {
               case Utils.SortBy.Date:
                  dateSortIcon = "Icons/date-active.svg";
                  break;

               case Utils.SortBy.Name:
                  nameSortIcon = "Icons/name-active.svg";
                  break;

               case Utils.SortBy.Company:
                  companySortIcon = "Icons/company-active.svg";
                  break;

               case Utils.SortBy.Email:
                  emailSortIcon = "Icons/email-active.svg";
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
            (child as SvgImage).ResourceName = "Icons/" + child.StyleId + ".svg";
         }
      }

      private async void SortButton_Tapped(object sender, EventArgs e)
      {
         SvgImage sourceImage = sender as SvgImage;

         // StyleId represents a unique identifier to identify the tapped image and it also holds the inactive image source name
         // that we use to deselect all images before marking the new one as selected.
         if (sourceImage.StyleId.Contains("date"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Date) return;

            DeactivateSortImages(sourceImage);
            sourceImage.ResourceName = "Icons/date-active.svg";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Date;
         }
         else if (sourceImage.StyleId.Contains("name"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Name) return;

            DeactivateSortImages(sourceImage);
            sourceImage.ResourceName = "Icons/name-active.svg";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Name;
         }
         else if (sourceImage.StyleId.Contains("company"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Company) return;

            DeactivateSortImages(sourceImage);
            sourceImage.ResourceName = "Icons/company-active.svg";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Company;
         }
         else if (sourceImage.StyleId.Contains("email"))
         {
            if (HomePage.CurrentAppData.SortBy == Utils.SortBy.Email) return;

            DeactivateSortImages(sourceImage);
            sourceImage.ResourceName = "Icons/email-active.svg";
            HomePage.CurrentAppData.SortBy = Utils.SortBy.Email;
         }

         await PopupNavigation.Instance.PopAsync();
      }

      private async void ActionButton_Tapped(object sender, EventArgs e)
      {
         Image source = sender as Image;
         PopupPage page = null;

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

         await PopupNavigation.Instance.PushAsync(page);
      }
   }
}
