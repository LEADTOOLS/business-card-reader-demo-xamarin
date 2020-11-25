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
   public class AnnParallelLinesDrawer : AnnRectangleDrawDesigner
   {
      static AnnRectangleObject _objectTemplate;

      public virtual int LinesCount
      {
         get { return 2; }
      }

      public override AnnObject FinalTargetObject
      {
         get { return _annParallelLinesObject; }
      }
      static AnnParallelLinesDrawer()
      {
         _objectTemplate = new AnnRectangleObject();
         _objectTemplate.Stroke.Stroke = AnnSolidColorBrush.Create("blue");
         _objectTemplate.Stroke.StrokeDashArray = new double[] { 4, 2, 2, 2, 2, 2 };
      }

      AnnParallelLinesObject _annParallelLinesObject = null;
      public AnnParallelLinesDrawer(IAnnAutomationControl automationControl, AnnContainer container, AnnParallelLinesObject annParallelLinesObject)
         : base(automationControl, container, _objectTemplate)
      {
         _annParallelLinesObject = annParallelLinesObject;
      }

      protected override bool EndWorking()
      {
         LeadPointD[] points = _objectTemplate.Points.ToArray();

         if (points != null && points.Length > 0)
         {
            _annParallelLinesObject.Points.Add(points[0]);
            _annParallelLinesObject.Points.Add(points[1]);

            int lineCount = LinesCount - 2;
            if (LinesCount > 0)
            {
               int x = (lineCount + 1);
               double height = _objectTemplate.Rect.Height / x;

               LeadPointD start = points[0];
               LeadPointD end = points[1];

               for (int i = 0; i < lineCount; ++i)
               {
                  start = LeadPointD.Create(start.X, start.Y + height);
                  end = LeadPointD.Create(end.X, end.Y + height);

                  _annParallelLinesObject.Points.Add(SnapPointToGrid(start, false));
                  _annParallelLinesObject.Points.Add(SnapPointToGrid(end, false));
               }
            }

            _annParallelLinesObject.Points.Add(points[2]);
            _annParallelLinesObject.Points.Add(points[3]);
         }

         _annParallelLinesObject.Labels["AnnObjectName"] = _objectTemplate.Labels["AnnObjectName"].Clone();

         Container.Children.Remove(_objectTemplate);
         TargetObject = _annParallelLinesObject;
         Container.Children.Add(_annParallelLinesObject);
         return base.EndWorking();
      }
   }
}
