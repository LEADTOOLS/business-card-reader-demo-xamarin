// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

using Leadtools.Controls;
using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.Xamarin
{
   // This is the automation interactive mode
   public class AutomationInteractiveMode : ImageViewerInteractiveMode
   {
      public const int AutomationId = AutomationInteractiveMode.UserModeId;

      public AutomationInteractiveMode()
      {
         // Hook to left and right mouse button (if supported)
         this.MouseButtons = MouseButtons.Left | MouseButtons.Right;
         // Setup our item mode, when the user first click, make this our item
         this.AutoItemMode = ImageViewerAutoItemMode.AutoSet;
         // Work on the image
         this.ItemPart = ImageViewerItemPart.Image;
         // So selection works even if we click outside the items
         this.WorkOnBounds = false;
      }

      // Supports either setting the automation control or if the ImageViewer itself implements IAnnAutomationControl
      private IAnnAutomationControl _automationControl;

      public IAnnAutomationControl AutomationControl
      {
         get { return _automationControl; }
         set { _automationControl = value; }
      }

      private IAnnAutomationControl WorkAutomationControl
      {
         get
         {
            if (_automationControl != null)
               return _automationControl;
            else
               return this.ImageViewer as IAnnAutomationControl;
         }
      }

      private int _id = AutomationId;
      public override int Id
      {
         get { return _id; }
      }

      public override string Name
      {
         get { return "AutomationInteractiveMode"; }
      }

      protected override bool CanStartWork(InteractiveEventArgs e)
      {
         return base.CanStartWork(e) && this.WorkAutomationControl != null;
      }

      public override void Start(ImageViewer imageViewer)
      {
         base.Start(imageViewer);

         var interactiveService = base.InteractiveService;

         interactiveService.DragStarted += new EventHandler<InteractiveDragStartedEventArgs>(interactiveService_DragStarted);
         interactiveService.DragDelta += new EventHandler<InteractiveDragDeltaEventArgs>(interactiveService_DragDelta);
         interactiveService.DragCompleted += new EventHandler<InteractiveDragCompletedEventArgs>(interactiveService_DragCompleted);
         interactiveService.Tap += new EventHandler<InteractiveEventArgs>(interactiveService_Tap);
         interactiveService.DoubleTap += new EventHandler<InteractiveEventArgs>(interactiveService_DoubleTap);
         interactiveService.Move += new EventHandler<InteractiveEventArgs>(interactiveService_Move);
      }

      public override void Stop(ImageViewer imageViewer)
      {
         if (IsStarted)
         {
            InteractiveService interactiveService = base.InteractiveService;
            interactiveService.DragStarted -= new EventHandler<InteractiveDragStartedEventArgs>(interactiveService_DragStarted);
            interactiveService.DragDelta -= new EventHandler<InteractiveDragDeltaEventArgs>(interactiveService_DragDelta);
            interactiveService.DragCompleted -= new EventHandler<InteractiveDragCompletedEventArgs>(interactiveService_DragCompleted);
            interactiveService.Tap -= new EventHandler<InteractiveEventArgs>(interactiveService_Tap);
            interactiveService.DoubleTap -= new EventHandler<InteractiveEventArgs>(interactiveService_DoubleTap);
            interactiveService.Move -= new EventHandler<InteractiveEventArgs>(interactiveService_Move);

            base.Stop(imageViewer);
         }
      }

      private static AnnPointerEventArgs ConvertPointerEventArgs(InteractiveEventArgs e, bool isDoubleTap)
      {
         // Convert the point
         var point = LeadPointD.Create(e.Position.X, e.Position.Y);

         // Convert the mouse button
         var mouseButton = AnnMouseButton.Left;

         if (!isDoubleTap)
         {
            if (e.MouseButton == MouseButtons.Right)
               mouseButton = AnnMouseButton.Right;
         }

         var args = AnnPointerEventArgs.Create(mouseButton, point);
         args.IsHandled = e.IsHandled;
         return args;
      }

      private void interactiveService_DragStarted(object sender, InteractiveDragStartedEventArgs e)
      {
         if (CanStartWork(e) && !e.IsMouseWheel)
         {
            OnWorkStarted(EventArgs.Empty);

            var annArgs = ConvertPointerEventArgs(e, false);
            if (!e.IsHandled)
            {
               this.WorkAutomationControl.OnAutomationPointerDown(annArgs);
               e.IsHandled = annArgs.IsHandled;

               if (!e.IsHandled)
                  OnWorkCompleted(EventArgs.Empty);
            }
         }
      }

      private void interactiveService_DragDelta(object sender, InteractiveDragDeltaEventArgs e)
      {
         if (IsWorking)
         {
            var annArgs = ConvertPointerEventArgs(e, false);
            if (!e.IsHandled)
            {
               this.WorkAutomationControl.OnAutomationPointerMove(annArgs);
               e.IsHandled = annArgs.IsHandled;
            }
         }
      }

      private void interactiveService_DragCompleted(object sender, InteractiveDragCompletedEventArgs e)
      {
         if (IsWorking)
         {
            var annArgs = ConvertPointerEventArgs(e, false);
            if (!e.IsHandled)
            {
               this.WorkAutomationControl.OnAutomationPointerUp(annArgs);
               e.IsHandled = annArgs.IsHandled;
               OnWorkCompleted(EventArgs.Empty);
            }
         }
      }

      private void interactiveService_Tap(object sender, InteractiveEventArgs e)
      {
         if (!IsWorking && CanStartWork(e))
         {
            var annArgs = ConvertPointerEventArgs(e, false);
            if (!e.IsHandled)
            {
               this.WorkAutomationControl.OnAutomationPointerUp(annArgs);
               e.IsHandled = annArgs.IsHandled;
            }
         }
      }

      private void interactiveService_DoubleTap(object sender, InteractiveEventArgs e)
      {
         if (CanStartWork(e))
         {
            var annArgs = ConvertPointerEventArgs(e, true);
            if (!e.IsHandled)
            {
               OnWorkStarted(EventArgs.Empty);
               this.WorkAutomationControl.OnAutomationDoubleClick(annArgs);
               e.IsHandled = annArgs.IsHandled;
               OnWorkCompleted(EventArgs.Empty);
            }
         }
      }

      private void interactiveService_Move(object sender, InteractiveEventArgs e)
      {
         if (this.WorkAutomationControl == null)
            return;

         var annArgs = ConvertPointerEventArgs(e, false);
         this.WorkAutomationControl.OnAutomationPointerMove(annArgs);
      }
   }
}
