// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Rendering;
using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnIntersectionObjectRenderer : AnnTowLinesObjectRenderer
   {
      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         IAnnDrawEngine engine = RenderingEngine as IAnnDrawEngine;

         if (engine != null)
         {
            base.Render(mapper, annObject);

            AnnIntersectionPointObject annIntersectionPointObject = annObject as AnnIntersectionPointObject;
            if (annIntersectionPointObject != null)
            {
               int count = annIntersectionPointObject.Points.Count;
               if (count < 2)
                  return;

               LeadPointD[] leadPoints = mapper.PointsFromContainerCoordinates(annIntersectionPointObject.Points.ToArray(), annIntersectionPointObject.FixedStateOperations);

               if (annIntersectionPointObject.SupportsStroke && annIntersectionPointObject.Stroke != null)
               {
                  AnnStroke stroke = AnnStroke.Create(AnnSolidColorBrush.Create("Blue"), annIntersectionPointObject.Stroke.StrokeThickness);
                  stroke.StrokeDashArray = new double[] { 3, 1, 1, 1, 1, 1 }; // DashDotDot

                  IAnnDrawPen pen = engine.ToPen(mapper.StrokeFromContainerCoordinates(stroke, annIntersectionPointObject.FixedStateOperations), annIntersectionPointObject.Opacity);
                  try
                  {
                     if (leadPoints.Length > 2)
                     {
                        LeadPointD intersectionPoint = annIntersectionPointObject.IntersectionPoint;
                        intersectionPoint = mapper.PointFromContainerCoordinates(intersectionPoint, annIntersectionPointObject.FixedStateOperations);

                        double radius = mapper.LengthFromContainerCoordinates(annIntersectionPointObject.IntersectionPointRadius, annIntersectionPointObject.FixedStateOperations);
                        DrawPoint(annIntersectionPointObject, engine, intersectionPoint, radius);

                        if (leadPoints.Length < 5 && annIntersectionPointObject.IntersectionInsideContainer)
                        {
                           engine.DrawLine(pen, leadPoints[3], intersectionPoint);
                        }
                     }
                  }
                  finally
                  {
                     engine.Destroy(pen);
                  }
               }
            }
         }
      }

      private void DrawPoint(AnnIntersectionPointObject annObject, IAnnDrawEngine engine, LeadPointD point, double radius)
      {
         LeadRectD pointBounds = new LeadRectD(point.X - radius, point.Y - radius, radius * 2, radius * 2);
         LeadPointD topLeft = pointBounds.TopLeft;
         LeadPointD topRight = pointBounds.TopRight;
         LeadPointD bottomLeft = pointBounds.BottomLeft;
         LeadPointD bottomRight = pointBounds.BottomRight;

         IAnnDrawPen pen = engine.ToPen(AnnStroke.Create(AnnSolidColorBrush.Create("green"), annObject.Stroke.StrokeThickness), annObject.Opacity);
         try
         {
            engine.DrawLine(pen, topLeft, bottomRight);
            engine.DrawLine(pen, bottomLeft, topRight);
            engine.DrawLine(pen, new LeadPointD(point.X, (point.Y - pointBounds.Width / 2)), new LeadPointD(point.X, (point.Y + pointBounds.Width / 2)));
            engine.DrawLine(pen, new LeadPointD(point.X - pointBounds.Width / 2, point.Y), new LeadPointD(point.X + pointBounds.Width / 2, point.Y));
         }
         finally
         {
            engine.Destroy(pen);
         }
      }
   }

}
