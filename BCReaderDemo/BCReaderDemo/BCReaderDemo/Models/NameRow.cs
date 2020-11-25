// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace BCReaderDemo.Models
{
    class NameRow : ISectionRow
    {
      public string Text { get; set; }
      public Action<object> AddImageAction { get; set; }
      public string ImagePath { get; set; }
      public Action<EventArgs> RowTappedAction { get; set; }
      public Action<EventArgs> OnFocusAction { get; set; }
      public Action<EventArgs> OnUnfocusAction { get; set; }
      public Action<object, EventArgs> TextChangedAction { get; set; }
   }
}
