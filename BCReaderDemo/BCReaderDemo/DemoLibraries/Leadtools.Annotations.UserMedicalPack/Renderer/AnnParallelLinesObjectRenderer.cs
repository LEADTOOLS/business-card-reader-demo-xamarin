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
   public class AnnParallelLinesObjectRenderer : AnnObjectRenderer
   {
      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         IAnnDrawEngine engine = RenderingEngine as IAnnDrawEngine;

         if (engine != null)
         {
            int count = annObject.Points.Count / 2;

            if (count > 1)
            {
               LeadPointD[] tmpPoints = mapper.PointsFromContainerCoordinates(annObject.Points.ToArray(), annObject.FixedStateOperations);

               if (annObject.SupportsStroke && annObject.Stroke != null)
               {
                  IAnnDrawPen pen = engine.ToPen(mapper.StrokeFromContainerCoordinates(annObject.Stroke, annObject.FixedStateOperations), annObject.Opacity);
                  try
                  {
                     for (int i = 0; i < count; i++)
                     {
                        int index = 2 * i;
                        engine.DrawLine(pen, tmpPoints[index], tmpPoints[index + 1]);
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
