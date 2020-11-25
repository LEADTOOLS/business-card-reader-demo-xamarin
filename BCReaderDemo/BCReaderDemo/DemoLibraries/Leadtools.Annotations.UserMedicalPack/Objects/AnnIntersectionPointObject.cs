// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Rendering;
using Leadtools.Annotations.Engine;
using System.Xml;

namespace Leadtools.Annotations.UserMedicalPack
{
   public partial class AnnIntersectionPointObject : AnnObject
   {
      public AnnIntersectionPointObject()
      {
         SetId(-1006);
      }

      protected override AnnObject Create()
      {
         return new AnnIntersectionPointObject();
      }

      public override string FriendlyName
      {
         get
         {
            return "Intersection Point";
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

      private bool _intersectionInsideContainer = false;
      public bool IntersectionInsideContainer
      {
         get
         {
            return _intersectionInsideContainer;
         }
         set
         {
            _intersectionInsideContainer = value;
         }
      }


      private LeadPointD _intersectionPoint = LeadPointD.Empty;

      public virtual LeadPointD IntersectionPoint
      {
         get
         {
            CalculateIntersectionPoint();
            return _intersectionPoint;
         }
      }

      private LeadLengthD _intersectionPointRadius = LeadLengthD.Create(16);

      public virtual LeadLengthD IntersectionPointRadius
      {
         get
         {
            return _intersectionPointRadius;
         }
         set
         {
            if (_intersectionPointRadius.Value >= 0)
               _intersectionPointRadius = value;
            else
               throw new InvalidOperationException("IntersectionPointRadius should be greater than or equal 0");
         }
      }

      protected override LeadRectD GetBoundingRectangle()
      {
         LeadRectD rc = base.GetBoundingRectangle();

         double radius = _intersectionPointRadius.Value;
         if (!(double.IsInfinity(radius) || (double.IsInfinity(radius))))
         {
            if (!_intersectionPoint.IsEmpty && _intersectionInsideContainer)
            {
               LeadRectD intersectionBounds = LeadRectD.Create(_intersectionPoint.X - radius, _intersectionPoint.Y - radius, radius * 2, radius * 2);
               rc = LeadRectD.UnionRects(rc, intersectionBounds);
            }

         }

         return rc;
      }

      private void CalculateIntersectionPoint()
      {
         double firstLineLength, cos, sin, newX, firstLinePosition;

         if (Points.Count < 4)
         {
            _intersectionPoint = LeadPointD.Empty;

            return;
         }

         LeadPointD Line1FirstPoint = Points[0];
         LeadPointD Line1SecondPoint = Points[1];
         LeadPointD Line2FirstPoint = Points[2];
         LeadPointD Line2SecondPoint = Points[3];

         //If either line1 length is 0 or line2 length is 0 return empty point.
         if (LeadPointD.Equals(Line2FirstPoint, Line2SecondPoint))
         {
            _intersectionPoint = LeadPointD.Empty;

            return;
         }

         //Translate the system so that line1 first point is on the origin.
         Line1SecondPoint.X -= Line1FirstPoint.X; Line1SecondPoint.Y -= Line1FirstPoint.Y;
         Line2FirstPoint.X -= Line1FirstPoint.X; Line2FirstPoint.Y -= Line1FirstPoint.Y;
         Line2SecondPoint.X -= Line1FirstPoint.X; Line2SecondPoint.Y -= Line1FirstPoint.Y;

         //Calculate first line length.
         firstLineLength = (float)Math.Sqrt((float)(Line1SecondPoint.X * Line1SecondPoint.X + Line1SecondPoint.Y * Line1SecondPoint.Y));

         //Rotate the system so that first line second point is on the positive X axis.
         cos = Line1SecondPoint.X / firstLineLength;
         sin = Line1SecondPoint.Y / firstLineLength;
         newX = Line2FirstPoint.X * cos + Line2FirstPoint.Y * sin;
         Line2FirstPoint.Y = Line2FirstPoint.Y * cos - Line2FirstPoint.X * sin; Line2FirstPoint.X = newX;
         newX = Line2SecondPoint.X * cos + Line2SecondPoint.Y * sin;
         Line2SecondPoint.Y = Line2SecondPoint.Y * cos - Line2SecondPoint.X * sin; Line2SecondPoint.X = newX;

         //If the lines are parallel return empty point.
         if (Line2FirstPoint.Y == Line2SecondPoint.Y)
         {
            _intersectionPoint = LeadPointD.Empty;

            return;
         }

         //Find the position of the intersection point along first line
         firstLinePosition = Line2SecondPoint.X + (Line2FirstPoint.X - Line2SecondPoint.X) * Line2SecondPoint.Y / (Line2SecondPoint.Y - Line2FirstPoint.Y);

         //Apply the founded position to first line in the original coordinate system.

         _intersectionPoint = LeadPointD.Create(Line1FirstPoint.X + firstLinePosition * cos, Line1FirstPoint.Y + firstLinePosition * sin);

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

      public override void Serialize(AnnSerializeOptions options, XmlNode parentNode, XmlDocument document)
      {
         base.Serialize(options, parentNode, document);

         XmlNode element = document.CreateElement("IntersectionPointRadius");
         element.InnerText = _intersectionPointRadius.Value.ToString();
         parentNode.AppendChild(element);
      }

      public override void Deserialize(AnnDeserializeOptions options, XmlNode element, XmlDocument document)
      {
         base.Deserialize(options, element, document);

         string data = string.Empty;

         XmlElement xmlElement = element as XmlElement;
         XmlNodeList nodeList = xmlElement.GetElementsByTagName("IntersectionPointRadius");

         foreach (XmlNode childNode in nodeList)
         {
            if (childNode != null && (childNode.ParentNode == element))
            {
               data = childNode.FirstChild.Value.Trim();
               break;
            }
         }

         _intersectionPointRadius = LeadLengthD.Create(double.Parse(data));
      }

   }
}
