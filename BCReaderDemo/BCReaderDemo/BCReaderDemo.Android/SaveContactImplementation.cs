// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Content;
using Android.Provider;
using BCReaderDemo.Droid;
using BCReaderDemo.Models;
using DataService;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveContactImplementation))]
namespace BCReaderDemo.Droid
{
   class SaveContactImplementation : ISaveContact
   {

      private int GetPhoneDataKind(PhoneField field)
      {
         if (field.Type == PhoneType.Home)
            return (int)PhoneDataKind.Home;
         else if (field.Type == PhoneType.WorkMobile)
            return (int)PhoneDataKind.WorkMobile;
         else if (field.Type == PhoneType.HomeMobile)
            return (int)PhoneDataKind.Mobile;
         else if (field.Type == PhoneType.Work)
            return (int)PhoneDataKind.Work;
         else if (field.Type == PhoneType.WorkFax)
            return (int)PhoneDataKind.FaxWork;
         else if (field.Type == PhoneType.HomeFax)
            return (int)PhoneDataKind.FaxHome;

         return (int)PhoneDataKind.Work;
      }

      private int GetEmailDataKind(EmailField field)
      {
         if (field.Type == EmailType.Personal)
            return (int)EmailDataKind.Home;
         else if (field.Type == EmailType.Work)
            return (int)EmailDataKind.Work;

         return (int)EmailDataKind.Work;
      }

      public void SaveContact(ContactModel contact)
      {
         var activity = MainActivity.Instance;

         List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

         ops.Add(ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri)
             .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null)
             .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null)
             .Build());

         // Name
         if (contact.Name.Text != null)
         {
            ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
            .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
            .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredName.ContentItemType)
            .WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, contact.Name.Text)
            .Build());
         }

         // Phone Numbers
         foreach(PhoneField phone in contact.PhoneNumbers)
         {
            if (!String.IsNullOrEmpty(phone.Number))
            {
               ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
               .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
               .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Phone.ContentItemType)
               .WithValue(ContactsContract.CommonDataKinds.Phone.Number, phone.Number)
               .WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.TypeCustom)
               .WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Data2, GetPhoneDataKind(phone))
               .Build());
            }
         }

         // Emails
         foreach(EmailField email in contact.Emails)
         {
            if (email.Email != null)
            {
               ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                   .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                   .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Email.ContentItemType)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, email.Email)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Email.InterfaceConsts.TypeCustom)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data2, GetEmailDataKind(email))
                   .Build());
            }
         }

         // Websites
         foreach (ContactField website in contact.Websites)
         {
            if (website.Text != null)
            {
               ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                   .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                   .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Website.ContentItemType)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, website.Text)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Website.InterfaceConsts.TypeCustom)
                   .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data2, (int) WebsiteDataKind.Work)
                   .Build());
            }
         }

         // Organization
         foreach (ContactField company in contact.Companies)
         {
            if (company.Text != null)
            {
               ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                   .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                   .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Organization.ContentItemType)
                   .WithValue(ContactsContract.CommonDataKinds.Organization.Company, company.Text)
                   .WithValue(ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Data3)

                   .Build());
            }
         }

         // Job Title
         foreach (ContactField jobTitle in contact.JobTitles)
         {
            ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Organization.ContentItemType)
                .WithValue(ContactsContract.CommonDataKinds.Organization.Title, jobTitle.Text)
                .WithValue(ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Data3)

                .Build());
         }

         // Attach thumbnail
         if(System.IO.File.Exists(contact.ProfileImage))
         {
            ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
               .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
               .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Photo.ContentItemType)
               .WithValue(ContactsContract.CommonDataKinds.Photo.InterfaceConsts.Data15, System.IO.File.ReadAllBytes(contact.ProfileImage))
               .Build());
         }

         activity.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);

      }
   }
}