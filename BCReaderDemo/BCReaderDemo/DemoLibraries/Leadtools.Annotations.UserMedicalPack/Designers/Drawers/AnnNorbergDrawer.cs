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
   public class AnnNorbergDrawer : AnnRectangleDrawDesigner
   {
      static AnnRectangleObject _objectTemplate;

      public override AnnObject FinalTargetObject
      {
         get { return _annNorbergObject; }
      }

      static AnnNorbergDrawer()
      {
         _objectTemplate = new AnnRectangleObject();
         _objectTemplate.Stroke.Stroke = AnnSolidColorBrush.Create("black");
         _objectTemplate.Stroke.StrokeThickness = LeadLengthD.Create(2);
      }

      AnnNorbergObject _annNorbergObject = null;
      public AnnNorbergDrawer(IAnnAutomationControl automationControl, AnnContainer container, AnnNorbergObject annNorbergObject)
         : base(automationControl, container, _objectTemplate)
      {
         _annNorbergObject = annNorbergObject;
      }

      protected override bool EndWorking()
      {
         LeadPointD[] points = _objectTemplate.Points.ToArray();

         if (points != null && points.Length > 0)
         {
            _annNorbergObject.Points.Clear();
            _annNorbergObject.Points.Add(points[0]);
            _annNorbergObject.Points.Add(points[3]);
            _annNorbergObject.Points.Add(points[2]);
            _annNorbergObject.Points.Add(points[1]);
         }

         Container.Children.Remove(_objectTemplate);
         TargetObject = _annNorbergObject;
         Container.Children.Add(_annNorbergObject);

         return base.EndWorking();
      }
   }
}
