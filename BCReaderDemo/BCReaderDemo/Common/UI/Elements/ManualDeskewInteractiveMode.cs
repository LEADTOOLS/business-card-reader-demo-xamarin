// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Controls;
using Leadtools.ImageProcessing.Core;
using System;

namespace Leadtools.Demos.UI.Elements
{
   // Image viewer interactive mode to show manual deskew moveable area with resizable thumbs
   public class ManualDeskewInteractiveMode : ImageViewerInteractiveMode
   {
      public ManualDeskewInteractiveMode()
      {
      }

      public const int ModeId = UserModeId + 20;

      private const int ManualDeskewHitTest = 75;
      private const int ManualDeskewDeltaThreshold = 20;
      private const int ManualDeskewMinimumPointSeparation = 20;

      private enum ManualDeskewActivePoint
      {
         None,
         TopLeft,
         TopRight,
         BottomLeft,
         BottomRight,
         TopMid,
         LeftMid,
         RightMid,
         BottomMid
      }

      public override string Name
      {
         get { return "Manual Deskew"; }
      }

      public override int Id
      {
         get { return ModeId; }
      }

      public RasterImage TransformedImage { get; private set; }

      private ManualDeskewActivePoint ActivePoint { get; set; }

      private LeadPoint _topLeft = LeadPoint.Empty;
      private LeadPoint TopLeft
      {
         get
         {
            return _topLeft;
         }
         set
         {
            bool changeX = false, changeY = false;

            if (value.X + ManualDeskewMinimumPointSeparation < _topRight.X) changeX = true;
            if (value.Y + ManualDeskewMinimumPointSeparation < _bottomLeft.Y) changeY = true;

            if (changeX && IsValidX(value.X)) _topLeft.X = value.X;
            if (changeY && IsValidY(value.Y)) _topLeft.Y = value.Y;
         }
      }

      private LeadPoint _topRight = LeadPoint.Empty;
      private LeadPoint TopRight
      {
         get
         {
            return _topRight;
         }
         set
         {
            bool changeX = false, changeY = false;

            if (value.X - ManualDeskewMinimumPointSeparation > _topLeft.X) changeX = true;
            if (value.Y - ManualDeskewMinimumPointSeparation < _bottomRight.Y) changeY = true;

            if (changeX && IsValidX(value.X)) _topRight.X = value.X;
            if (changeY && IsValidY(value.Y)) _topRight.Y = value.Y;
         }
      }

      private LeadPoint _bottomLeft = LeadPoint.Empty;
      private LeadPoint BottomLeft
      {
         get
         {
            return _bottomLeft;
         }
         set
         {
            bool changeX = false, changeY = false;

            if (value.X + ManualDeskewMinimumPointSeparation < _bottomRight.X) changeX = true;
            if (value.Y - ManualDeskewMinimumPointSeparation > _topLeft.Y) changeY = true;

            if (changeX && IsValidX(value.X)) _bottomLeft.X = value.X;
            if (changeY && IsValidY(value.Y)) _bottomLeft.Y = value.Y;
         }
      }

      private LeadPoint _bottomRight = LeadPoint.Empty;
      private LeadPoint BottomRight
      {
         get
         {
            return _bottomRight;
         }
         set
         {
            bool changeX = false, changeY = false;

            if (value.X - ManualDeskewMinimumPointSeparation > _bottomLeft.X) changeX = true;
            if (value.Y - ManualDeskewMinimumPointSeparation > _topRight.Y) changeY = true;

            if (changeX && IsValidX(value.X)) _bottomRight.X = value.X;
            if (changeY && IsValidY(value.Y)) _bottomRight.Y = value.Y;
         }
      }

      private LeadPoint TopMid
      {
         get
         {
            return LeadPoint.Create((int)((_topLeft.X + _topRight.X) * 0.5), (int)((_topLeft.Y + _topRight.Y) * 0.5));
         }
         set
         {
            int dX = (int)(value.X - (_topLeft.X + _topRight.X) * 0.5);
            int dY = (int)(value.Y - (_topLeft.Y + _topRight.Y) * 0.5);

            if (IsValidX(_topRight.X + dX) && IsValidX(_topLeft.X + dX))
            {
               _topRight.X += dX;
               _topLeft.X += dX;
            }

            if ((_bottomLeft.Y - _topLeft.Y - dY) > ManualDeskewDeltaThreshold && (_bottomRight.Y - _topRight.Y - dY) > ManualDeskewDeltaThreshold && IsValidY(_topRight.Y + dY) && IsValidY(_topLeft.Y + dY))
            {
               _topRight.Y += dY;
               _topLeft.Y += dY;
            }
         }
      }

      private LeadPoint BottomMid
      {
         get
         {
            return LeadPoint.Create((int)((_bottomLeft.X + _bottomRight.X) * 0.5), (int)((_bottomLeft.Y + _bottomRight.Y) * 0.5));
         }
         set
         {
            int dX = (int)(value.X - (_bottomLeft.X + _bottomRight.X) * 0.5);
            int dY = (int)(value.Y - (_bottomLeft.Y + _bottomRight.Y) * 0.5);

            if (IsValidX(_bottomRight.X + dX) && IsValidX(_bottomLeft.X + dX))
            {
               _bottomRight.X += dX;
               _bottomLeft.X += dX;
            }

            if ((_bottomLeft.Y - _topLeft.Y + dY) > ManualDeskewDeltaThreshold && (_bottomRight.Y - _topRight.Y + dY) > ManualDeskewDeltaThreshold && IsValidY(_bottomRight.Y + dY) && IsValidY(_bottomLeft.Y + dY))
            {
               _bottomRight.Y += dY;
               _bottomLeft.Y += dY;
            }
         }
      }

      private LeadPoint LeftMid
      {
         get
         {
            return LeadPoint.Create((int)((_topLeft.X + _bottomLeft.X) * 0.5), (int)((_topLeft.Y + _bottomLeft.Y) * 0.5));
         }
         set
         {
            int dX = (int)(value.X - (_topLeft.X + _bottomLeft.X) * 0.5);
            int dY = (int)(value.Y - (_topLeft.Y + _bottomLeft.Y) * 0.5);

            if (IsValidY(_topLeft.Y + dY) && IsValidY(_bottomLeft.Y + dY))
            {
               _topLeft.Y += dY;
               _bottomLeft.Y += dY;
            }

            if ((_topRight.X - _topLeft.X - dX) > ManualDeskewDeltaThreshold && (_bottomRight.X - _bottomLeft.X - dX) > ManualDeskewDeltaThreshold && IsValidX(_bottomLeft.X + dX) && IsValidX(_topLeft.X + dX))
            {
               _topLeft.X += dX;
               _bottomLeft.X += dX;
            }
         }
      }

      private LeadPoint RightMid
      {
         get
         {
            return LeadPoint.Create((int)((_topRight.X + _bottomRight.X) * 0.5), (int)((_topRight.Y + _bottomRight.Y) * 0.5));
         }
         set
         {
            int dX = (int)(value.X - (_topRight.X + _bottomRight.X) * 0.5);
            int dY = (int)(value.Y - (_topRight.Y + _bottomRight.Y) * 0.5);

            if (IsValidY(_topRight.Y + dY) && IsValidY(_bottomRight.Y + dY))
            {
               _topRight.Y += dY;
               _bottomRight.Y += dY;
            }

            if ((_bottomRight.X - _bottomLeft.X + dX) > ManualDeskewDeltaThreshold && (_topRight.X - _topLeft.X + dX) > ManualDeskewDeltaThreshold && IsValidX(_bottomRight.X + dX) && IsValidX(_topRight.X + dX))
            {
               _topRight.X += dX;
               _bottomRight.X += dX;
            }
         }
      }

      public override void Start(ImageViewer imageViewer)
      {
         base.Start(imageViewer);

         InteractiveService service = base.InteractiveService;
         service.DragStarted += new EventHandler<InteractiveDragStartedEventArgs>(service_DragStarted);
         service.DragDelta += new EventHandler<InteractiveDragDeltaEventArgs>(service_DragDelta);
         service.DragCompleted += new EventHandler<InteractiveDragCompletedEventArgs>(service_DragCompleted);

         ImageViewer.TransformChanged += ImageViewer_TransformChanged;
         ImageViewer.Invalidate(LeadRectD.Empty);
      }

      public override void Stop(ImageViewer imageViewer)
      {
         if (IsStarted)
         {
            imageViewer.TransformChanged -= ImageViewer_TransformChanged;

            InteractiveService service = base.InteractiveService;
            service.DragStarted -= new EventHandler<InteractiveDragStartedEventArgs>(service_DragStarted);
            service.DragDelta -= new EventHandler<InteractiveDragDeltaEventArgs>(service_DragDelta);
            service.DragCompleted -= new EventHandler<InteractiveDragCompletedEventArgs>(service_DragCompleted);
            base.Stop(imageViewer);
         }
      }

      public LeadPoint[] GetControlPoints(bool convertToItemCoordinates)
      {
         LeadPoint[] controlPoints = new LeadPoint[]
         {
            TopLeft,
            TopMid,
            TopRight,
            RightMid,
            BottomRight,
            BottomMid,
            BottomLeft,
            LeftMid
         };

         if (convertToItemCoordinates)
            ImageViewer.ConvertPoints(null, ImageViewerCoordinateType.Image, ImageViewerCoordinateType.Item, controlPoints);

         return controlPoints;
      }

      private void UpdateControlPoints()
      {
         if (ImageViewer != null && ImageViewer.ActiveItem != null && ImageViewer.ActiveItem.Element != null)
         {
            SelectAreaView view = ImageViewer.ActiveItem.Element as SelectAreaView;
            LeadPoint[] controlPoints = GetControlPoints(true);
            view.ControlPoints = controlPoints;
         }
      }

      public void ResetThumbs(LeadPoint[] thumbs)
      {
         _topLeft = thumbs[0];
         _topRight = thumbs[1];
         _bottomRight = thumbs[2];
         _bottomLeft = thumbs[3];

         UpdateControlPoints();
      }

      private void ImageViewer_TransformChanged(object sender, EventArgs e)
      {
         UpdateControlPoints();
      }

      protected override bool CanStartWork(InteractiveEventArgs e)
      {
         if (!base.CanStartWork(e)) return false;

         // IMPORTANT NOTE: The SelectAreaView we render the control points inside and the shaded area is added to the image viewer item itself
         // and not on the whole image viewer control so we need to map the touch points to item coordinates.
         var position = ImageViewer.ConvertPoint(null, ImageViewerCoordinateType.Control, ImageViewerCoordinateType.Image, e.Position);

         ManualDeskewActivePoint activePoint = FindActivePoint(position);
         if (activePoint != ManualDeskewActivePoint.None)
         {
            ActivePoint = activePoint;
            return true;
         }

         return false;
      }

      private void service_DragStarted(object sender, InteractiveDragStartedEventArgs e)
      {
         if (!CanStartWork(e) || ImageViewer.Image == null || IsWorking)
            return;

         OnWorkStarted(EventArgs.Empty);
      }

      private void service_DragDelta(object sender, InteractiveDragDeltaEventArgs e)
      {
         if (!IsWorking)
            return;

         // IMPORTANT NOTE: The SelectAreaView we render the control points inside and the shaded area is added to the image viewer item itself
         // and not on the whole image viewer control so we need to map the touch points to item coordinates.
         var position = ImageViewer.ConvertPoint(null, ImageViewerCoordinateType.Control, ImageViewerCoordinateType.Image, e.Position);

         if (ActivePoint != ManualDeskewActivePoint.None)
         {
            switch (ActivePoint)
            {
               case ManualDeskewActivePoint.TopLeft: TopLeft = position; break;
               case ManualDeskewActivePoint.TopRight: TopRight = position; break;
               case ManualDeskewActivePoint.BottomLeft: BottomLeft = position; break;
               case ManualDeskewActivePoint.BottomRight: BottomRight = position; break;
               case ManualDeskewActivePoint.TopMid: TopMid = position; break;
               case ManualDeskewActivePoint.BottomMid: BottomMid = position; break;
               case ManualDeskewActivePoint.LeftMid: LeftMid = position; break;
               case ManualDeskewActivePoint.RightMid: RightMid = position; break;
               default: break;
            }
         }

         UpdateControlPoints();
      }

      private void service_DragCompleted(object sender, InteractiveDragCompletedEventArgs e)
      {
         if (IsWorking)
         {
            ActivePoint = ManualDeskewActivePoint.None;

            OnWorkCompleted(EventArgs.Empty);
         }
      }

      private ManualDeskewActivePoint FindActivePoint(LeadPoint point)
      {
         if (DistanceBetweenPoints(point, TopLeft) < ManualDeskewHitTest) return ManualDeskewActivePoint.TopLeft;
         if (DistanceBetweenPoints(point, TopRight) < ManualDeskewHitTest) return ManualDeskewActivePoint.TopRight;
         if (DistanceBetweenPoints(point, BottomLeft) < ManualDeskewHitTest) return ManualDeskewActivePoint.BottomLeft;
         if (DistanceBetweenPoints(point, BottomRight) < ManualDeskewHitTest) return ManualDeskewActivePoint.BottomRight;
         if (DistanceBetweenPoints(point, TopMid) < ManualDeskewHitTest) return ManualDeskewActivePoint.TopMid;
         if (DistanceBetweenPoints(point, LeftMid) < ManualDeskewHitTest) return ManualDeskewActivePoint.LeftMid;
         if (DistanceBetweenPoints(point, RightMid) < ManualDeskewHitTest) return ManualDeskewActivePoint.RightMid;
         if (DistanceBetweenPoints(point, BottomMid) < ManualDeskewHitTest) return ManualDeskewActivePoint.BottomMid;

         return ManualDeskewActivePoint.None;
      }

      private int DistanceBetweenPoints(LeadPoint point1, LeadPoint point2)
      {
         int dx = point2.X - point1.X;
         int dy = point2.Y - point1.Y;

         return (int)Math.Sqrt((dx * dx) + (dy * dy));
      }

      private bool IsValidX(int x)
      {
         LeadRectD itemBounds = ImageViewer.GetItemBounds(ImageViewer.ActiveItem, ImageViewerItemPart.Item);
         itemBounds = ImageViewer.ConvertRect(null, ImageViewerCoordinateType.Item, ImageViewerCoordinateType.Image, itemBounds);
         return x >= 0.0 && x <= itemBounds.Size.Width;
      }

      private bool IsValidY(int y)
      {
         LeadRectD itemBounds = ImageViewer.GetItemBounds(ImageViewer.ActiveItem, ImageViewerItemPart.Item);
         itemBounds = ImageViewer.ConvertRect(null, ImageViewerCoordinateType.Item, ImageViewerCoordinateType.Image, itemBounds);
         return y >= 0.0 && y <= itemBounds.Size.Height;
      }

      public RasterImage ApplyDeskew()
      {
         if (ImageViewer == null || ImageViewer.ActiveItem.Image == null) return null;

         LeadPoint[] polygonPoints = new LeadPoint[4];
         polygonPoints[0] = TopLeft;
         polygonPoints[1] = TopRight;
         polygonPoints[2] = BottomRight;
         polygonPoints[3] = BottomLeft;
         KeyStoneCommand command = new KeyStoneCommand(polygonPoints);
         command.Run(ImageViewer.ActiveItem.Image);

         return command.TransformedImage;
      }
   }
}
