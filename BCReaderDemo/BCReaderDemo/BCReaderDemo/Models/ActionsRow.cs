// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace BCReaderDemo.Models
{
   class ActionsRow : ISectionRow
   {
      public Action<object> SendMessage { get; set; }
      public Action<object> OpenDialer { get; set; }

      public Action<object> SendEmail { get; set; }
      public Action<object> VisitWebsite { get; set; }

      public Action<object> ShareContact { get; set; }
   }
}
