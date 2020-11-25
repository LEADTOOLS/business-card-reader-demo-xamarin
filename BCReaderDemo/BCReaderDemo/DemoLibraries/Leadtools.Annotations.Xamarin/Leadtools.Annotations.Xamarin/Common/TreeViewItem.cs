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
   public sealed class TreeViewItem : StackLayout, ITreeObject
   {
      public TreeViewItem()
      {
         Init();
      }

      internal static TreeView treeView;

      private StackLayout _mainPanel;
      private Button _collapseButton;
      private Label _itemTextBlock;
      private StackLayout _itemsPanel;

      internal bool _isSelected = false;
      public bool IsSelected
      {
         get { return _isSelected; }
         set
         {
            _isSelected = value;
            UpdateControl();
         }
      }

      private bool _isCollapsed = false;
      public bool IsCollapsed
      {
         get { return _isCollapsed; }
         set
         {
            _isCollapsed = value;
            UpdateControl();
         }
      }

      private string _text = "";
      public string Text
      {
         get { return _text; }
         set
         {
            _text = value;
            UpdateControl();
         }
      }

      private List<TreeViewItem> _items = new List<TreeViewItem>();
      public List<TreeViewItem> Items
      {
         get
         {
            List<TreeViewItem> items = new List<TreeViewItem>();

            if (_itemsPanel != null)
            {
               foreach (TreeViewItem item in _itemsPanel.Children)
               {
                  items.Add(item);
               }
            }

            return items;
         }
      }

      private TreeViewItem _relativeItem = null;
      public TreeViewItem RelativeItem
      {
         get { return _relativeItem; }
      }

      private void Init()
      {
         _mainPanel = new StackLayout();
         _mainPanel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => MainPanel_Tap()) });

         this.Children.Add(_mainPanel);

         _itemsPanel = new StackLayout();
         this.Children.Add(_itemsPanel);

         StackLayout layout = new StackLayout() { Orientation = StackOrientation.Horizontal };
         _collapseButton = new Button() { WidthRequest = 30, HeightRequest = 30, FontSize = 10, BorderWidth = 0, TextColor = Color.White };
         _itemTextBlock = new Label() { FontSize = 10 };
         layout.Children.Add(_collapseButton);
         layout.Children.Add(_itemTextBlock);
         _mainPanel.Children.Add(layout);

         _collapseButton.Clicked += CollapseButton_Click;

         _itemsPanel.Children.Clear();
         foreach (TreeViewItem item in _items)
         {
            item.Margin = new Thickness(10, 0, 0, 5);
            _itemsPanel.Children.Add(item);
         }

         UpdateControl();
      }

      void MainPanel_Tap()
      {
         treeView.UnSelectAllItems();
         treeView._internalSelectedItem = this;
         _isSelected = true;

         UpdateControl();
      }

      void CollapseButton_Click(object sender, EventArgs e)
      {
         _isCollapsed = !_isCollapsed;

         treeView.UnSelectAllItems();
         treeView._internalSelectedItem = this;
         _isSelected = true;

         UpdateControl();
      }

      public void AddItem(TreeViewItem item)
      {
         item._relativeItem = this;
         _items.Add(item);
         if (_itemsPanel != null)
         {
            item.Margin = new Thickness(10, 0, 0, 5);
            _itemsPanel.Children.Add(item);
         }

         UpdateControl();
      }

      public void InsertItem(int index, TreeViewItem item)
      {
         _items.Insert(index, item);
         if (_itemsPanel != null)
         {
            item.Margin = new Thickness(10, 0, 0, 5);
            _itemsPanel.Children.Insert(index, item);
         }

         UpdateControl();
      }

      public void RemoveItem(TreeViewItem item)
      {
         _items.Remove(item);
         if (_itemsPanel != null)
         {
            _itemsPanel.Children.Remove(item);
            treeView._internalSelectedItem = this;
            _isSelected = true;
         }

         UpdateControl();
      }

      public void ClearItems()
      {
         _items.Clear();
         if (_itemsPanel != null)
         {
            _itemsPanel.Children.Clear();
            treeView._internalSelectedItem = this;
            _isSelected = true;
         }

         UpdateControl();
      }

      private void UpdateControl()
      {
         if (_itemTextBlock == null || _collapseButton == null || _itemsPanel == null)
            return;

         _itemTextBlock.Text = _text;

         if (_items.Count == 0)
         {
            _collapseButton.IsEnabled = false;
         }
         else
         {
            _collapseButton.IsEnabled = true;
         }

         if (_isSelected)
         {
            _itemTextBlock.TextColor = Color.Blue;
         }
         else
         {
            _itemTextBlock.TextColor = Color.White;
         }
         if (_isCollapsed)
         {
            _itemsPanel.IsVisible = false;
            _collapseButton.Text = "+";
         }
         else
         {
            _itemsPanel.IsVisible = true;
            _collapseButton.Text = "-";
         }

         treeView.UpdateControl();
      }

      internal void UnSelectAllItems()
      {
         if (_itemsPanel != null)
         {
            foreach (TreeViewItem item in _itemsPanel.Children)
            {
               item.IsSelected = false;
               item.UnSelectAllItems();
            }
         }
      }
   }
}
