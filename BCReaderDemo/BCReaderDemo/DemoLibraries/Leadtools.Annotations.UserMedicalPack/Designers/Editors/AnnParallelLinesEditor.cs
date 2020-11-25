// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Designers;
using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnParallelLinesEditor : AnnEditDesigner
   {
      public AnnParallelLinesEditor(IAnnAutomationControl automationControl, AnnContainer container, AnnParallelLinesObject annParallelLinesObject)
         : base(automationControl, container, annParallelLinesObject)
      {

      }

      protected override void MoveThumb(int thumbIndex, LeadPointD offset)
      {
         int thumbsCount = GetThumbLocations().Length;
         int targetPointsCount = TargetObject.Points.Count;

         LeadPointD firstPoint = TargetObject.Points[0];
         LeadPointD lastPoint = TargetObject.Points[TargetObject.Points.Count - 1];
         bool isFlipped = firstPoint.Y > lastPoint.Y;


         int mythumbIndex = thumbIndex;

         List<LeadPointD> points = new List<LeadPointD>();
         foreach (var point in TargetObject.Points)
            points.Add(point);

         //if the object is flipped then work with the points reversely 
         if (isFlipped)
         {
            //Reverse the points
            points.Reverse();
            //Also reverse the thumIndex
            mythumbIndex = Math.Abs(thumbIndex - (thumbsCount - 1));
         }

         int before = Math.Max(0, (mythumbIndex - 1));
         int after = Math.Min(mythumbIndex + 1, thumbsCount - 1);

         double offsetY = offset.Y;

         LeadPointD beforePoint = points[before * 2];
         LeadPointD current = points[mythumbIndex * 2];
         LeadPointD updated = AnnTransformer.TranslatePoint(points[mythumbIndex * 2], 0, offsetY);
         LeadPointD afterPoint = points[after * 2];

         bool x = LeadPointD.Equals(current, beforePoint) ? true : (updated.Y > (beforePoint.Y + 48));
         bool y = LeadPointD.Equals(current, afterPoint) ? true : updated.Y < (afterPoint.Y - 48);

         if (x && y)
         {
            points[mythumbIndex * 2] = updated;
            points[mythumbIndex * 2 + 1] = AnnTransformer.TranslatePoint(points[mythumbIndex * 2 + 1], 0, offsetY);

            if (isFlipped)
               points.Reverse();

            for (int i = 0; i < points.Count; i++)
               TargetObject.Points[i] = SnapPointToGrid(points[i], false);

         }
      }

      public override LeadPointD[] GetThumbLocations()
      {
         LeadPointD[] locations = null;

         LeadPointD[] points = TargetObject.Points.ToArray();
         int pointsCount = points.Length / 2;
         LeadPointD[] pts = new LeadPointD[pointsCount];

         for (int i = 0; i < pointsCount; ++i)
         {
            int index = i * 2;
            LeadPointD start = points[index];
            LeadPointD end = points[index + 1];
            pts[i] = LeadPointD.Create((start.X + end.X) / 2, (start.Y + end.Y) / 2);
         }

         locations = pts;

         return locations;
      }

      protected override void Move(double offsetX, double offsetY)
      {
         base.Move(offsetX, offsetY);
         SnapObjectToGrid(TargetObject, false);
      }
   }
}
