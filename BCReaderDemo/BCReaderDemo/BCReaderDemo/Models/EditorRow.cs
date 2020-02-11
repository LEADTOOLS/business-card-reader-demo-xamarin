// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace BCReaderDemo.Models
{
   public class EditorRow : ISectionRow
   {
      public string Title { get; set; }
      public string Text { get; set; }

      public Keyboard Keyboard { get; set; } = Keyboard.Default;

      public Action<EventArgs> RowTappedAction { get; set; }

      public Action<EventArgs> OnFocusAction { get; set; }

      public Action<EventArgs> OnUnfocusAction { get; set; }

      public Action<object, EventArgs> TextChangedAction { get; set; }
   }
}
