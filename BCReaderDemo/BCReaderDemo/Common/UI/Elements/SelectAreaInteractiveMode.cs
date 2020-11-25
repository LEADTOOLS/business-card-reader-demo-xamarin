// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Controls;
using System;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   // Custom view to render the shaded area and the select area control points on.
   // This view will be added to the image viewer item itself and not on the whole image viewer.
   public class SelectAreaView : View
   {
      public SelectAreaView() : base()
      {
      }

      public static readonly BindableProperty SelectAreaRectangleProperty = BindableProperty.Create(
         propertyName: nameof(SelectAreaRectangle),
         returnType: typeof(LeadRect),
         declaringType: typeof(SelectAreaView),
         defaultValue: LeadRect.Empty,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public LeadRect SelectAreaRectangle { get => GetValue(SelectAreaRectangleProperty) is LeadRect value ? value : LeadRect.Empty; set => SetValue(SelectAreaRectangleProperty, value); }

      public static readonly BindableProperty ControlPointsProperty = BindableProperty.Create(
         propertyName: nameof(ControlPoints),
         returnType: typeof(LeadPoint[]),
         declaringType: typeof(SelectAreaView),
         defaultValue: null,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public LeadPoint[] ControlPoints { get => GetValue(ControlPointsProperty) is LeadPoint[] value ? value : null; set => SetValue(ControlPointsProperty, value); }

      public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
         propertyName: nameof(IsActive),
         returnType: typeof(bool),
         declaringType: typeof(SelectAreaView),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool IsActive { get => GetValue(IsActiveProperty) is bool value ? value : false; set => SetValue(IsActiveProperty, value); }
   }

   public class SelectedAreaChangedEventArgs : EventArgs
   {
      public SelectedAreaChangedEventArgs(LeadRect selectedArea)
      {
         SelectedArea = selectedArea;
      }

      public LeadRect SelectedArea { get; private set; }
   }

   // Image viewer interactive mode to show moveable area with resizable thumbs
   public class SelectAreaInteractiveMode : ImageViewerInteractiveMode
   {
      public SelectAreaInteractiveMode()
      {
         this.WorkOnBounds = true;
      }

      public const int ModeId = UserModeId + 1;

      private const int _thumbSize = 16;

      public event EventHandler<SelectedAreaChangedEventArgs> SelectedAreaChanged;

      public override string Name
      {
         get { return "Select Area"; }
      }

      public override int Id
      {
         get { return ModeId; }
      }

      private LeadPoint[] _imagePoints;

      private LeadPoint[] _controlPoints;
      public LeadPoint[] GetControlPoints()
      {
         return _controlPoints;
      }

      private void UpdateImagePoints(LeadPoint[] points)
      {
         _imagePoints = points;
         if (_imagePoints != null)
            _controlPoints = new LeadPoint[_imagePoints.Length];
         else
            _controlPoints = null;

         if (this.IsStarted)
         {
            UpdateControlPoints();
            UpdateSelectedAreaView();
            ImageViewer.Invalidate(LeadRectD.Empty);
         }
      }

      private void UpdateImagePoints()
      {
         for (var i = 0; i < _controlPoints.Length; i++)
            _imagePoints[i] = new LeadPoint(_controlPoints[i].X, _controlPoints[i].Y);
         ImageViewer.ConvertPoints(null, ImageViewerCoordinateType.Item, ImageViewerCoordinateType.Image, _imagePoints);
      }

      private void UpdateControlPoints()
      {
         for (var i = 0; i < _imagePoints.Length; i++)
            _controlPoints[i] = new LeadPoint(_imagePoints[i].X, _imagePoints[i].Y);
         ImageViewer.ConvertPoints(null, ImageViewerCoordinateType.Image, ImageViewerCoordinateType.Item, _controlPoints);
      }

      private LeadPoint[] PointsFromRectangle(LeadRect rect)
      {
         var topLeft = new LeadPoint(rect.Left, rect.Top);
         var bottomRight = new LeadPoint(rect.Right, rect.Bottom);
         return PointsFromCorners(topLeft, bottomRight);
      }

      private LeadPoint[] PointsFromCorners(LeadPoint topLeft, LeadPoint bottomRight)
      {
         // Set the points from a 2 corner points
         var x1 = topLeft.X;
         var y1 = topLeft.Y;
         var x2 = bottomRight.X;
         var y2 = bottomRight.Y;

         var width = x2 - x1;
         var height = y2 - y1;
         var halfWidth = width / 2;
         var halfHeight = height / 2;

         return new LeadPoint[]
         {
            new LeadPoint(x1, y1),
            new LeadPoint(x1 + halfWidth, y1),
            new LeadPoint(x2, y1),
            new LeadPoint(x2, y1 + halfHeight),
            new LeadPoint(x2, y2),
            new LeadPoint(x1 + halfWidth, y2),
            new LeadPoint(x1, y2),
            new LeadPoint(x1, y1 + halfHeight)
         };
      }

      public LeadRect GetSelectedAreaRectangle(bool convertToItemCoordinates)
      {
         if (_imagePoints == null || ImageViewer == null) return LeadRect.Empty;

         LeadRect rect = LeadRect.FromLTRB(_imagePoints[0].X, _imagePoints[0].Y, _imagePoints[4].X, _imagePoints[4].Y);
         if (convertToItemCoordinates)
            rect = ImageViewer.ConvertRect(null, ImageViewerCoordinateType.Image, ImageViewerCoordinateType.Item, rect);

         return rect;
      }

      public void SetImageRectangle(LeadRect rect)
      {
         LeadPoint[] controlPoints = PointsFromRectangle(rect);
         UpdateImagePoints(controlPoints);
      }

      public override void Start(ImageViewer imageViewer)
      {
         base.Start(imageViewer);

         InteractiveService service = base.InteractiveService;
         service.DragStarted += new EventHandler<InteractiveDragStartedEventArgs>(service_DragStarted);
         service.DragDelta += new EventHandler<InteractiveDragDeltaEventArgs>(service_DragDelta);
         service.DragCompleted += new EventHandler<InteractiveDragCompletedEventArgs>(service_DragCompleted);

         imageViewer.TransformChanged += ImageViewer_TransformChanged;

         _dragThumbIndex = -1;
         _isDraggingBody = false;

         this.ImageViewer.Invalidate(LeadRectD.Empty);
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

      private void ImageViewer_TransformChanged(object sender, EventArgs e)
      {
         if (_controlPoints == null)
            return;

         UpdateControlPoints();
         UpdateSelectedAreaView();
      }

      private void UpdateSelectedAreaView()
      {
         if (ImageViewer != null && ImageViewer.ActiveItem != null && ImageViewer.ActiveItem.Element != null)
         {
            SelectAreaView view = ImageViewer.ActiveItem.Element as SelectAreaView;
            view.SelectAreaRectangle = GetSelectedAreaRectangle(false);
            view.ControlPoints = _controlPoints;

            SelectedAreaChanged?.Invoke(this, new SelectedAreaChangedEventArgs(view.SelectAreaRectangle));
         }
      }

      private static int HitTestThumb(LeadPoint testPoint, LeadPoint[] points, int thumbSize)
      {
         // Return the index of the handle under this point. -1 if none
         var pointCount = points.Length;
         for (var i = 0; i < pointCount; i++)
         {
            var point = points[i];
            var rect = new LeadRect(point.X - thumbSize, point.Y - thumbSize, thumbSize * 2, thumbSize * 2);
            if (testPoint.X >= rect.Left && testPoint.X <= rect.Right && testPoint.Y >= rect.Top && testPoint.Y <= rect.Bottom)
               return i;
         }

         return -1;
      }

      // The thumb index we are dragging
      private int _dragThumbIndex;
      // Or we are dragging the body
      private bool _isDraggingBody;

      private void service_DragStarted(object sender, InteractiveDragStartedEventArgs e)
      {
         if (!CanStartWork(e) || _controlPoints == null)
            return;

         // See if we are on any of our handles first
         // IMPORTANT NOTE: The SelectAreaView we render the control points inside and the shaded area is added to the image viewer item itself
         // and not on the whole image viewer control so we need to map the touch points to item coordinates.
         var testPoint = ImageViewer.ConvertPoint(null, ImageViewerCoordinateType.Control, ImageViewerCoordinateType.Item, e.Position);
         int thumbIndex = HitTestThumb(testPoint, _controlPoints, _thumbSize);
         if (thumbIndex != -1)
         {
            // We have a hit
            _dragThumbIndex = thumbIndex;
         }
         else
         {
            // Check if we are inside the rectangle, move all of it
            LeadRect selectedAreaRect = GetSelectedAreaRectangle(true);
            if(selectedAreaRect.Contains(testPoint))
               _isDraggingBody = true;
         }

         if (_dragThumbIndex != -1 || _isDraggingBody)
         {
            // So other interactive modes do not get this event
            e.IsHandled = true;
         }

         if (e.IsHandled)
            OnWorkStarted(EventArgs.Empty);
      }

      private void service_DragDelta(object sender, InteractiveDragDeltaEventArgs e)
      {
         if (!IsWorking)
            return;

         var imageViewer = this.ImageViewer;
         bool controlPointsHasChanged = false;
         LeadRectD itemBounds = ImageViewer.GetItemBounds(ImageViewer.ActiveItem, ImageViewerItemPart.Item);

         // IMPORTANT NOTE: The SelectAreaView we render the control points inside and the shaded area is added to the image viewer item itself
         // and not on the whole image viewer control so we need to map the touch points to item coordinates.
         var position = ImageViewer.ConvertPoint(null, ImageViewerCoordinateType.Control, ImageViewerCoordinateType.Item, e.Position);
         if (position.X < 0)
            position.X = 0;
         if (position.Y < 0)
            position.Y = 0;
         if (position.X > (int)itemBounds.Right)
            position.X = (int)itemBounds.Right;
         if (position.Y > (int)itemBounds.Bottom)
            position.Y = (int)itemBounds.Bottom;

         if (_dragThumbIndex != -1)
         {
            // Move this thumb

            // We have to find out which point we are and update the points from new corners accordingly
            switch (_dragThumbIndex)
            {
               case 0:
                  _controlPoints[0] = position;
                  break;

               case 1:
                  _controlPoints[0] = new LeadPoint(_controlPoints[0].X, position.Y);
                  break;

               case 2:
                  _controlPoints[0] = new LeadPoint(_controlPoints[0].X, position.Y);
                  _controlPoints[4] = new LeadPoint(position.X, _controlPoints[4].Y);
                  break;

               case 3:
                  _controlPoints[4] = new LeadPoint(position.X, _controlPoints[4].Y);
                  break;

               case 4:
                  _controlPoints[4] = position;
                  break;

               case 5:
                  _controlPoints[4] = new LeadPoint(_controlPoints[4].X, position.Y);
                  break;

               case 6:
                  _controlPoints[0] = new LeadPoint(position.X, _controlPoints[0].Y);
                  _controlPoints[4] = new LeadPoint(_controlPoints[4].X, position.Y);
                  break;

               case 7:
                  _controlPoints[0] = new LeadPoint(position.X, _controlPoints[0].Y);
                  break;

               default:
                  break;
            }

            _controlPoints = PointsFromCorners(_controlPoints[0], _controlPoints[4]);
            controlPointsHasChanged = true;
         }
         else if (_isDraggingBody)
         {
            // Just update the points
            int dx = e.Change.X;
            int dy = e.Change.Y;
            if (dx != 0 || dy != 0)
            {
               if (dx < 0 && _controlPoints[0].X + dx < 0)
                  dx = -_controlPoints[0].X;
               if (dx > 0 && _controlPoints[4].X + dx > itemBounds.Right)
                  dx = (int)itemBounds.Right - _controlPoints[4].X;
               if (dy < 0 && _controlPoints[0].Y + dy < 0)
                  dy = -_controlPoints[0].Y;
               if (dy > 0 && _controlPoints[4].Y + dy > itemBounds.Bottom)
                  dy = (int)itemBounds.Bottom - _controlPoints[4].Y;

               for (var i = 0; i < _controlPoints.Length; i++)
               {
                  _controlPoints[i].X += dx;
                  _controlPoints[i].Y += dy;
               }
               controlPointsHasChanged = true;
            }
         }

         if (controlPointsHasChanged)
         {
            UpdateImagePoints();
            UpdateSelectedAreaView();
            imageViewer.Invalidate(LeadRectD.Empty);
         }

         e.IsHandled = true;
      }

      private void service_DragCompleted(object sender, InteractiveDragCompletedEventArgs e)
      {
         if (IsWorking)
         {
            e.IsHandled = true;

            _dragThumbIndex = -1;
            _isDraggingBody = false;

            OnWorkCompleted(EventArgs.Empty);
         }
      }
   }
}
