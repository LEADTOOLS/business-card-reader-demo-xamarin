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
   public class AnnSnapPointEditor : AnnTwoLinesEditer
   {
      public AnnSnapPointEditor(IAnnAutomationControl automationControl, AnnContainer container, AnnMidlineObject annMidlineObject)
         : base(automationControl, container, annMidlineObject)
      {
      }

      public override LeadPointD[] GetThumbLocations()
      {
         LeadPointCollection pointsCollection = TargetObject.Points;
         LeadPointD[] points = new LeadPointD[] { pointsCollection[0], pointsCollection[1]};
         return points;
      }
   }
}
