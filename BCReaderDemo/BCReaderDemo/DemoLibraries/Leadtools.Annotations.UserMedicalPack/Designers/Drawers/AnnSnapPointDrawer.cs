// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Designers;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnSnapPointDrawer : AnnTwoLinesDrawer
   {
      public AnnSnapPointDrawer(IAnnAutomationControl automationControl, AnnContainer container, AnnMidlineObject annMidlineObject)
         : base(automationControl, container, annMidlineObject)
      {
      }

      public override bool OnPointerUp(AnnContainer sender, AnnPointerEventArgs e)
      {
         AnnMidlineObject midlineObject = TargetObject as AnnMidlineObject;
         LeadPointCollection points = midlineObject.Points;

         if (ClickCount > 1)
         {
            EndWorking();
         }

         return true;
      }
   }
}
