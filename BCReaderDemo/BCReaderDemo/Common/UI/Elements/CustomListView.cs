// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomListView : ListView
   {
      public CustomListView()
      {
         // Configure default options
         BackgroundColor = Color.Transparent;

         if(Device.RuntimePlatform != Device.UWP)
            HasUnevenRows = true; // Setting ListView.HasUnevenRows to true for UWP throws Error HRESULT E_FAIL has been returned from a call to a COM component for some reason, so dont call it for UWP.
         SelectionMode = ListViewSelectionMode.None;
      }

      #region Public properties

      public static readonly BindableProperty ShowSelectionProperty = BindableProperty.Create(
         propertyName: nameof(ShowSelection),
         returnType: typeof(bool),
         declaringType: typeof(CustomListView),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool ShowSelection { get => GetValue(ShowSelectionProperty) is bool value ? value : false; set => SetValue(ShowSelectionProperty, value); }

      public static readonly BindableProperty IsLongPressEnabledProperty = BindableProperty.Create(
         propertyName: nameof(IsLongPressEnabled),
         returnType: typeof(bool),
         declaringType: typeof(CustomListView),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool IsLongPressEnabled { get => GetValue(IsLongPressEnabledProperty) is bool value ? value : false; set => SetValue(IsLongPressEnabledProperty, value); }

      public static readonly BindableProperty IsDragDropEnabledProperty = BindableProperty.Create(
         propertyName: nameof(IsDragDropEnabled),
         returnType: typeof(bool),
         declaringType: typeof(CustomListView),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool IsDragDropEnabled { get => GetValue(IsDragDropEnabledProperty) is bool value ? value : false; set => SetValue(IsDragDropEnabledProperty, value); }

      public static readonly BindableProperty CanDragItemsProperty = BindableProperty.Create(
         propertyName: nameof(CanDragItems),
         returnType: typeof(bool),
         declaringType: typeof(CustomListView),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool CanDragItems { get => GetValue(CanDragItemsProperty) is bool value ? value : true; set => SetValue(CanDragItemsProperty, value); }

      #endregion

      #region Public Events

      public event EventHandler<LongPressedEventArgs> ItemLongPressed;
      public event EventHandler Clicked;
      public event EventHandler<object> DropCompleted;

      #endregion

      #region Methods

      internal void OnItemLongPressed(LongPressedEventArgs e)
      {
         ItemLongPressed?.Invoke(this, e);
      }

      internal void OnClicked()
      {
         Clicked?.Invoke(this, null);
      }

      internal void OnDropCompleted(object itemDataObject)
      {
         DropCompleted?.Invoke(this, itemDataObject);
      }

      #endregion
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class LongPressedEventArgs : EventArgs
   {
      public LongPressedEventArgs(object item, int section, int row)
      {
         Item = item;
         Section = section;
         Row = row;
      }

      public object Item { get; set; }
      public int Section { get; set; }
      public int Row { get; set; }
   }
}