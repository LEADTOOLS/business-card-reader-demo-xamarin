// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace BCReaderDemo.Models
{
   [XmlRoot]
   public enum PickerItemChangeType
   {
      Added,
      Deleted,
      Replaced,
      Cleared
   };

   public class PickerItemsChangedEventArgs : EventArgs
   {
      public readonly string ChangedItem;
      public readonly PickerItemChangeType ChangeType;
      public readonly string ReplacedWith;

      public PickerItemsChangedEventArgs(PickerItemChangeType change, string item,
          string replacement)
      {
         ChangeType = change;
         ChangedItem = item;
         ReplacedWith = replacement;
      }
   }

   [XmlRoot]
   public class PickerItems
   {
      public event EventHandler<PickerItemsChangedEventArgs> Changed;

      private const string NONE = "None";
      private const string ADD_NEW = "Add New";

      private Collection<string> _items;

      public Collection<string> Items
      {
         get { return _items; }
         set { _items = new Collection<string>(value); }
      }

      public List<string> DisplayItems
      {
         get
         {
            List<string> list = new List<string>();
            list.Add(NONE);
            foreach (string item in _items)
               list.Add(item);
            list.Add(ADD_NEW);

            return list;
         }
      }

      public PickerItems()
      {
         _items = new Collection<string>();
      }

      public void AddItem(string item)
      {
         if (_items.Contains(item))
            return;

         _items.Add(item);
         Changed?.Invoke(this, new PickerItemsChangedEventArgs(PickerItemChangeType.Added, item, null));
      }

      public void DeleteItem(string item)
      {
         _items.Remove(item);
         Changed?.Invoke(this, new PickerItemsChangedEventArgs(PickerItemChangeType.Deleted, item, null));
      }

      public void SetItem(string oldItemName, string newValue)
      {
         _items[_items.IndexOf(oldItemName)] = newValue;
         Changed?.Invoke(this, new PickerItemsChangedEventArgs(PickerItemChangeType.Replaced, oldItemName, newValue));
      }

      public void ClearItems()
      {
         _items.Clear();
         Changed?.Invoke(this, new PickerItemsChangedEventArgs(PickerItemChangeType.Cleared, null, null));
      }
   }
}
