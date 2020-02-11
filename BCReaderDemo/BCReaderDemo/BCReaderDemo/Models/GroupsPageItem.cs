// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BCReaderDemo.Models
{
   public class GroupsPageItem : INotifyPropertyChanged
   {
      public GroupsPageItem()
      {
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      private string _groupName = string.Empty;
      public string GroupName
      {
         get => _groupName;
         set
         {
            _groupName = value;
            NotifyPropertyChanged();
         }
      }

      private int _count = 0;
      public int Count
      {
         get => _count;
         set
         {
            _count = value;
            NotifyPropertyChanged();
         }
      }

      private bool _isSelected = false;
      public bool IsSelected
      {
         get => _isSelected;
         set
         {
            _isSelected = value;
            if (_isSelected)
               Icon = "checked.png";
            else
               Icon = "unchecked.png";
            NotifyPropertyChanged();
         }
      }

      private string _icon = "unchecked.png";
      public string Icon
      {
         get => _icon;
         set
         {
            _icon = value;
            NotifyPropertyChanged();
         }
      }

      private bool _renameGroup = false;
      public bool RenameGroup
      {
         get => _renameGroup;
         set
         {
            _renameGroup = value;
            NotifyPropertyChanged();
         }
      }

      public string TitleSort
      {
         get
         {
            if (string.IsNullOrWhiteSpace(GroupName) || GroupName.Length == 0)
               return "?";

            return GroupName[0].ToString().ToUpper();
         }
      }
   }
}
