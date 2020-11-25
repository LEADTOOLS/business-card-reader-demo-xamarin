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
   public class AnnNorbergObjectRenderer : AnnProtractorObjectRenderer
   {
      public AnnNorbergObjectRenderer()
      {
      }

      public override void Render(AnnContainerMapper mapper, AnnObject annObject)
      {
         if (mapper == null) ExceptionHelper.ArgumentNullException("mapper");
         if (annObject == null) ExceptionHelper.ArgumentNullException("annObject");

         AnnProtractorObject firstProtractorObject = annObject.Clone() as AnnProtractorObject;
         firstProtractorObject.Points.Clear();
         firstProtractorObject.Points.Add(annObject.Points[0]);
         firstProtractorObject.Points.Add(annObject.Points[1]);
         firstProtractorObject.Points.Add(annObject.Points[2]);
         base.Render(mapper, firstProtractorObject);

         AnnProtractorObject secondProtractorObject = annObject.Clone() as AnnProtractorObject;
         secondProtractorObject.Points.Clear();
         secondProtractorObject.Points.Add(annObject.Points[1]);
         secondProtractorObject.Points.Add(annObject.Points[2]);
         secondProtractorObject.Points.Add(annObject.Points[3]);
         base.Render(mapper, secondProtractorObject);

         annObject.Labels["FirstAngle"] = firstProtractorObject.Labels["AngleText"].Clone();
         annObject.Labels["SecondAngle"] = secondProtractorObject.Labels["AngleText"].Clone();
      }
   }
}
