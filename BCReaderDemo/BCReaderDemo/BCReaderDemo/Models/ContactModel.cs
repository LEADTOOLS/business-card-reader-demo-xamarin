// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace BCReaderDemo.Models
{
   [Serializable]
   public enum PhoneType
   {
      Home,
      Work,
      WorkMobile,
      HomeMobile,
      WorkFax,
      HomeFax,
   }

   [Serializable]
   public enum EmailType
   {
      Personal,
      Work,
   }

   [XmlRoot]
   public struct ContactField
   {
      public string Text { get; set; }
      public LeadRect Bounds { get; set; }

      public ContactField(string text, LeadRect bounds)
      {
         this.Text = text;
         this.Bounds = bounds;
      }
   }
   
   [XmlRoot]
   public struct PhoneField
   {
      public PhoneType Type { get; set; }
      public string Number { get; set; }
      public LeadRect Bounds { get; set; }

      public PhoneField (string number, LeadRect bounds, PhoneType type)
      {
         this.Number = number;
         this.Bounds = bounds;
         this.Type = type;
      }
   }

   [XmlRoot]
   public struct EmailField
   {
      public EmailType Type;
      public string Email;
      public LeadRect Bounds;

      public EmailField(string email, LeadRect bounds, EmailType type)
      {
         this.Email = email;
         this.Bounds = bounds;
         this.Type = type;
      }
   }
   
   [XmlRoot]
   public class ContactModel : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public ContactField Name { get; set; }
      
      private List<ContactField> _companies = new List<ContactField>();
      private List<ContactField> _jobTitles = new List<ContactField>();
      private List<ContactField> _addresses = new List<ContactField>();
      private List<ContactField> _websites = new List<ContactField>();
      private List<EmailField> _emails = new List<EmailField>();
      private List<PhoneField> _phoneNumbers = new List<PhoneField>();

      private bool _isSelected = false;
      private bool _showQuickActions = false;
      private string _icon = "Icons/radio-unselected.svg";

      public List<ContactField> Companies
      {
         get { return _companies; }
         set { _companies = new List<ContactField>(value); }
      }
      
      public List<ContactField> JobTitles
      {
         get { return _jobTitles; }
         set { _jobTitles = new List<ContactField>(value); }
      }
            
      public List<ContactField> Addresses
      {
         get { return _addresses; }
         set { _addresses = new List<ContactField>(value); }
      }
            
      public List<ContactField> Websites
      {
         get { return _websites; }
         set { _websites = new List<ContactField>(value); }
      }

      public List<EmailField> Emails
      {
         get { return _emails; }
         set { _emails = new List<EmailField>(value); }
      }
      
      public List<PhoneField> PhoneNumbers
      {
         get { return _phoneNumbers; }
         set { _phoneNumbers = new List<PhoneField>(value); }
      }

      public string Notes { get; set; }

      public string Event { get; set; }
      public string Group { get; set; }
      public string Referral { get; set; }

      public string Reminder { get; set; }
      public string Picture { get; set; }
      public string BackImage { get; set; }
      public int ImageWidth { get; set; }
      public int ImageHeight { get; set; }
      public string Thumbnail { get; set; }
      public DateTime Date { get; set; }

      public ContactField QRCode { get; set; }

      public string ProfileImage { get; set; }

      public bool FocusDisabled { get; set; }


      [XmlIgnore]
      public string NameSort
      {
         get
         {
            if (string.IsNullOrWhiteSpace(Name.Text) || Name.Text.Length == 0)
               return "?";

            return Name.Text[0].ToString().ToUpper();
         }
      }

      [XmlIgnore]
      public string CompanySort
      {
         get
         {
            if (string.IsNullOrWhiteSpace(Company.Text) || Company.Text.Length == 0)
               return "?";

            return Company.Text[0].ToString().ToUpper();
         }
      }

      [XmlIgnore]
      public string EmailSort
      {
         get
         {
            if (Emails.Count == 0 || string.IsNullOrWhiteSpace(Emails[0].Email) || Emails[0].Email.Length == 0)
               return "?";

            return Emails[0].Email[0].ToString().ToUpper();
         }
      }

      [XmlIgnore]
      public string DateSort
      {
         get
         {
            return Date.ToString("MM/dd/yyyy");
         }
      }

      [XmlIgnore]
      public ContactField Company
      {
         get
         {
            if (_companies.Count > 0)
               return _companies[0];
            else
               return new ContactField();
         }
      }

      [XmlIgnore]
      public string FirstName
      {
         get
         {
            if (!String.IsNullOrEmpty(Name.Text))
            {
               return Name.Text.Split().First();
            }

            return "";
         }
      }

      [XmlIgnore]
      public string LastName
      {
         get
         {
            if (!String.IsNullOrEmpty(Name.Text) && Name.Text.Split().Length > 1)
            {
               return Name.Text.Split().Last();
            }

            return "";
         }
      }

      [XmlIgnore]
      public bool IsSelected
      {
         get => _isSelected;
         set
         {
            _isSelected = value;
            if (_isSelected)
               Icon = "Icons/radio-selected.svg";
            else
               Icon = "Icons/radio-unselected.svg";
            NotifyPropertyChanged();
         }
      }

      [XmlIgnore]
      public string Icon
      {
         get => _icon;
         set
         {
            _icon = value;
            NotifyPropertyChanged();
         }
      }

      [XmlIgnore]
      public bool ShowQuickActions
      {
         get => _showQuickActions;
         set
         {
            _showQuickActions = value;
            NotifyPropertyChanged();
         }
      }

      public ContactModel Clone()
      {
         ContactModel temp = new ContactModel();

         temp.Name            = this.Name;
         temp.PhoneNumbers    = this.PhoneNumbers;
         temp.Emails          = this.Emails;
         temp.Websites        = this.Websites;
         temp.Companies       = this.Companies;
         temp.JobTitles       = this.JobTitles;
         temp.Addresses       = this.Addresses;

         temp.Notes           = this.Notes;
         temp.Event           = this.Event;
         temp.Group           = this.Group;
         temp.Referral        = this.Referral;
         temp.Reminder        = this.Reminder;
         temp.QRCode          = this.QRCode;

         temp.Date            = this.Date;
         temp.Picture         = this.Picture;
         temp.Thumbnail       = this.Thumbnail;
         temp.ProfileImage    = this.ProfileImage;
         temp.BackImage       = this.BackImage;
         temp.ImageWidth      = this.ImageWidth;
         temp.ImageHeight     = this.ImageHeight;
         temp.FocusDisabled   = this.FocusDisabled;
                  
         return temp;
      }
   }
}
