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
   public class AnnMidlineObjectRenderer : AnnTowLinesObjectRenderer
   {
      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         IAnnDrawEngine engine = RenderingEngine as IAnnDrawEngine;

         if (engine != null)
         {
            base.Render(mapper, annObject);

            AnnMidlineObject annMidlineObject = annObject as AnnMidlineObject;
            if (annMidlineObject != null)
            {
               LeadPointD[] leadPoints = annMidlineObject.Points.ToArray();
               int linesCount = leadPoints.Length / 2;
               if (linesCount > 0)
               {
                  LeadPointD[] linesCenters = new LeadPointD[linesCount];
                  leadPoints = mapper.PointsFromContainerCoordinates(leadPoints, annMidlineObject.FixedStateOperations);

                  if (annMidlineObject.SupportsStroke && annMidlineObject.Stroke != null)
                  {
                     double radius = mapper.LengthFromContainerCoordinates(annMidlineObject.CenterPointRadius, annMidlineObject.FixedStateOperations);
                     IAnnDrawPen pen = engine.ToPen(mapper.StrokeFromContainerCoordinates(annMidlineObject.Stroke, annMidlineObject.FixedStateOperations), annMidlineObject.Opacity);
                     try
                     {
                        for (int i = 0; i < linesCount; ++i)
                        {
                           LeadPointD firstPoint = leadPoints[2 * i];
                           LeadPointD secondPoint = leadPoints[2 * i + 1];

                           LeadPointD center = new LeadPointD((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2);
                           DrawPoint(annMidlineObject, engine, center, radius);

                           linesCenters[i] = center;
                        }

                        if (linesCount > 1)
                        {
                           int count = linesCount - 1;
                           for (int i = 0; i < count; ++i)
                              engine.DrawLine(pen, linesCenters[i], linesCenters[i + 1]); // draw midline
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
      }

      private void DrawPoint(AnnMidlineObject annObject, IAnnDrawEngine engine, LeadPointD point, double radius)
      {
         LeadRectD pointBounds = new LeadRectD(point.X - radius, point.Y - radius, radius * 2, radius * 2);
         LeadPointD topLeft = pointBounds.TopLeft;
         LeadPointD topRight = pointBounds.TopRight;
         LeadPointD bottomLeft = pointBounds.BottomLeft;
         LeadPointD bottomRight = pointBounds.BottomRight;

         IAnnDrawPen pen = engine.ToPen(AnnStroke.Create(AnnSolidColorBrush.Create("blue"), annObject.Stroke.StrokeThickness), annObject.Opacity);
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
