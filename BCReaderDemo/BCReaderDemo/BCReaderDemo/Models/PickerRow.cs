// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace BCReaderDemo.Models
{
   class PickerRow : ISectionRow
    {
      public string Title { get; set; }

      public int SelectedIndex { get; set; }

      public PickerItems Items { get; set; }

      public Action<object, EventArgs> SelectedIndexChanged { get; set; }
   }
}
