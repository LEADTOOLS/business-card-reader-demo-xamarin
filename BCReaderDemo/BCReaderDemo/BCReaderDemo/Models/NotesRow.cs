// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace BCReaderDemo.Models
{
   public class NotesRow : ISectionRow
   {
      public string Title { get; set; }
      public string Text { get; set; }

      public Action<object, EventArgs> TextChangedAction { get; set; }

      public Action<EventArgs> OnFocusAction { get; set; }

      public Action<EventArgs> OnUnfocusAction { get; set; }
   }
}
