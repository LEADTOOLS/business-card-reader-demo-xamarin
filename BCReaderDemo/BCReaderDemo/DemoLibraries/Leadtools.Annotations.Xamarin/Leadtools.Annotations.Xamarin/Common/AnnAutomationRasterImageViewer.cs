// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

using Leadtools;
using Leadtools.Controls;
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Automation;
using Leadtools.Annotations.Rendering;

namespace Leadtools.Annotations.Xamarin
{
   public class AutomationRasterImageViewer : ImageViewer, IAnnAutomationControl
   {
      public AutomationRasterImageViewer() : base()
      {
         this.BackgroundColor = Color.Transparent;
         this.Focused += AutomationRasterImageViewer_Focused;
         this.PostRender += AutomationRasterImageViewer_PostRender;
      }

      // Pointer events
      public event EventHandler<AnnPointerEventArgs> AutomationPointerDown;
      public event EventHandler<AnnPointerEventArgs> AutomationPointerMove;
      public event EventHandler<AnnPointerEventArgs> AutomationPointerUp;
      public event EventHandler<AnnPointerEventArgs> AutomationDoubleClick;

      // Automation object
      private AnnAutomation _automationObject;
      public virtual object AutomationObject
      {
         get { return _automationObject; }
         set { _automationObject = value as AnnAutomation; }
      }

      public virtual void OnAutomationPointerDown(AnnPointerEventArgs e)
      {
         if (this.AutomationPointerDown != null)
            this.AutomationPointerDown(this, e);
      }

      public virtual void OnAutomationPointerMove(AnnPointerEventArgs e)
      {
         if (this.AutomationPointerMove != null)
            this.AutomationPointerMove(this, e);
      }

      public virtual void OnAutomationPointerUp(AnnPointerEventArgs e)
      {
         if (this.AutomationPointerUp != null)
            this.AutomationPointerUp(this, e);
      }

      public virtual void OnAutomationDoubleClick(AnnPointerEventArgs e)
      {
         if (this.AutomationDoubleClick != null)
            this.AutomationDoubleClick(this, e);
      }

      // Resolution
      public virtual double AutomationDpiX
      {
         get
         {
            return this.ScreenDpi.Width;
         }
      }

      public virtual double AutomationDpiY
      {
         get
         {
            return this.ScreenDpi.Height;
         }
      }

      // Enabled/Focus
      public virtual bool AutomationEnabled
      {
         get { return this.IsEnabled; }
      }

      public event EventHandler AutomationGotFocus;
      public event EventHandler AutomationLostFocus;

      private void AutomationRasterImageViewer_Focused(object sender, FocusEventArgs e)
      {
         if(e.IsFocused)
         {
            if (AutomationGotFocus != null)
               AutomationGotFocus(this, e);
         }
         else
         {
            if (AutomationLostFocus != null)
               AutomationLostFocus(this, e);
         }
      }

      // Fire automation size event when image has changed 
      public event EventHandler AutomationSizeChanged;
      protected override void OnItemChanged(ImageViewerItemChangedEventArgs e)
      {
         switch (e.Reason)
         {
            case ImageViewerItemChangedReason.Url:
            case ImageViewerItemChangedReason.Image:
            case ImageViewerItemChangedReason.ImageChanged:
            case ImageViewerItemChangedReason.Size:
            case ImageViewerItemChangedReason.Transform:
            case ImageViewerItemChangedReason.Visibility:
               // When the item size changes (or the image inside the item, it might have a new size - for example, if the user
               // resizes the image), we need to inform the automation of this to resize the container accordingly
               // Fire the AutomationSizeChanged event
               if (AutomationTransformChanged != null)
                  AutomationTransformChanged(this, EventArgs.Empty);
               if (AutomationSizeChanged != null)
                  AutomationSizeChanged(this, EventArgs.Empty);
               break;

            default:
               break;
         }

         base.OnItemChanged(e);
      }

      private ImageViewerItem GetItemForCurrentContainer()
      {
         return this.ActiveItem;
      }

      // Annotations toolkit will handle the DPI, so always return the transform without one
      public virtual LeadMatrix AutomationTransform
      {
         get
         {
            ImageViewerItem item = GetItemForCurrentContainer();
            if (item != null)
            {
               // Check if it is visible (for single item mode)
               var bounds = this.GetItemBounds(item, ImageViewerItemPart.Item);
               if (bounds.IsEmpty)
               {
                  // Make it go outside the screen
                  var matrix = LeadMatrix.Identity;
                  matrix.OffsetX = -100000;
                  matrix.OffsetY = -100000;
                  return matrix;
               }

               return this.GetItemImageTransformWithDpi(item, false);
            }
            else
               return LeadMatrix.Identity;
         }
      }

      // Inform the automation that the current transformation has changed, user scrolled or zoomed
      public event EventHandler AutomationTransformChanged;
      public override void OnTransformChanged(EventArgs e)
      {
         base.OnTransformChanged(e);

         // Fire the AutomationTransformChanged event
         if (AutomationTransformChanged != null)
         {
            AutomationTransformChanged(this, EventArgs.Empty);
         }

         if (_automationObject != null)
            _automationObject.Invalidate(LeadRectD.Empty);
      }

      // Get a value indicating whether the automation should take the image DPI (resolution) into consideration when viewing
      public bool AutomationUseDpi
      {
         get { return UseDpi; }
      }

      // Inform the automation that UseDpi has changed
      public event EventHandler AutomationUseDpiChanged;
      public event EventHandler AutomationEnabledChanged;

      public new bool IsEnabled
      {
         get { return base.IsEnabled; }
         set
         {
            base.IsEnabled = value;
            if (AutomationEnabledChanged != null)
               AutomationEnabledChanged(this, EventArgs.Empty);
         }
      }

      protected override void OnPropertyChanged(string name)
      {
         base.OnPropertyChanged(name);
         if (name == "UseDpi")
         {
            if (AutomationUseDpiChanged != null)
            {
               AutomationUseDpiChanged(this, EventArgs.Empty);
            }
         }
      }

      // Get the image DPI (resolution)
      public virtual double AutomationXResolution
      {
         get
         {
            ImageViewerItem item = GetItemForCurrentContainer();

            if (item != null)
               return item.Resolution.Width;
            else
               return 96.0;
         }
      }

      public virtual double AutomationYResolution
      {
         get
         {
            ImageViewerItem item = item = GetItemForCurrentContainer();

            if (item != null)
               return item.Resolution.Height;
            else
               return 96.0;
         }
      }

      private AnnRenderingEngine _engine;
      public virtual AnnRenderingEngine RenderingEngine
      {
         get
         {
            return _engine;
         }
         set
         {
            _engine = value;
         }
      }

      public void AutomationInvalidate(LeadRectD rc)
      {
         InvalidateImageSurface(rc);
      }

      private void AutomationRasterImageViewer_PostRender(object sender, ImageViewerRenderEventArgs e)
      {
         var engine = _engine as AnnDrawRenderingEngine;
         if (engine == null || e.SurfaceContext == null)
            return;

         // Render all containers
         if (_getContainersCallback != null)
         {
            // Using multi-containers
            AnnContainerCollection containers = _getContainersCallback();
            foreach (AnnContainer container in containers)
               RenderContainer(e, engine, container);
         }
         else
         {
            // Using single-containers, just render the active
            RenderContainer(e, engine, _container);
         }
      }

      private void RenderContainer(ImageViewerRenderEventArgs e, AnnDrawRenderingEngine engine, AnnContainer container)
      {
         object surface = e.SurfaceContext;

         if (surface == null)
            return;

         // Attach to the current container and graphics.
         engine.Attach(container, surface);

         try
         {
            // Render the annotations
            engine.Render(LeadRectD.Empty, false);
         }
         finally
         {
            engine.Detach();
         }
      }

      // Containers support
      // Multi container support
      private AnnAutomationControlGetContainersCallback _getContainersCallback;
      public virtual AnnAutomationControlGetContainersCallback AutomationGetContainersCallback
      {
         get { return _getContainersCallback; }
         set { _getContainersCallback = value; }
      }

      // Single container support
      private AnnContainer _container;
      public virtual void AutomationAttach(AnnContainer container)
      {
         if (container != null)
         {
            _container = container;
            container.Children.CollectionChanged += new EventHandler<AnnNotifyCollectionChangedEventArgs>(CollectionChanged);
            container.Layers.CollectionChanged += new EventHandler<AnnNotifyCollectionChangedEventArgs>(CollectionChanged);
         }
      }

      public virtual void AutomationDetach()
      {
         _container.Children.CollectionChanged -= new EventHandler<AnnNotifyCollectionChangedEventArgs>(CollectionChanged);
         _container.Layers.CollectionChanged -= new EventHandler<AnnNotifyCollectionChangedEventArgs>(CollectionChanged);
         _container = null;
      }


      public AnnContainer AutomationContainer
      {
         get { return _container; }
      }

      // Data provider for the images
      private AnnDataProvider _dataProvider;
      public virtual AnnDataProvider AutomationDataProvider
      {
         get { return _dataProvider; }
         set { _dataProvider = value; }
      }

      private void CollectionChanged(object sender, AnnNotifyCollectionChangedEventArgs e)
      {
         AutomationInvalidate(LeadRectD.Empty);
      }

      // Scroll Offset values for viewer
      public virtual LeadPoint AutomationScrollOffset
      {
         get { return LeadPoint.Create((int)this.ScrollOffset.X, (int)this.ScrollOffset.Y); }
      }

      public virtual double AutomationRotateAngle
      {
         get { return this.RotateAngle; }
      }

      public virtual double AutomationScaleFactor
      {
         get { return this.ScaleFactor; }
      }

      private bool _isAutomationEventsHooked;
      public virtual bool IsAutomationEventsHooked
      {
         get { return _isAutomationEventsHooked; }
         set { _isAutomationEventsHooked = value; }
      }

      // Turn anti aliasing on and off
      private bool _antiAlias;
      public virtual bool AutomationAntiAlias
      {
         get { return _antiAlias; }
         set { _antiAlias = value; }
      }

      private int _containerIndex = -1;
      public virtual int AutomationContainerIndex
      {
         get { return _containerIndex; }
         set { _containerIndex = value; }
      }
   }
}
