// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Automation;
using Leadtools.Annotations.Designers;
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Rendering;

namespace Leadtools.Annotations.UserMedicalPack
{
   public partial class AnnMedicalPack : IAnnPackage
   {
      private double _thumbSize = 144;
      public double ThumbSize
      {
         get { return _thumbSize; }
         set
         {
            if (value >= 0)
               _thumbSize = value;
            else
               throw new InvalidOperationException("_thumbSize");
         }
      }

      private AnnThumbStyle CreateLocationThumbStyle()
      {
         AnnThumbStyle locationThumbStyle = new AnnRectangleThumbStyle();

         double thumbSize = ThumbSize;
         locationThumbStyle.Size = LeadSizeD.Create(thumbSize, thumbSize);
         locationThumbStyle.Stroke = AnnStroke.Create(AnnSolidColorBrush.Create("black"), LeadLengthD.Create(1));

         locationThumbStyle.Fill = AnnSolidColorBrush.Create("lightblue");

         return locationThumbStyle;
      }

      private AnnThumbStyle CreateRotateCenterThumbStyle()
      {
         AnnThumbStyle rotateCenterThumbStyle = new AnnEllipseThumbStyle();

         double thumbSize = ThumbSize;
         rotateCenterThumbStyle.Size = LeadSizeD.Create(thumbSize / 2.0, thumbSize / 2.0);
         rotateCenterThumbStyle.Stroke = AnnStroke.Create(AnnSolidColorBrush.Create("black"), LeadLengthD.Create(1));

         rotateCenterThumbStyle.Fill = AnnSolidColorBrush.Create("lightgreen");

         return rotateCenterThumbStyle;
      }

      private AnnThumbStyle CreateRotateGripperThumbStyle()
      {
         AnnThumbStyle rotateGripperThumbStyle = new AnnEllipseThumbStyle();

         double thumbSize = ThumbSize;
         rotateGripperThumbStyle.Size = LeadSizeD.Create(thumbSize, thumbSize);
         rotateGripperThumbStyle.Stroke = AnnStroke.Create(AnnSolidColorBrush.Create("black"), LeadLengthD.Create(1));

         rotateGripperThumbStyle.Fill = AnnSolidColorBrush.Create("lightgreen");

         return rotateGripperThumbStyle;
      }

      private AnnAutomationObject CreateParallelLines()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();
         AnnParallelLinesObject annParallelLinesObject = new AnnParallelLinesObject();

         automationObj.Id = annParallelLinesObject.Id;
         automationObj.Name = "Two Parallel Lines";
         automationObj.DrawDesignerType = typeof(AnnParallelLinesDrawer);
         automationObj.EditDesignerType = typeof(AnnParallelLinesEditor);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annParallelLinesObject;

         IAnnObjectRenderer renderer = new AnnParallelLinesObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();

         automationObj.Renderer = renderer;

         return automationObj;
      }

      private AnnAutomationObject CreateFourParallelLines()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();
         AnnParallelLinesObject annParallelLinesObject = new AnnParallelLinesObject();
         annParallelLinesObject.SetId(annParallelLinesObject.Id - 1);

         automationObj.Id = annParallelLinesObject.Id;
         automationObj.Name = "Four Parallel Lines";
         automationObj.DrawDesignerType = typeof(AnnFourParallelLinesDrawer);
         automationObj.EditDesignerType = typeof(AnnParallelLinesEditor);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annParallelLinesObject;

         IAnnObjectRenderer renderer = new AnnParallelLinesObjectRenderer();
         automationObj.Renderer = renderer;
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();

         return automationObj;
      }

      private AnnAutomationObject CreateMidline()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();
         AnnMidlineObject annMidlineObject = new AnnMidlineObject();

         automationObj.Id = annMidlineObject.Id;
         automationObj.Name = "MidLine";
         automationObj.DrawDesignerType = typeof(AnnTwoLinesDrawer);
         automationObj.EditDesignerType = typeof(AnnTwoLinesEditer);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annMidlineObject;

         IAnnObjectRenderer renderer = new AnnMidlineObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();
         renderer.RotateCenterThumbStyle = CreateRotateCenterThumbStyle();
         renderer.RotateGripperThumbStyle = CreateRotateGripperThumbStyle();

         automationObj.Renderer = renderer;

         return automationObj;
      }

      private AnnAutomationObject CreateSnapPoint()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();
         AnnMidlineObject annMidlineObject = new AnnMidlineObject();
         annMidlineObject.SetId(annMidlineObject.Id - 1);
         automationObj.Id = annMidlineObject.Id;
         automationObj.Name = "SnapPoint";
         automationObj.DrawDesignerType = typeof(AnnSnapPointDrawer);
         automationObj.EditDesignerType = typeof(AnnSnapPointEditor);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annMidlineObject;

         IAnnObjectRenderer renderer = new AnnMidlineObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();
         automationObj.Renderer = renderer;

         return automationObj;
      }

      private AnnAutomationObject CreateIntersectionPoint()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();

         AnnIntersectionPointObject annIntersectionPointObject = new AnnIntersectionPointObject();
         automationObj.Id = annIntersectionPointObject.Id;
         automationObj.Name = "Intersection Point";
         automationObj.DrawDesignerType = typeof(AnnTwoLinesDrawer);
         automationObj.EditDesignerType = typeof(AnnTwoLinesEditer);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annIntersectionPointObject;

         IAnnObjectRenderer renderer = new AnnIntersectionObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();
         renderer.RotateCenterThumbStyle = CreateRotateCenterThumbStyle();
         renderer.RotateGripperThumbStyle = CreateRotateGripperThumbStyle();

         automationObj.Renderer = renderer;

         return automationObj;
      }

      private AnnAutomationObject CreateCobbAngle()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();

         AnnCobbAngleObject annCobbAngleObject = new AnnCobbAngleObject();
         automationObj.Id = annCobbAngleObject.Id;
         automationObj.Name = "CobbAngle";
         automationObj.DrawDesignerType = typeof(AnnTwoLinesDrawer);
         automationObj.EditDesignerType = typeof(AnnTwoLinesEditer);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annCobbAngleObject;

         IAnnObjectRenderer renderer = new AnnCobbAngleObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();
         renderer.RotateCenterThumbStyle = CreateRotateCenterThumbStyle();
         renderer.RotateGripperThumbStyle = CreateRotateGripperThumbStyle();

         automationObj.Renderer = renderer;

         return automationObj;
      }

      private AnnAutomationObject CreateNorberg()
      {
         AnnAutomationObject automationObj = new AnnAutomationObject();

         AnnNorbergObject annCobbAngleObject = new AnnNorbergObject();
         automationObj.Id = annCobbAngleObject.Id;
         automationObj.Name = "Norberg";
         automationObj.DrawDesignerType = typeof(AnnNorbergDrawer);
         automationObj.EditDesignerType = typeof(AnnPolylineEditDesigner);
         automationObj.RunDesignerType = typeof(AnnRunDesigner);
         automationObj.ObjectTemplate = annCobbAngleObject;

         IAnnObjectRenderer renderer = new AnnNorbergObjectRenderer();
         renderer.LocationsThumbStyle = CreateLocationThumbStyle();
         renderer.RotateCenterThumbStyle = CreateRotateCenterThumbStyle();
         renderer.RotateGripperThumbStyle = CreateRotateGripperThumbStyle();

         automationObj.Renderer = renderer;

         return automationObj;
      }

      public AnnAutomationObject[] GetAutomationObjects()
      {
         List<AnnAutomationObject> objects = new List<AnnAutomationObject>();

         objects.Add(CreateParallelLines());
         objects.Add(CreateFourParallelLines());
         objects.Add(CreateMidline());
         objects.Add(CreateSnapPoint());
         objects.Add(CreateIntersectionPoint());
         objects.Add(CreateCobbAngle());
         objects.Add(CreateNorberg());

         return objects.ToArray();
      }

      #region IAnnPackage Members

      public string Author
      {
         get { return "Lead Technologies,Inc."; }
      }

      public string Description
      {
         get { return "Medical Package"; }
      }

      public string FriendlyName
      {
         get { return "Medical Package"; }
      }

      #endregion
   }
}
