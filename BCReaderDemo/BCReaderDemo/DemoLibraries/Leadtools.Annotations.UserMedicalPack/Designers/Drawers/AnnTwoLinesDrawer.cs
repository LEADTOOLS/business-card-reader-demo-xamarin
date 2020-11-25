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
   public class AnnTwoLinesDrawer : AnnDrawDesigner
   {
      private LeadPointD _end = LeadPointD.Empty;
      private uint _clickCount = 0;

      protected uint ClickCount
      {
         get { return _clickCount; }
      }

      public AnnTwoLinesDrawer(IAnnAutomationControl automationControl, AnnContainer container, AnnObject annObject)
         : base(automationControl, container, annObject)
      {
      }

      public override bool OnPointerDown(AnnContainer sender, AnnPointerEventArgs e)
      {
         bool handled = base.OnPointerDown(sender, e);

         if (e.Button != AnnMouseButton.Left)
            return handled;

         _clickCount++;

         if (e.Button == AnnMouseButton.Left)
         {
            LeadPointD _begin = ClipPoint(e.Location, ClipRectangle);
            _end = _begin;

            LeadPointCollection points = TargetObject.Points;

            if (_clickCount == 1)
            {
               points.Add(_begin);
               points.Add(_end);
            }

            else if (_clickCount % 2 != 0)
            {
               points.Add(_begin);
               points.Add(_end);
            }

            StartWorking();
            handled = true;
         }

         return handled;
      }

      public override bool OnPointerMove(AnnContainer sender, AnnPointerEventArgs e)
      {
         bool handled = false;

         if (TargetObject != null && HasStarted)
         {
            LeadPointD pt = ClipPoint(e.Location, ClipRectangle);

            if (!LeadPoint.Equals(pt, _end))
            {
               _end = pt;
               LeadPointCollection points = TargetObject.Points;

               if (points.Count > 1)
               {
                  if (_clickCount % 2 != 0)
                  {
                     points[points.Count - 1] = pt; // end point 
                  }
               }

               AnnIntersectionPointObject intersectionPointObject = TargetObject as AnnIntersectionPointObject;
               if (intersectionPointObject != null)
               {
                  intersectionPointObject.IntersectionInsideContainer = ClipRectangle.ContainsPoint(intersectionPointObject.IntersectionPoint);
               }

               Working();
            }

            handled = true;
         }

         return handled;
      }

      public override bool OnPointerUp(AnnContainer sender, AnnPointerEventArgs e)
      {
         LeadPointCollection points = TargetObject.Points;

         if (_clickCount > 3)
         {
            points.Add(_end); //we want to add  point at the end to infrom us that drawing finished
            EndWorking();
         }

         return true;
      }

   }
}
