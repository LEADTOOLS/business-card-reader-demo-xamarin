// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace BCReaderDemo.Utils
{
   public enum PredefinedActions
   {
      All = 0,   // If you passed this element as predefined action then all actions will show up inside the SelectCardsPage
      Group,
      Share,
      SaveToContacts,
      Delete
   }

   public static class Actions
   {
      public static async void DialPhone(string phoneNumber, Page parentPage)
      {
#if __IOS__
         phoneNumber = Regex.Replace(phoneNumber, @"\s", "");
#endif // #if __IOS__

         try
         {
            await Launcher.OpenAsync(new Uri(String.Format("tel:{0}", phoneNumber)));
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Phone Call", ex.Message, "OK");
         }
      }

      public static async void DialPhone(ContactModel contact, Page parentPage)
      {
         bool errorOccured = false;

         try
         {
            if (contact.PhoneNumbers.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            if (contact.PhoneNumbers.Count == 1)
            {
               if (string.IsNullOrEmpty(contact.PhoneNumbers[0].Number))
               {
                  errorOccured = true;
                  goto ShowError;
               }

               string phoneNumber = contact.PhoneNumbers[0].Number;
#if __IOS__
               phoneNumber = Regex.Replace(contact.PhoneNumbers[0].Number, @"\s", "");
#endif // #if __IOS__
               await Launcher.OpenAsync(new Uri(String.Format("tel:{0}", phoneNumber)));
               return;
            }

            List<string> displayPhoneTypes = new List<string>();

            foreach (PhoneField field in contact.PhoneNumbers)
            {
               if (!string.IsNullOrEmpty(field.Number))
                  displayPhoneTypes.Add(String.Format("{0} ({1})", Helpers.PhoneTypeToString(field.Type), field.Number));
            }

            if (displayPhoneTypes.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            string action = await parentPage.DisplayActionSheet("Dial", "Cancel", null, displayPhoneTypes.ToArray());
            if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
            {
               string phoneNumber = action.Substring(action.IndexOf('(') + 1).TrimEnd(new char[] { ')' });
               IEnumerable<PhoneField> phoneFields = contact.PhoneNumbers.Where(x => x.Number.Equals(phoneNumber));

               phoneNumber = phoneFields.ElementAt(0).Number;
#if __IOS__
               phoneNumber = Regex.Replace(phoneFields.ElementAt(0).Number, @"\s", "");
#endif // #if __IOS__
               await Launcher.OpenAsync(new Uri(String.Format("tel:{0}", phoneNumber)));
            }
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Phone Call", ex.Message, "OK");
         }

      ShowError:
         if (errorOccured)
            await parentPage.DisplayAlert("Phone Call", "No phone(s) available.", "OK");
         return;
      }

      public static async void ShareContact(ContactModel contact)
      {
         await VCardUtils.ShareBusinessCard(contact);
      }

      public static async void ComposeSms(string phoneNumber = "", string messageBody = "", Page parentPage = null)
      {
#if __IOS__
         if (!string.IsNullOrWhiteSpace(phoneNumber))
            phoneNumber = Regex.Replace(phoneNumber, @"\s", "");
#endif // #if __IOS__

         try
         {
            string command = string.Format("sms:{0}{1}body={2}", (phoneNumber == null) ? string.Empty : phoneNumber, (Device.RuntimePlatform == Device.iOS) ? "&" : "?", (messageBody == null) ? string.Empty : messageBody);
            await Launcher.OpenAsync(new Uri(new Uri(command).AbsoluteUri));
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Send SMS", ex.Message, "OK");
         }
      }

      public static async void ComposeSms(ContactModel contact, Page parentPage)
      {
         bool errorOccured = false;
         try
         {
            if (contact.PhoneNumbers.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            if (contact.PhoneNumbers.Count == 1)
            {
               if (string.IsNullOrEmpty(contact.PhoneNumbers[0].Number))
               {
                  errorOccured = true;
                  goto ShowError;
               }

               string phoneNumber = contact.PhoneNumbers[0].Number;
#if __IOS__
               phoneNumber = Regex.Replace(contact.PhoneNumbers[0].Number, @"\s", "");
#endif // #if __IOS__
               await Launcher.OpenAsync(new Uri(String.Format("sms:{0}", phoneNumber)));
               return;
            }

            List<string> displayPhoneTypes = new List<string>();

            foreach (PhoneField field in contact.PhoneNumbers)
            {
               if (!string.IsNullOrEmpty(field.Number))
                  displayPhoneTypes.Add(String.Format("{0} ({1})", Helpers.PhoneTypeToString(field.Type), field.Number));
            }

            if (displayPhoneTypes.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            string action = await parentPage.DisplayActionSheet("SMS", "Cancel", null, displayPhoneTypes.ToArray());
            if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
            {
               string phoneNumber = action.Substring(action.IndexOf('(') + 1).TrimEnd(new char[] { ')' });
               IEnumerable<PhoneField> phoneFields = contact.PhoneNumbers.Where(x => x.Number.Equals(phoneNumber));

               phoneNumber = phoneFields.ElementAt(0).Number;
#if __IOS__
               phoneNumber = Regex.Replace(phoneFields.ElementAt(0).Number, @"\s", "");
#endif // #if __IOS__
               await Launcher.OpenAsync(new Uri(String.Format("sms:{0}", phoneNumber)));
            }
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Send SMS", ex.Message, "OK");
         }

      ShowError:
         if (errorOccured)
            await parentPage.DisplayAlert("Send SMS", "No phone(s) available.", "OK");
         return;
      }

      public static async void ComposeEmail(string email = "", string subject = "", string messageBody = "", Page parentPage = null)
      {
         try
         {
            string command = string.Format("mailto:{0}?subject={1}&body={2}", (email == null) ? string.Empty : email, (subject == null) ? string.Empty : subject, (messageBody == null) ? string.Empty : messageBody);
            await Launcher.OpenAsync(new Uri(new Uri(command).AbsoluteUri));
         }
         catch(Exception ex)
         {
            await parentPage.DisplayAlert("Send Email", ex.Message, "OK");
         }
      }

      public static async void ComposeEmail(ContactModel contact, Page parentPage)
      {
         bool errorOccured = false;

         try
         {
            if (contact.Emails.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            if (contact.Emails.Count == 1)
            {
               if (string.IsNullOrEmpty(contact.Emails[0].Email))
               {
                  errorOccured = true;
                  goto ShowError;
               }

               await Launcher.OpenAsync(new Uri(String.Format("mailto:{0}", contact.Emails[0].Email)));
               return;
            }

            List<string> displayMailType = new List<string>();

            foreach (EmailField field in contact.Emails)
            {
               if (!string.IsNullOrEmpty(field.Email))
                  displayMailType.Add(String.Format("{0} ({1})", Helpers.EmailTypeToString(field.Type), field.Email));
            }

            if (displayMailType.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            string action = await parentPage.DisplayActionSheet("Email To", "Cancel", null, displayMailType.ToArray());
            if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
            {
               await Launcher.OpenAsync(new Uri(String.Format("mailto:{0}", contact.Emails[displayMailType.IndexOf(action)].Email)));
            }
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Send Email", ex.Message, "OK");
         }

      ShowError:
         if (errorOccured)
            await parentPage.DisplayAlert("Send Email", "No Email address available.", "OK");
         return;
      }

      public static async void VisitWebsite(string website, Page parentPage)
      {
         try
         {
            string str = website;

            if (!str.StartsWith("http://") && !str.StartsWith("https://"))
               str = "http://" + str;

            await Launcher.OpenAsync(new Uri(str));
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Visit Website", ex.Message, "OK");
         }
      }

      public static async void VisitWebsite(ContactModel contact, Page parentPage)
      {
         bool errorOccured = false;

         try
         {
            if (contact.Websites.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            if (contact.Websites.Count == 1)
            {
               string website = contact.Websites[0].Text;
               if (string.IsNullOrEmpty(website))
               {
                  errorOccured = true;
                  goto ShowError;
               }

               if (!website.StartsWith("http://") && !website.StartsWith("https://"))
                  website = "http://" + website;

               await Launcher.OpenAsync(new Uri(website));
               return;
            }

            List<string> displayWebsites = new List<string>();

            foreach (ContactField field in contact.Websites)
            {
               if (!string.IsNullOrEmpty(field.Text))
                  displayWebsites.Add(field.Text);
            }

            if (displayWebsites.Count == 0)
            {
               errorOccured = true;
               goto ShowError;
            }

            string action = await parentPage.DisplayActionSheet("Visit Website", "Cancel", null, displayWebsites.ToArray());
            if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
            {
               string website = action;
               if (!website.StartsWith("http://") && !website.StartsWith("https://"))
                  website = "http://" + website;

               await Launcher.OpenAsync(new Uri(website));
            }
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Visit Website", ex.Message, "OK");
         }

      ShowError:
         if (errorOccured)
            await parentPage.DisplayAlert("Visit Website", "No Website(s) available.", "OK");
         return;
      }

      public static async void LocateAddressOnMap(string address, Page parentPage)
      {
         try
         {
            string command = string.Empty;
            if (Device.RuntimePlatform == Device.iOS)
               command = string.Format("http://maps.apple.com/?q={0}", WebUtility.UrlEncode(address));
            else
               command = string.Format("geo:0,0?q={0}", WebUtility.UrlEncode(address));

            await Launcher.OpenAsync(new Uri(command));
         }
         catch (Exception ex)
         {
            await parentPage.DisplayAlert("Visit Location", ex.Message, "OK");
         }
      }

      public static void DeleteAllBusinessCardFiles(ContactModel contact)
      {
         if (contact == null)
            return;

         if (!string.IsNullOrEmpty(contact.Picture) && File.Exists(contact.Picture))
            File.Delete(contact.Picture);

         if (!string.IsNullOrEmpty(contact.Thumbnail) && File.Exists(contact.Thumbnail))
            File.Delete(contact.Thumbnail);

         if (!string.IsNullOrEmpty(contact.BackImage) && File.Exists(contact.BackImage))
            File.Delete(contact.BackImage);

         if (!string.IsNullOrEmpty(contact.ProfileImage) && File.Exists(contact.ProfileImage))
            File.Delete(contact.ProfileImage);
      }

      public static void DeleteContact(ContactModel contact, Page parentPage)
      {
         Device.BeginInvokeOnMainThread(async () =>
         {
            bool accept = await parentPage.DisplayAlert("Delete Contact", "Delete this contact?", "Yes", "No");
            if (accept)
            {
               DeleteAllBusinessCardFiles(contact);

               HomePage.ContactCollection.Remove(contact);
               HomePage.ContactsGrouped = Task.Run(() => HomePage.Instance.OrganizeContacts()).Result;
               HomePage.Instance.RefreshListView();
               HomePage.Instance.SaveContactList();
            }
         });
      }
   }
}
