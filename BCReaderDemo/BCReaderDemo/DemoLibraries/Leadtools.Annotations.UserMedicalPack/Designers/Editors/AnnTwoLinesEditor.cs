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
   public class AnnTwoLinesEditer : AnnEditDesigner
   {
      public AnnTwoLinesEditer(IAnnAutomationControl automationControl, AnnContainer container, AnnObject annMidlineObject)
         : base(automationControl, container, annMidlineObject)
      {
      }

      public override LeadPointD[] GetThumbLocations()
      {
         LeadPointCollection pointsCollection = TargetObject.Points;
         if (pointsCollection.Count == 2) // Snap Point Object
            return pointsCollection.ToArray();
         else // Midline Object
         {
            return  new LeadPointD[] { pointsCollection[0], pointsCollection[1], pointsCollection[2], pointsCollection[3] };
         }
      }

      protected override void MoveThumb(int thumbIndex, LeadPointD offset)
      {
         AnnObject targetObject = TargetObject;
         LeadPointD point = targetObject.Points[thumbIndex];
         targetObject.Points[thumbIndex] = ClipPoint(AnnTransformer.TranslatePoint(point, offset.X, offset.Y), ClipRectangle);

         AnnIntersectionPointObject intersectionPointObject = targetObject as AnnIntersectionPointObject;
         if (intersectionPointObject != null)
         {
            intersectionPointObject.IntersectionInsideContainer = ClipRectangle.ContainsPoint(intersectionPointObject.IntersectionPoint);
         }

         base.MoveThumb(thumbIndex, offset);
      }
   }
}
