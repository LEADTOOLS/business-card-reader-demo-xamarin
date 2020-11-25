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
   public class AnnCobbAngleObjectRenderer : AnnTowLinesObjectRenderer
   {
      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         IAnnDrawEngine engine = RenderingEngine as IAnnDrawEngine;

         if (engine != null)
         {
            base.Render(mapper, annObject);

            AnnCobbAngleObject annCobbAngleObject = annObject as AnnCobbAngleObject;
            if (annCobbAngleObject != null)
            {
               int count = annCobbAngleObject.Points.Count;
               if (count < 2)
                  return;

               LeadPointD[] leadPoints = mapper.PointsFromContainerCoordinates(annCobbAngleObject.Points.ToArray(), annCobbAngleObject.FixedStateOperations);

               if (annCobbAngleObject.SupportsStroke && annCobbAngleObject.Stroke != null)
               {
                  AnnStroke stroke = AnnStroke.Create(AnnSolidColorBrush.Create("Blue"), annCobbAngleObject.Stroke.StrokeThickness);
                  stroke.StrokeDashArray = new double[] { 4, 2, 2, 2, 2, 2 };
                  IAnnDrawPen pen = engine.ToPen(mapper.StrokeFromContainerCoordinates(annCobbAngleObject.Stroke, annCobbAngleObject.FixedStateOperations), annCobbAngleObject.Opacity);
                  try
                  {
                     if (leadPoints.Length > 3)
                     {
                        AnnCobbAngleData cobbAngleData = annCobbAngleObject.CobbAngleData;
                        LeadPointD firstPoint = mapper.PointFromContainerCoordinates(cobbAngleData.FirstPoint, annCobbAngleObject.FixedStateOperations);
                        LeadPointD secondPoint = mapper.PointFromContainerCoordinates(cobbAngleData.SecondPoint, annCobbAngleObject.FixedStateOperations);
                        LeadPointD intersectionPoint = mapper.PointFromContainerCoordinates(cobbAngleData.IntersectionPoint, annCobbAngleObject.FixedStateOperations);

                        engine.DrawLine(pen, firstPoint, intersectionPoint);
                        engine.DrawLine(pen, secondPoint, intersectionPoint);

                        //Draw angle label
                        if (annCobbAngleObject.Labels.ContainsKey("CobbAngle"))
                        {
                           AnnLabel label = annCobbAngleObject.Labels["CobbAngle"];
                           if (label != null)
                           {
                              string precisionFormat = string.Format("XXX:F{0}", annCobbAngleObject.AnglePrecision);
                              precisionFormat = precisionFormat.Replace("XXX", "{0");
                              precisionFormat = string.Format("{0}{1}", precisionFormat, "}");

                              string angle = string.Format(precisionFormat, cobbAngleData.Angle);

                              label.Text = angle;
                              label.Foreground = AnnSolidColorBrush.Create("Blue");
                              label.OriginalPosition = cobbAngleData.IntersectionPoint;
                           }
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
   }
}
