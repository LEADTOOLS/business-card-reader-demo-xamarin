// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Collections.Generic;

namespace BCReaderDemo.Models
{
   public class Section
   {
      public string Header { get; set; }
      public IList<ISectionRow> Rows { get; } = new List<ISectionRow>();
   }
}
