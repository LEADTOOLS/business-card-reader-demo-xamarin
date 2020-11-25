// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{
   public class CustomTableView : TableView
   {
      public CustomTableView() {}
   }

   public class GradientContentView : ContentView
   {
      public GradientContentView() {}
   }

   public class CustomEntry : Entry
   {
      public static readonly BindableProperty ShowCautionImageProperty =
          BindableProperty.CreateAttached(
              "ShowCautionImage",
              typeof(bool),
              typeof(CustomEntry),
              false,
              BindingMode.TwoWay);

      public Regex ValidationRegex
      {
         get;
         set;
      }

      public Func<string, string> Formatter
      {
         get;
         set;
      }

      public bool ShowCautionImage
      {
         get
         {
            return (bool)GetValue(ShowCautionImageProperty);
         }
         set
         {
            SetValue(ShowCautionImageProperty, value);
         }
      }

      public CustomEntry()
      {
         this.TextChanged += CustomEntry_TextChanged;
      }

      private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
      {
         ValidateText(e.NewTextValue);

         if (Formatter != null)
         {
            this.Text = Formatter.Invoke(this.Text);
         }
      }

      public void ValidateText(string text)
      {
         if (ValidationRegex != null)
         {
            bool match = ValidationRegex.IsMatch(text);
            ShowCautionImage = !match;
         }
      }
   }

   public class ContactSavedEventArgs : EventArgs
   {
      public ContactSavedEventArgs(ContactModel contact)
      {
         Contact = contact;
      }

      public ContactModel Contact { get; set; }
   }

   public class SettingsFields : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      private string _fieldName = null;
      public string FieldName
      {
         get
         {
            return _fieldName;
         }
         set
         {
            _fieldName = value;
            NotifyPropertyChanged();
         }
      }

      private bool _fieldValue = false;
      public bool FieldValue
      {
         get
         {
            return _fieldValue;
         }
         set
         {
            _fieldValue = value;
            NotifyPropertyChanged();
         }
      }

      private bool _fieldEnable = false;
      public bool FieldEnabled
      {
         get
         {
            return _fieldEnable;
         }
         set
         {
            _fieldEnable = value;
            NotifyPropertyChanged();
         }
      }

      private Type _associatedPageType = null;
      public Type AssociatedPageType
      {
         get
         {
            return _associatedPageType;
         }
         set
         {
            _associatedPageType = value;
         }
      }

      private string _stringValue = string.Empty;
      public string StringValue
      {
         get
         {
            return _stringValue;
         }
         set
         {
            _stringValue = value;
            NotifyPropertyChanged();
         }
      }

      public SettingsFields()
      {
      }

      public SettingsFields(string fieldName, bool fieldValue, bool fieldEnabled, Type associatedPageType, string stringValue)
      {
         FieldName = fieldName;
         FieldValue = fieldValue;
         FieldEnabled = fieldEnabled;
         AssociatedPageType = associatedPageType;
         StringValue = stringValue;
      }
   }

   public class FollowUsFields
   {
      private string _fieldName = null;
      public string FieldName
      {
         get
         {
            return _fieldName;
         }
         set
         {
            _fieldName = value;
         }
      }

      private Color _fieldTextColor = Color.Black;
      public Color FieldTextColor
      {
         get
         {
            return _fieldTextColor;
         }
         set
         {
            _fieldTextColor = value;
         }
      }

      private string _fieldLink = string.Empty;
      public string FieldLink
      {
         get
         {
            return _fieldLink;
         }
         set
         {
            _fieldLink = value;
         }
      }

      private string _fieldIcon = string.Empty;
      public string FieldIcon
      {
         get
         {
            return _fieldIcon;
         }
         set
         {
            _fieldIcon = value;
         }
      }

      public FollowUsFields()
      {
      }

      public FollowUsFields(string fieldName, Color fieldTextColor, string fieldLink)
      {
         FieldName = fieldName;
         FieldTextColor = fieldTextColor;
         FieldLink = fieldLink;
      }
   }
}
