// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************

namespace BCReaderDemo.Models
{
   public interface ISectionRow
   {

   }

   public class DividerRow : ISectionRow
   {
      public bool VisibleDivider { get; set; } = true;
      public double HeightRequest { get; set; } = 0;
   }

}
