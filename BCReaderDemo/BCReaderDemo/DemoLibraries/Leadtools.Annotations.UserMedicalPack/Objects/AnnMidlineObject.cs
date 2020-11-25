// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Engine;
using System.Xml;

namespace Leadtools.Annotations.UserMedicalPack
{
   public partial class AnnMidlineObject : AnnObject
   {
      public AnnMidlineObject()
      {
         SetId(-1004);
      }

      protected override AnnObject Create()
      {
         return new AnnMidlineObject();
      }

      public override string FriendlyName
      {
         get
         {
            return "Midline";
         }
      }

      public override bool SupportsStroke
      {
         get
         {
            return true;
         }
      }

      public override bool SupportsFill
      {
         get
         {
            return false;
         }
      }

      private LeadLengthD _centerPointRadius = LeadLengthD.Create(13);

      public virtual LeadLengthD CenterPointRadius
      {
         get
         {
            return _centerPointRadius;
         }
         set
         {
            if (_centerPointRadius.Value >= 0)
               _centerPointRadius = value;
            else
               throw new InvalidOperationException("CenterPointRadius should be greater than or equal 0");
         }
      }

      protected override LeadRectD GetBoundingRectangle()
      {
         LeadRectD rc = base.GetBoundingRectangle();

         double radius = _centerPointRadius.Value;
         if (!(double.IsInfinity(radius) || (double.IsInfinity(radius))) && !rc.IsEmpty)
         {
            rc.Inflate(radius, radius);
         }

         return rc;
      }

      public override bool HitTest(LeadPointD point, double hitTestBuffer)
      {
         bool hit = base.HitTest(point, hitTestBuffer);

         if (hit)
         {
            AnnPolylineObject line = new AnnPolylineObject();

            hit = false;

            int pointsCount = Points.Count / 2;
            for (int i = 0; i < pointsCount && !hit; ++i)
            {
               line.Points.Add(Points[2 * i]);
               line.Points.Add(Points[2 * i + 1]);
               if (line.HitTest(point, hitTestBuffer))
                  hit = true;
            }
         }

         return hit;
      }

      public override void Serialize(AnnSerializeOptions options, System.Xml.XmlNode parentNode, System.Xml.XmlDocument document)
      {
         base.Serialize(options, parentNode, document);

         XmlNode element = document.CreateElement("CenterPointRadius");
         element.InnerText = _centerPointRadius.Value.ToString();
         parentNode.AppendChild(element);
      }

      public override void Deserialize(AnnDeserializeOptions options, XmlNode element, XmlDocument document)
      {
         base.Deserialize(options, element, document);

         string data = string.Empty;

         XmlElement xmlElement = element as XmlElement;
         XmlNodeList nodeList = xmlElement.GetElementsByTagName("CenterPointRadius");

         foreach (XmlNode childNode in nodeList)
         {
            if (childNode != null && (childNode.ParentNode == element))
            {
               data = childNode.FirstChild.Value.Trim();
               break;
            }
         }

         _centerPointRadius = LeadLengthD.Create(double.Parse(data));
      }
   }
}
