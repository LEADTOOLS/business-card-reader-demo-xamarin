// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using BCReaderDemo.Models;
using Contacts;
using DataService;
using Foundation;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveContactImplementation))]
namespace BCReaderDemo.iOS
{
   class SaveContactImplementation : ISaveContact
   {
      private string PhoneNumberTypeToKey(PhoneType type)
      {
         if (type == PhoneType.Work || type == PhoneType.Home)
         {
            return CNLabelPhoneNumberKey.Main;
         }
         else if (type == PhoneType.HomeMobile || type == PhoneType.WorkMobile)
         {
            return CNLabelPhoneNumberKey.Mobile;
         }
         else if (type == PhoneType.HomeFax)
         {
            return CNLabelPhoneNumberKey.HomeFax;
         }
         else if (type == PhoneType.WorkFax)
         {
            return CNLabelPhoneNumberKey.WorkFax;
         }

         return CNLabelPhoneNumberKey.Main;
      }

      private string EmailTypeToKey(EmailType type)
      {
         if (type == EmailType.Work)
            return CNLabelKey.Work;
         else
            return CNLabelKey.Home;
      }

      public void SaveContact(ContactModel contact)
      {
         var store = new CNContactStore();
         var iosContact = new CNMutableContact();

         List<CNLabeledValue<CNPhoneNumber>> phoneNumbers = new List<CNLabeledValue<CNPhoneNumber>>();
         List<CNLabeledValue<NSString>> emails = new List<CNLabeledValue<NSString>>();
         List<CNLabeledValue<NSString>> websites = new List<CNLabeledValue<NSString>>();

         foreach (PhoneField phoneField in contact.PhoneNumbers)
         {
            phoneNumbers.Add(new CNLabeledValue<CNPhoneNumber>(PhoneNumberTypeToKey(phoneField.Type), new CNPhoneNumber(phoneField.Number)));
         }

         foreach (EmailField emailField in contact.Emails)
         {
            emails.Add(new CNLabeledValue<NSString>(EmailTypeToKey(emailField.Type), new NSString(emailField.Email)));
         }

         foreach (ContactField websiteField in contact.Websites)
         {
            websites.Add(new CNLabeledValue<NSString>(CNLabelKey.UrlAddressHomePage, new NSString(websiteField.Text)));
         }

         iosContact.PhoneNumbers = phoneNumbers.ToArray();
         iosContact.FamilyName = contact.LastName;
         iosContact.GivenName = contact.FirstName;
         iosContact.OrganizationName = !string.IsNullOrEmpty(contact.Company.Text) ? contact.Company.Text : string.Empty;
         iosContact.EmailAddresses = emails.ToArray();
         iosContact.UrlAddresses = websites.ToArray();

         if(!string.IsNullOrEmpty(contact.ProfileImage))
            iosContact.ImageData = NSData.FromFile(contact.ProfileImage);

         var saveRequest = new CNSaveRequest();
         saveRequest.AddContact(iosContact, store.DefaultContainerIdentifier);
         if (store.ExecuteSaveRequest(saveRequest, out NSError error))
         {
            Console.WriteLine("New contact saved");
         }
         else
         {
            Console.WriteLine("Save error: {0}", error);
         }
      }
   }
}