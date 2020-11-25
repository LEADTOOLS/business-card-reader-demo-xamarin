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
   public class AnnTowLinesObjectRenderer : AnnObjectRenderer
   {
      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         IAnnDrawEngine engine = RenderingEngine as IAnnDrawEngine;

         if (engine != null)
         {
            LeadPointD[] leadPoints = annObject.Points.ToArray();
            int linesCount = leadPoints.Length / 2;
            if (linesCount > 0)
            {
               if (annObject.SupportsStroke && annObject.Stroke != null)
               {

                  leadPoints = mapper.PointsFromContainerCoordinates(leadPoints, annObject.FixedStateOperations);

                  IAnnDrawPen pen = engine.ToPen(mapper.StrokeFromContainerCoordinates(annObject.Stroke, annObject.FixedStateOperations), annObject.Opacity);
                  try
                  {
                     for (int i = 0; i < linesCount; ++i)
                     {
                        LeadPointD firstPoint = leadPoints[2 * i];
                        LeadPointD secondPoint = leadPoints[2 * i + 1];

                        engine.DrawLine(pen, firstPoint, secondPoint);
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
