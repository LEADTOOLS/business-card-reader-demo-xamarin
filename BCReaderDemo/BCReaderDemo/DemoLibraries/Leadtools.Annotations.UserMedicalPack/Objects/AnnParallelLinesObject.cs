// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnParallelLinesObject : AnnObject
   {

      public AnnParallelLinesObject()
      {
         SetId(-1001);
      }

      protected override AnnObject Create()
      {
         return new AnnParallelLinesObject();
      }

      public override bool SupportsStroke
      {
         get
         {
            return true;
         }
      }

      public override string FriendlyName
      {
         get
         {
            return "Parallel Lines";
         }
      }

      public override bool SupportsFill
      {
         get
         {
            return false;
         }
      }

      public override bool CanRotate
      {
         get
         {
            return false;
         }
      }

      public override bool HitTest(LeadPointD point, double hitTestBuffer)
      {
         bool hit = base.HitTest(point, hitTestBuffer);

         if (hit)
         {
            AnnPolylineObject line = new AnnPolylineObject();

            hit = false;

            int pointsCount = Points.Count / 2;
            for (int i = 0; i < pointsCount && !hit; ++i)
            {
               line.Points.Add(Points[2 * i]);
               line.Points.Add(Points[2 * i + 1]);
               if (line.HitTest(point, hitTestBuffer))
                  hit = true;
            }
         }

         return hit;
      }
   }
}
