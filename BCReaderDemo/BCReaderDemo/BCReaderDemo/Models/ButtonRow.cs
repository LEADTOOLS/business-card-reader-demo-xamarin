// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace BCReaderDemo.Models
{
   class ButtonRow : ISectionRow
    {
      public string Text { get; set; }

      public Action<EventArgs> RowTappedAction { get; set; }
   }
}
