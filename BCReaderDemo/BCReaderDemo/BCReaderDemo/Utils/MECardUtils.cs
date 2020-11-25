// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using Leadtools;

namespace BCReaderDemo.Utils
{
   public static class MECardKeys
   {
      public const string MECARD_KEY = "MECARD:";
      public const string NAME_KEY = "N:";
      public const string PHONE_KEY = "TEL:";
      public const string EMAIL_KEY = "EMAIL:";
      public const string COPMANY_KEY = "ORG:";
      public const string TITLE_KEY = "TITLE:";
      public const string WEBSITE_KEY = "URL:";
      public const string ADDRESS_KEY = "ADR:";
      public const string NOTE_KEY = "NOTE:";
   }

   public static class MECardUtils
   {
      public static ContactModel ParseMECardToContact(string meCardString, LeadRect bounds = new LeadRect())
      {
         ContactModel contact = new ContactModel();
         
         if (!meCardString.StartsWith(MECardKeys.MECARD_KEY) && meCardString.Contains(MECardKeys.MECARD_KEY))
            meCardString = meCardString.Substring(meCardString.IndexOf(MECardKeys.MECARD_KEY));

         string meCardStr = meCardString.Substring(MECardKeys.MECARD_KEY.Length);

         string[] meCardTokens = meCardStr.Split(';');

         foreach (string meCardToken in meCardTokens)
         {
            if (meCardToken.StartsWith(MECardKeys.NAME_KEY))
            {
               string name = meCardToken.Substring(MECardKeys.NAME_KEY.Length);
               if (name.Contains(","))
                  name = name.Substring(name.IndexOf(',') + 1) + " " + name.Substring(0, name.IndexOf(','));

               contact.Name = new ContactField(name, bounds);
            }
            else if (meCardToken.StartsWith(MECardKeys.PHONE_KEY))
            {
               string phoneNumber = meCardToken.Substring(MECardKeys.PHONE_KEY.Length);

               contact.PhoneNumbers.Add(new PhoneField(phoneNumber, bounds, PhoneType.Work));
            }
            else if (meCardToken.StartsWith(MECardKeys.EMAIL_KEY))
            {
               string email = meCardToken.Substring(MECardKeys.EMAIL_KEY.Length);

               contact.Emails.Add(new EmailField(email, bounds, EmailType.Work));
            }
            else if (meCardToken.StartsWith(MECardKeys.WEBSITE_KEY))
            {
               string website = meCardToken.Substring(MECardKeys.WEBSITE_KEY.Length);

               contact.Websites.Add(new ContactField(website, bounds));
            }
            else if (meCardToken.StartsWith(MECardKeys.COPMANY_KEY))
            {
               string company = meCardToken.Substring(MECardKeys.COPMANY_KEY.Length);

               contact.Companies.Add(new ContactField(company, bounds));
            }
            else if (meCardToken.StartsWith(MECardKeys.TITLE_KEY))
            {
               string title = meCardToken.Substring(MECardKeys.TITLE_KEY.Length);

               contact.JobTitles.Add(new ContactField(title, bounds));
            }
            else if (meCardToken.StartsWith(MECardKeys.ADDRESS_KEY))
            {
               string address = meCardToken.Substring(MECardKeys.ADDRESS_KEY.Length);

               contact.Addresses.Add(new ContactField(address, bounds));
            }
            else if (meCardToken.StartsWith(MECardKeys.NOTE_KEY))
            {
               string note = meCardToken.Substring(MECardKeys.NOTE_KEY.Length);

               contact.Notes = note;
            }
         }
         
         return contact;
      }

   }
}
