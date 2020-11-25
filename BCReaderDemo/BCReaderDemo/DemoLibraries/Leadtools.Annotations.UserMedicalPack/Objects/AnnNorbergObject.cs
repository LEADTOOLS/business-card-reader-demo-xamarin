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
   public class AnnNorbergObject : AnnProtractorObject
   {
      public AnnNorbergObject()
      {
         SetId(-1008);

         this.Labels["FirstAngle"] = new AnnLabel();
         this.Labels["SecondAngle"] = new AnnLabel();

         this.Labels["AngleText"].Background = AnnSolidColorBrush.Create("white");
         this.Labels["AngleText"].Foreground = AnnSolidColorBrush.Create("blue");

         this.Labels["FirstRulerLength"].IsVisible = false;
         this.Labels["SecondRulerLength"].IsVisible = false;
      }

      protected override AnnObject Create()
      {
         return new AnnNorbergObject();
      }

      public override bool SupportsStroke
      {
         get
         {
            return true;
         }
      }

      public override string FriendlyName
      {
         get
         {
            return "Norberg";
         }
      }

      public override bool SupportsFill
      {
         get
         {
            return false;
         }
      }

      public override bool SupportsOpacity
      {
         get
         {
            return false;
         }
      }

      public override bool CanRotate
      {
         get
         {
            return false;
         }
      }

      public override bool ShowGauge
      {
         get { return false; }
         set { ;}
      }

      public override bool ShowTickMarks
      {
         get { return false; }
         set { ;}
      }
   }
}
