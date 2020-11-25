// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace BCReaderDemo.Models
{
   public class SwitchRow : ISectionRow
   {
      public string Title { get; set; }
      public bool On { get; set; }
      public Action<ToggledEventArgs> SwitchChangedAction { get; set; }      
   }
}
