// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnCobbAngleData
   {
      public LeadPointD FirstPoint { get; set; }
      public LeadPointD SecondPoint { get; set; }
      public LeadPointD IntersectionPoint { get; set; }
      public double Angle { get; set; }
   }

   public partial class AnnCobbAngleObject : AnnObject
   {
      public AnnCobbAngleObject()
      {
         SetId(-1007);
         Labels["CobbAngle"] = new AnnLabel();
         FixedStateOperations = AnnFixedStateOperations.FontSize;
      }

      protected override AnnObject Create()
      {
         return new AnnCobbAngleObject();
      }

      public override string FriendlyName
      {
         get
         {
            return "CobbAngle";
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

      private AnnCobbAngleData _cobbAngleData = new AnnCobbAngleData();
      public virtual AnnCobbAngleData CobbAngleData
      {
         get
         {
            CalculateCobbAngleData();
            return _cobbAngleData;
         }
      }

      private int _anglePrecision = 2;
      public int AnglePrecision
      {
         get { return _anglePrecision; }
         set
         {
            if (value >= 0)
               _anglePrecision = value;
            else
               throw new InvalidOperationException("AnglePrecision should be greater than or equal 0");
         }
      }

      private void CalculateCobbAngleData()
      {
         if (this.Points.Count < 4)
            return;

         LeadPointD[] points = this.Points.ToArray();

         LeadPointD startPoint1 = points[0];
         LeadPointD endPoint1 = points[1];
         LeadPointD startPoint2 = points[2];
         LeadPointD endPoint2 = points[3];

         double angle1 = GetLineAngle(startPoint1, endPoint1);
         double angle2 = GetLineAngle(startPoint2, endPoint2);

         LeadPointD point1;
         LeadPointD point2;
         LeadPointD intersectionPoint;

         double distanceSS = Distance(startPoint1.X, startPoint1.Y, startPoint2.X, startPoint2.Y);
         double distanceSE = Distance(startPoint1.X, startPoint1.Y, endPoint2.X, endPoint2.Y);
         double distanceES = Distance(endPoint1.X, endPoint1.Y, startPoint2.X, startPoint2.Y);
         double distanceEE = Distance(endPoint1.X, endPoint1.Y, endPoint2.X, endPoint2.Y);

         bool IsSS = false;
         int minimumIndex = 0;
         if (distanceSS < distanceSE)
         {
            IsSS = true;
         }

         if (distanceES < distanceEE)
         {
            if (IsSS)
            {
               if (distanceSS < distanceES)
                  minimumIndex = 0;
               else
                  minimumIndex = 2;
            }
            else
            {
               if (distanceSE < distanceES)
                  minimumIndex = 1;
               else
                  minimumIndex = 2;
            }
         }
         else
         {
            if (IsSS)
            {
               if (distanceSS < distanceEE)
                  minimumIndex = 0;
               else
                  minimumIndex = 3;
            }
            else
            {
               if (distanceSE < distanceEE)
                  minimumIndex = 1;
               else
                  minimumIndex = 3;
            }
         }

         double factor1 = 1;
         switch (minimumIndex)
         {
            case 0:
               {
                  factor1 = -1;
                  point1 = startPoint1;
                  point2 = startPoint2;
               }
               break;
            case 1:
               {
                  factor1 = -1;
                  point1 = startPoint1;
                  point2 = endPoint2;
               }
               break;
            case 2:
               {
                  point1 = endPoint1;
                  point2 = startPoint2;
               }
               break;

            default:
            case 3:
               {
                  point1 = endPoint1;
                  point2 = endPoint2;
               }
               break;
         }

         double distance = 40;
         intersectionPoint = GetPointExtension(point1, angle1, distance, factor1);

         if (angle1 < 0)
            angle1 += Math.PI;

         if (angle2 < 0)
            angle2 += Math.PI;

         double resultAngle = ((angle1 - angle2) * 180 / Math.PI);

         if (resultAngle < 0)
            resultAngle += 180;

         if (resultAngle > 90)
            resultAngle = 180 - resultAngle;

         _cobbAngleData.FirstPoint = point1;
         _cobbAngleData.SecondPoint = point2;
         _cobbAngleData.IntersectionPoint = intersectionPoint;
         _cobbAngleData.Angle = resultAngle;
      }

      private double GetLineAngle(LeadPointD point1, LeadPointD point2)
      {
         double value = Math.Atan2((point2.Y - point1.Y), (point2.X - point1.X));
         return value;
      }
      private double Distance(double x1, double y1, double x2, double y2)
      {
         double dX = (x1 - x2);
         double dY = (y1 - y2);
         return (double)Math.Sqrt(dX * dX + dY * dY);
      }
      LeadPointD GetPointExtension(LeadPointD point1, double angle, double distance, double factor)
      {
         double dXRatio = factor * distance * Math.Cos(angle);
         double dYRatio = factor * distance * Math.Sin(angle);

         LeadPointD resultPoint = LeadPointD.Create((point1.X + dXRatio), (point1.Y + dYRatio));

         return resultPoint;
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

      protected override LeadRectD GetBoundingRectangle()
      {
         LeadRectD rc = base.GetBoundingRectangle();

         if (!_cobbAngleData.IntersectionPoint.IsEmpty)
            rc = LeadRectD.Union(rc, _cobbAngleData.IntersectionPoint);

         return rc;
      }

      public override LeadRectD GetInvalidateRect(AnnContainerMapper mapper, IAnnObjectRenderer renderer)
      {
         LeadRectD invalidateRect = base.GetInvalidateRect(mapper, renderer);

         // Add angle label to the invalidate rect 
         IAnnLabelRenderer labelRenderer = renderer.LabelRenderer;

         if (labelRenderer != null && labelRenderer.RenderingEngine != null && Labels.ContainsKey("CobbAngle"))
         {
            AnnLabel label = Labels["CobbAngle"];
            LeadRectD lablebounds = labelRenderer.GetBounds(mapper, label, FixedStateOperations);
            invalidateRect = LeadRectD.UnionRects(lablebounds, invalidateRect);
         }

         return invalidateRect;
      }

      public override void Serialize(AnnSerializeOptions options, System.Xml.XmlNode parentNode, System.Xml.XmlDocument document)
      {
         base.Serialize(options, parentNode, document);

         XmlNode element = document.CreateElement("AnglePrecision");
         element.InnerText = AnglePrecision.ToString();
         parentNode.AppendChild(element);
      }

      public override void Deserialize(AnnDeserializeOptions options, XmlNode element, XmlDocument document)
      {
         base.Deserialize(options, element, document);

         string data = string.Empty;

         XmlElement xmlElement = element as XmlElement;
         XmlNodeList nodeList = xmlElement.GetElementsByTagName("AnglePrecision");

         foreach (XmlNode childNode in nodeList)
         {
            if (childNode != null && (childNode.ParentNode == element))
            {
               data = childNode.FirstChild.Value.Trim();
               break;
            }
         }

         _anglePrecision = int.Parse(data);
      }

   }
}
