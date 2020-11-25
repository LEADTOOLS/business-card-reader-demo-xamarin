// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using Xamarin.Forms;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Leadtools.Annotations.Xamarin
{
   public sealed class TreeView : ScrollView, ITreeObject
   {
      public event EventHandler<EventArgs> SelectedItemChanged;

      private StackLayout _mainPanel;

      internal TreeViewItem _internalSelectedItem;

      private TreeViewItem _selectedItem = null;

      public TreeViewItem SelectedItem
      {
         get { return _selectedItem; }
         set
         {
            this.UnSelectAllItems();
            this._internalSelectedItem = value;
            value.IsSelected = true;
         }
      }

      private List<TreeViewItem> _items = new List<TreeViewItem>();
      public List<TreeViewItem> Items
      {
         get
         {
            List<TreeViewItem> items = new List<TreeViewItem>();

            if (_mainPanel != null)
            {
               foreach (var item in _mainPanel.Children)
               {
                  items.Add(item as TreeViewItem);
               }
            }

            return items;
         }
      }

      public TreeView()
      {
         Init();

         GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => TreeView_Tap()) } );

         TreeViewItem.treeView = this;
      }

      void TreeView_Tap()
      {
         if (SelectedItemChanged != null)
         {
            SelectedItemChanged(this, EventArgs.Empty);
         }
      }

      public void Init()
      {
         _mainPanel = new StackLayout();
         this.Content = _mainPanel;

         foreach (TreeViewItem item in _items)
         {
            _mainPanel.Children.Add(item);
         }

         UpdateControl();
      }

      public void AddItem(TreeViewItem item)
      {
         _items.Add(item);
         if (_mainPanel != null)
         {
            _mainPanel.Children.Add(item);
         }

         UpdateControl();
      }

      public void InsertItem(int index, TreeViewItem item)
      {
         _items.Insert(index, item);
         if (_mainPanel != null)
         {
            _mainPanel.Children.Insert(index, item);
         }

         UpdateControl();
      }

      public void RemoveItem(TreeViewItem item)
      {
         _items.Remove(item);
         if (_mainPanel != null)
         {
            _mainPanel.Children.Remove(item);
            if (_items.Count == 0)
               _internalSelectedItem = null;
            else
               _internalSelectedItem = _items[0];
         }

         UpdateControl();
      }

      public void ClearItems()
      {
         _items.Clear();

         if (_mainPanel != null)
         {
            _mainPanel.Children.Clear();
         }
         _internalSelectedItem = null;

         UpdateControl();
      }

      internal void UpdateControl()
      {
         if (_mainPanel == null)
            return;

         if (_items.Count == 0)
         {
            _internalSelectedItem = null;
         }

         if (_selectedItem != _internalSelectedItem)
         {
            _selectedItem = _internalSelectedItem;
            if (SelectedItemChanged != null)
            {
               SelectedItemChanged(this, EventArgs.Empty);
            }
         }
      }

      internal void UnSelectAllItems()
      {
         foreach (TreeViewItem item in _mainPanel.Children)
         {
            item.IsSelected = false;
            item.UnSelectAllItems();
         }
      }
   }

   public interface ITreeObject
   {
      List<TreeViewItem> Items
      {
         get;
      }
      void AddItem(TreeViewItem item);
      void InsertItem(int index, TreeViewItem item);
      void RemoveItem(TreeViewItem item);
      void ClearItems();
   }
}
