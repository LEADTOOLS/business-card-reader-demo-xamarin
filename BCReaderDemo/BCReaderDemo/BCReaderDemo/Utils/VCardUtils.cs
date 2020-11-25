// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using DataService;
using Leadtools;
using MixERP.Net.VCards;
using MixERP.Net.VCards.Models;
using MixERP.Net.VCards.Serializer;
using MixERP.Net.VCards.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{

   public static class VCardUtils
   {
      public const string BEGIN_KEY = "BEGIN:VCARD";
      public const string END_KEY = "END:VCARD";

      public static async Task ShareBusinessCard(ContactModel contact)
      {
         string filePath = ContactToVcf(contact);

         await DependencyService.Get<IShareContact>().Show(filePath);
      }

      public static async Task ShareBusinessCards(List<ContactModel> contacts)
      {
         string filePath = ContactsToVcf(contacts);

         await DependencyService.Get<IShareContact>().Show(filePath);
      }

      private static string ContactToVcf(ContactModel contact)
      {
         VCard vcard = VCardUtils.ContactToVCard(contact);
         string seralizedVCard = VCardSerializer.Serialize(vcard);
         string filePath = Path.Combine(HomePage.APP_DIR, String.Format("BC_{0}_{1}.vcf", contact.LastName, contact.Date.ToString("MM-dd-yyyy")));

         File.WriteAllText(filePath, seralizedVCard);

         return filePath;
      }

      private static string ContactsToVcf(List<ContactModel> contacts)
      {
         IList<VCard> vcards = VCardUtils.ContactsToVCards(contacts);
         StringBuilder builder = new StringBuilder();

         foreach (VCard vcard in vcards)
         {
            builder.Append(VCardSerializer.Serialize(vcard));
            builder.Append(Environment.NewLine);
         }

         string filePath = Path.Combine(HomePage.APP_DIR, String.Format("BC_{0}.vcf", DateTime.Now.ToString("MM-dd-yyyy HH.mm.ss")));

         File.WriteAllText(filePath, builder.ToString());

         return filePath;
      }

      public static void ExportContact(ContactModel contact, Page page)
      {
         string filePath = ContactToVcf(contact);

         Device.BeginInvokeOnMainThread(() =>
         {
            page.DisplayAlert("VCard Saved", $"VCard Saved To {filePath}", "OK");
         });
      }

      public static void ExportContacts(List<ContactModel> contacts, Page page)
      {
         string filePath = ContactsToVcf(contacts);

         Device.BeginInvokeOnMainThread(() =>
         {
            page.DisplayAlert("VCard Saved", $"VCard Saved To {filePath}", "OK");
         });
      }

      public static IList<VCard> ContactsToVCards(IList<ContactModel> contacts)
      {
         List<VCard> vcards = new List<VCard>();
         foreach(ContactModel contact in contacts)
         {
            vcards.Add(ContactToVCard(contact));
         }

         return vcards;
      }

      public static ContactModel VCardStringToContact(string vcardString, LeadRect bounds = default(LeadRect))
      {
         VCard vcard = Deserializer.GetVCard(vcardString);

         return VCardToContact(vcard, bounds);
      }

      public static ContactModel VCardToContact(VCard vcard, LeadRect bounds = default(LeadRect))
      {
         ContactModel contact = new ContactModel();

         contact.Name = new ContactField(String.Format("{0} {1}", vcard.FirstName, vcard.LastName), bounds);
         
         foreach(var phoneNumber in vcard.Telephones)
         {
            PhoneType type;

            switch(phoneNumber.Type)
            {
               case TelephoneType.Home:
                  type = PhoneType.Home;
                  break;
               case TelephoneType.Work:
                  type = PhoneType.Work;
                  break;
               case TelephoneType.Cell:
                  type = PhoneType.WorkMobile;
                  break;
               case TelephoneType.Fax:
                  type = PhoneType.WorkFax;
                  break;
               default:
                  type = PhoneType.Home;
                  break;
            }

            contact.PhoneNumbers.Add(new PhoneField(phoneNumber.Number, bounds, type));
         }

         contact.Companies.Add(new ContactField(vcard.Organization, bounds));

         contact.JobTitles.Add(new ContactField(vcard.Title, bounds));

         foreach(var email in vcard.Emails)
         {
            contact.Emails.Add(new EmailField(email.EmailAddress, bounds, Models.EmailType.Work));
         }

         contact.Websites.Add(new ContactField(vcard.Url.ToString(), bounds));

         return contact;
      }

      public static VCard ContactToVCard(ContactModel contact)
      {
         VCard vcard = new VCard();
         vcard.Version = MixERP.Net.VCards.Types.VCardVersion.V3;
         
         vcard.FormattedName = contact.Name.Text;
         vcard.FirstName = contact.FirstName;
         vcard.LastName = contact.LastName;

         List<Telephone> phoneNumbers = new List<Telephone>();
         foreach(PhoneField phoneNumber in contact.PhoneNumbers)
         {
            Telephone tel = new Telephone();
            tel.Number = phoneNumber.Number;
            switch(phoneNumber.Type)
            {
               case PhoneType.Home:
                  tel.Type = MixERP.Net.VCards.Types.TelephoneType.Home;
                  break;
               case PhoneType.HomeFax:
               case PhoneType.WorkFax:
                  tel.Type = MixERP.Net.VCards.Types.TelephoneType.Fax;
                  break;
               case PhoneType.HomeMobile:
               case PhoneType.WorkMobile:
                  tel.Type = MixERP.Net.VCards.Types.TelephoneType.Cell;
                  break;
               case PhoneType.Work:
                  tel.Type = MixERP.Net.VCards.Types.TelephoneType.Work;
                  break;
            }
            phoneNumbers.Add(tel);
         }

         vcard.Telephones = phoneNumbers;

         vcard.Organization = contact.Company.Text;
         if(contact.JobTitles != null && contact.JobTitles.Count > 0)
         {
            vcard.Title = contact.JobTitles[0].Text;
         }

         List<Email> emails = new List<Email>();

         foreach(EmailField emailField in contact.Emails)
         {
            Email email = new Email
            {
               EmailAddress = emailField.Email,
               Type = MixERP.Net.VCards.Types.EmailType.Smtp,
            };
            emails.Add(email);
         }
         vcard.Emails = emails;

         List<Address> addresses = new List<Address>();
         foreach(ContactField addressField in contact.Addresses)
         {
            Address address = new Address()
            {
               Label = addressField.Text
            };

            addresses.Add(address);
         }
         vcard.Addresses = addresses;

         vcard.Note = contact.Notes;
         vcard.Categories = new string[] { contact.Group };

         if(!String.IsNullOrEmpty(contact.ProfileImage) && File.Exists(contact.ProfileImage))
         {
            string imageContents = Convert.ToBase64String(File.ReadAllBytes(contact.ProfileImage));

            vcard.Photo = new Photo(true, "jpeg", imageContents);
         }
        
         return vcard;
      }
                 
   }
}
