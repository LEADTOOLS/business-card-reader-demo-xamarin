// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace BCReaderDemo.Utils
{
   [XmlRoot]
   public enum SortBy
   {
      Name,
      Company,
      Date,
      Email
   }

   [XmlRoot]
   public enum CameraQuality
   {
      Medium,
      High
   }

   [XmlRoot]
   public class OptionalFields : INotifyPropertyChanged
   {
      private bool _email = true;
      private bool _company = true;
      private bool _jobTitle = true;
      private bool _website = true;
      private bool _address = true;

      private bool _group = true;
      private bool _note = true;
      private bool _event = true;
      private bool _referral = true;
      private bool _reminder = true;

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public bool Email
      {
         get { return _email; }
         set
         {
            _email = value;
            NotifyPropertyChanged();
         }
      }
      public bool Company
      {
         get { return _company; }
         set
         {
            _company = value;
            NotifyPropertyChanged();
         }
      }
      public bool JobTitle
      {
         get { return _jobTitle; }
         set
         {
            _jobTitle = value;
            NotifyPropertyChanged();
         }
      }
      public bool Website
      {
         get { return _website; }
         set
         {
            _website = value;
            NotifyPropertyChanged();
         }
      }
      public bool Address
      {
         get { return _address; }
         set
         {
            _address = value;
            NotifyPropertyChanged();
         }
      }

      public bool Group
      {
         get { return _group; }
         set
         {
            _group = value;
            NotifyPropertyChanged();
         }
      }

      public bool Note
      {
         get { return _note; }
         set
         {
            _note = value;
            NotifyPropertyChanged();
         }
      }

      public bool Event
      {
         get { return _event; }
         set
         {
            _event = value;
            NotifyPropertyChanged();
         }
      }

      public bool Reminder
      {
         get { return _reminder; }
         set
         {
            _reminder = value;
            NotifyPropertyChanged();
         }
      }

      public bool Referral
      {
         get { return _referral; }
         set
         {
            _referral = value;
            NotifyPropertyChanged();
         }
      }
   }

   [XmlRoot]
   public class AppData : INotifyPropertyChanged
   {
      public static readonly string DATA_FILE_NAME = "AppData.xml";

      private SortBy _sortBy = SortBy.Name;
      private PickerItems _groupItems = new PickerItems();
      private PickerItems _eventItems = new PickerItems();
      private PickerItems _referralItems = new PickerItems();
      private bool _autoSaveToContacts = false;
      private bool _enableAutoCapture = true;
      private OptionalFields _optionalFields = new OptionalFields();
      private int _selectedGroupIndex = 0;
      private bool _recognizeQRCode = false;
      private bool _enableBarcodeDoublePass = true;
      private bool _enableBarcodePreprocessing = true;
      private CameraQuality _cameraQuality = CameraQuality.Medium;

      private bool _dataModified = false;

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public SortBy SortBy
      {
         get => _sortBy;
         set
         {
            _sortBy = value;
            DataModified = true;
            NotifyPropertyChanged();
         }
      }

      public CameraQuality CameraQuality
      {
         get => _cameraQuality;
         set
         {
            _cameraQuality = value;
            DataModified = true;
            NotifyPropertyChanged();
         }
      }

      public PickerItems GroupPickerItems
      {
         get => _groupItems;
         set
         {
            _groupItems = value;
            DataModified = true;
         }
      }
      public PickerItems EventPickerItems
      {
         get => _eventItems;
         set
         {
            _eventItems = value;
            DataModified = true;
         }
      }
      public PickerItems ReferralPickerItems
      {
         get => _referralItems;
         set
         {
            _referralItems = value;
            DataModified = true;
         }
      }
      public bool AutoSaveToContacts
      {
         get => _autoSaveToContacts;
         set
         {
            _autoSaveToContacts = value;
            DataModified = true;
         }
      }
      public OptionalFields OptionalFields
      {
         get => _optionalFields;
         set
         {
            _optionalFields = value;
            DataModified = true;
         }
      }
      public int SelectedGroupIndex
      {
         get => _selectedGroupIndex;
         set
         {
            _selectedGroupIndex = value;
            DataModified = true;
         }
      }

      [XmlIgnore]
      public bool DataModified
      {
         get { return _dataModified; }
         set
         {
            if (value)
            {
               _dataModified = value;
               Save(Path.Combine(HomePage.APP_DIR, DATA_FILE_NAME));
            }

         }
      }

      public bool RecognizeQRCode
      {
         get { return _recognizeQRCode; }
         set
         {
            _recognizeQRCode = value;
            DataModified = true;
            NotifyPropertyChanged();
         }
      }
      public bool EnableBarcodeDoublePass
      {
         get { return _enableBarcodeDoublePass; }
         set
         {
            _enableBarcodeDoublePass = value;
            DataModified = true;
            NotifyPropertyChanged();
         }
      }
      public bool EnableBarcodePreprocessing
      {
         get { return _enableBarcodePreprocessing; }
         set
         {
            _enableBarcodePreprocessing = value;
            DataModified = true;
            NotifyPropertyChanged();
         }
      }

      public bool EnableCameraAutoCapture
      {
         get => _enableAutoCapture;
         set
         {
            _enableAutoCapture = value;
            DataModified = true;
         }
      }

      public AppData()
      {
         _groupItems.Changed += _items_Changed;
         _eventItems.Changed += _items_Changed;
         _referralItems.Changed += _items_Changed;
         _optionalFields.PropertyChanged += _items_Changed;
      }

      public void Load(string fileName)
      {
         try
         {
            if (File.Exists(fileName))
            {
               XmlSerializer serializer = new XmlSerializer(typeof(AppData));
               using (StreamReader reader = new StreamReader(fileName))
               {
                  AppData appData = (AppData)serializer.Deserialize(reader);

                  this._sortBy = appData.SortBy;
                  this._groupItems = appData.GroupPickerItems;
                  this._eventItems = appData.EventPickerItems;
                  this._referralItems = appData.ReferralPickerItems;
                  this._autoSaveToContacts = appData.AutoSaveToContacts;
                  this._enableAutoCapture = appData._enableAutoCapture;
                  this._optionalFields = appData.OptionalFields;
                  this._selectedGroupIndex = appData.SelectedGroupIndex;
                  this._recognizeQRCode = appData.RecognizeQRCode;
                  this._enableBarcodeDoublePass = appData.EnableBarcodeDoublePass;
                  this._enableBarcodePreprocessing = appData.EnableBarcodePreprocessing;
                  this._cameraQuality = appData.CameraQuality;
               }

               _groupItems.Changed += _items_Changed;
               _eventItems.Changed += _items_Changed;
               _referralItems.Changed += _items_Changed;
               _optionalFields.PropertyChanged += _items_Changed;
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private void _items_Changed(object sender, EventArgs e)
      {
         DataModified = true;
      }

      public void Save(string fileName)
      {
         if (HomePage.IsLoading) return;

         try
         {
            XmlSerializer serializer = new XmlSerializer(typeof(AppData));

            using (StreamWriter writer = new StreamWriter(fileName))
            {
               serializer.Serialize(writer, this);
               DataModified = false;
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }
   }
}
