// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Annotations.Automation;
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Rendering;
using Leadtools.Controls;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Leadtools.Annotations.Xamarin
{
   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
   public enum AutomationControlMultiContainerMode
   {
      SinglePage,       // All containers belong to the same item (ActiveItem in the ImageViewer)
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPage")]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
      MultiPage         // One container per image viewer item
   }

   public delegate LeadMatrix UpdateAutomationTransformCallback(ImageViewerItem item, LeadMatrix transform);

   // This version of IAnnAutomationControl contains an image viewer
   public class ImageViewerAutomationControl : IAnnAutomationControl, IDisposable
   {
      public ImageViewerAutomationControl()
      {
      }

      private AutomationControlMultiContainerMode _multiContainerMode = AutomationControlMultiContainerMode.SinglePage;
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
      public AutomationControlMultiContainerMode MultiContainerMode
      {
         get { return _multiContainerMode; }
         set { _multiContainerMode = value; }
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      ~ImageViewerAutomationControl()
      {
         Dispose(false);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            Unhook();
         }
      }

      // The image viewer
      private ImageViewer _imageViewer;
      public ImageViewer ImageViewer
      {
         get { return _imageViewer; }
         set
         {
            if (_imageViewer != value)
            {
               Unhook();
               _imageViewer = value;
               Hook();
            }
         }
      }

      private void Hook()
      {
         if (_imageViewer == null)
            return; 

         // Hook to the image viewer events we need
         _imageViewer.Focused += _imageViewer_Focused;
         _imageViewer.PropertyChanged += _imageViewer_PropertyChanged;
         _imageViewer.ItemChanged += _imageViewer_ItemChanged;
         _imageViewer.ActiveItemChanged += _imageViewer_ActiveItemChanged;
         _imageViewer.TransformChanged += _imageViewer_TransformChanged;
         _imageViewer.PostRender += _imageViewer_PostRender;
      }

      private void Unhook()
      {
         if (_imageViewer == null)
            return;

         // Unhook from the image viewer events
         _imageViewer.Focused -= _imageViewer_Focused;
         _imageViewer.PropertyChanged -= _imageViewer_PropertyChanged;
         _imageViewer.ItemChanged -= _imageViewer_ItemChanged;
         _imageViewer.ActiveItemChanged -= _imageViewer_ActiveItemChanged;
         _imageViewer.TransformChanged -= _imageViewer_TransformChanged;
         _imageViewer.PostRender -= _imageViewer_PostRender;

         if (_automationObject != null)
            _automationObject.ActiveContainerChanged -= _automationObject_ActiveContainerChanged;
      }

      // Automation object
      private AnnAutomation _automationObject;
      public virtual object AutomationObject
      {
         get { return _automationObject; }
         set
         {
            if (_automationObject != null)
               _automationObject.ActiveContainerChanged -= _automationObject_ActiveContainerChanged;

            _automationObject = value as AnnAutomation;

            if (_automationObject != null)
               _automationObject.ActiveContainerChanged += _automationObject_ActiveContainerChanged;
         }
      }

      // Pointer events
      public event EventHandler<AnnPointerEventArgs> AutomationPointerDown;
      public event EventHandler<AnnPointerEventArgs> AutomationPointerMove;
      public event EventHandler<AnnPointerEventArgs> AutomationPointerUp;
      public event EventHandler<AnnPointerEventArgs> AutomationDoubleClick;

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
            return 96.0;
         }
      }

      public virtual double AutomationDpiY
      {
         get
         {
            return 96.0;
         }
      }

      // Enabled/Focus
      public virtual bool AutomationEnabled
      {
         get
         {
            return _imageViewer != null && _imageViewer.IsEnabled;
         }
      }

      
      private void _imageViewer_EnabledChanged(object sender, EventArgs e)
      {
         if (this.AutomationEnabledChanged != null)
            this.AutomationEnabledChanged(this, EventArgs.Empty);
      }

      public event EventHandler AutomationGotFocus;
      public event EventHandler AutomationLostFocus;
      private void _imageViewer_Focused(object sender, FocusEventArgs e)
      {
         if (e.IsFocused)
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


      // Automation items properties

      public event EventHandler AutomationSizeChanged;
      private void _imageViewer_ItemChanged(object sender, ImageViewerItemChangedEventArgs e)
      {
         switch (e.Reason)
         {
            case ImageViewerItemChangedReason.Url:
            case ImageViewerItemChangedReason.Image:
            case ImageViewerItemChangedReason.ImageChanged:
            case ImageViewerItemChangedReason.Size:
            case ImageViewerItemChangedReason.Transform:
            case ImageViewerItemChangedReason.Visibility:
               _needsImageViewerItemChanged = true;
               TryImageViewerItemChanged();
               break;

            default:
               break;
         }
      }

      // Use Begin/End ImageViewerItemChanges when bulk resizing/changing items to prevent O(n^2) behavior.
      private int _imageViewerItemChangesCounter = 0;
      private bool _needsImageViewerItemChanged = false;

      internal void BeginImageViewerItemChanges()
      {
         _imageViewerItemChangesCounter++;
      }

      internal void EndImageViewerItemChanges()
      {
         if (_imageViewerItemChangesCounter == 0) throw new InvalidOperationException("'EndImageViewerItemChanges' called without a matching 'BeginImageViewerItemChanges'");
         _imageViewerItemChangesCounter--;

         TryImageViewerItemChanged();
      }

      private void TryImageViewerItemChanged()
      {
         if (_imageViewerItemChangesCounter == 0 && _needsImageViewerItemChanged)
         {
            _needsImageViewerItemChanged = false;
            // Let the automation know the size of item has changed
            if (this.AutomationTransformChanged != null)
               this.AutomationTransformChanged(this, EventArgs.Empty);
            if (this.AutomationSizeChanged != null)
               this.AutomationSizeChanged(this, EventArgs.Empty);
         }
      }

      private void _imageViewer_ActiveItemChanged(object sender, EventArgs e)
      {
         SyncActiveItemContainer(true);
      }

      private void _automationObject_ActiveContainerChanged(object sender, EventArgs e)
      {
         SyncActiveItemContainer(false);
      }

      private void SyncActiveItemContainer(bool fromViewer)
      {
         // Ensure that both the image viewer and automation active "item" is the same
         if (_automationObject == null || _imageViewer == null)
            return;

         var itemsCount = _imageViewer.Items.Count;
         var containersCount = _automationObject.Containers.Count;
         if (itemsCount == 0 || itemsCount != containersCount)
            return;

         var imageViewerIndex = _imageViewer.Items.IndexOf(_imageViewer.ActiveItem);
         int containerIndex = -1;
         if (_automationObject.ActiveContainer != null)
            containerIndex = _automationObject.Containers.IndexOf(_automationObject.ActiveContainer);

         if (imageViewerIndex != containerIndex)
         {
            if (fromViewer)
            {
               if (imageViewerIndex != -1)
                  _automationObject.ActiveContainer = _automationObject.Containers[imageViewerIndex];
            }
            else
            {
               if (containerIndex != -1)
                  _imageViewer.ActiveItem = _imageViewer.Items[containerIndex];
            }
         }
      }

      private ImageViewerItem GetItemForCurrentContainer()
      {
         if (_imageViewer == null)
            return null;

         // Multiple container support?
         if (_containerIndex != -1)
         {
            // Yes, get the item
            switch (this.MultiContainerMode)
            {
               case AutomationControlMultiContainerMode.MultiPage:
                  // One container for each item
                  // Sanity check
                  if (_containerIndex >= 0 && _containerIndex < _imageViewer.Items.Count)
                     return _imageViewer.Items[_containerIndex];

                  return null;

               case AutomationControlMultiContainerMode.SinglePage:
               default:
                  // All containers belong to the first item
                  return _imageViewer.ActiveItem;
            }
         }
         else
         {
            // No, active item
            return _imageViewer.ActiveItem;
         }
      }


      private UpdateAutomationTransformCallback _updateAutomationTransform;
      public UpdateAutomationTransformCallback UpdateAutomationTransform
      {
         get { return _updateAutomationTransform; }
         set { _updateAutomationTransform = value; }
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
               var bounds = _imageViewer.GetItemBounds(item, ImageViewerItemPart.Item);
               if (bounds.IsEmpty)
               {
                  // Make it go outside the screen
                  var matrix = LeadMatrix.Identity;
                  matrix.OffsetX = -100000;
                  matrix.OffsetY = -100000;
                  return matrix;
               }

               var transform = _imageViewer.GetItemImageTransformWithDpi(item, false);

               if (_updateAutomationTransform != null)
                  transform = _updateAutomationTransform(item, transform);

               return transform;
            }
            else
               return LeadMatrix.Identity;
         }
      }

      public event EventHandler AutomationTransformChanged;
      private void _imageViewer_TransformChanged(object sender, EventArgs e)
      {
         // Let the automation know
         if (this.AutomationTransformChanged != null)
            this.AutomationTransformChanged(this, EventArgs.Empty);
      }

      public virtual bool AutomationUseDpi
      {
         get
         {
            return _imageViewer != null && _imageViewer.UseDpi;
         }
      }

      public event EventHandler AutomationUseDpiChanged;
      public event EventHandler AutomationEnabledChanged;
      private void _imageViewer_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         switch (e.PropertyName)
         {
            case "UseDpi":
               if (this.AutomationUseDpiChanged != null)
                  this.AutomationUseDpiChanged(this, EventArgs.Empty);
               break;
            case "IsEnabled":
               if (this.AutomationEnabledChanged != null)
                  this.AutomationEnabledChanged(this, EventArgs.Empty);
               break;

            default:
               break;
         }
      }

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

      // Rendering
      public virtual void AutomationInvalidate(LeadRectD rc)
      {
         // Invalidate the viewer
         if (_imageViewer != null)
         {
            _imageViewer.InvalidateImageSurface(rc);
         }
      }

      // Turn anti aliasing on and off
      private bool _antiAlias;
      public virtual bool AutomationAntiAlias
      {
         get
         {
            return _antiAlias;
         }
         set
         {
            _antiAlias = value;

            AutomationInvalidate(LeadRectD.Empty);
         }
      }

      private AnnRenderingEngine _renderingEngine;
      public virtual AnnRenderingEngine RenderingEngine
      {
         get
         {
            return _renderingEngine;
         }
         set
         {
            _renderingEngine = value;
         }
      }

      private void _imageViewer_PostRender(object sender, ImageViewerRenderEventArgs e)
      {
         // Do we have a rendering engine?
         var renderingEngine = this.RenderingEngine as AnnDrawRenderingEngine;
         if (renderingEngine == null)
            return;

         bool runMode = false;
         if (_automationObject != null && _automationObject.Manager != null)
            runMode = (_automationObject.Manager.UserMode == AnnUserMode.Run);

         try
         {
            // Do we have multiple containers?
            if (_getContainersCallback != null)
            {
               // Yes, get the container for this item
               var containers = _getContainersCallback();

               if (containers != null)
               {
                  switch (this.MultiContainerMode)
                  {
                     case AutomationControlMultiContainerMode.MultiPage:
                        // Each container belong to an item
                        for (var index = 0; index < containers.Count; index++)
                        {
                           if (index < _imageViewer.Items.Count)
                           {
                              var itemBounds = _imageViewer.GetItemBounds(_imageViewer.Items[index], ImageViewerItemPart.Item);
                              if (!itemBounds.IsEmpty)
                              {
                                 var container = containers[index];
                                 var item = _imageViewer.Items[index];
                                 var containerBounds = _automationObject.GetContainerInvalidRect(container, true);

                                 if (!_imageViewer.GetItemViewBounds(item, ImageViewerItemPart.Item, true).IsEmpty)
                                    RenderContainer(e, renderingEngine, container, runMode);
                              }
                           }
                        }
                        break;

                     case AutomationControlMultiContainerMode.SinglePage:
                     default:
                        // All containers belong to the active item
                        ImageViewerItem activeItem = _imageViewer.ActiveItem;

                        if (activeItem != null &&
                           !_imageViewer.GetItemViewBounds(activeItem, ImageViewerItemPart.Item, true).IsEmpty)
                        {
                           for (var index = 0; index < containers.Count; index++)
                           {
                              var itemBounds = _imageViewer.GetItemBounds(activeItem, ImageViewerItemPart.Item);
                              if (!itemBounds.IsEmpty)
                              {
                                 var container = containers[index];
                                 RenderContainer(e, renderingEngine, container, runMode);
                              }
                           }
                        }
                        break;
                  }
               }
            }
            else
            {
               // Using single-containers, just render the one the user set
               var container = this._container;
               if (container != null)
                  RenderContainer(e, renderingEngine, container, runMode);
            }
         }
         finally
         {
         }
      }

      private static void RenderContainer(ImageViewerRenderEventArgs e, AnnRenderingEngine renderingEngine, AnnContainer container, bool runMode)
      {
         object surface = e.SurfaceContext;

         if (surface == null)
            return;

         // Attach to the current container and context
         renderingEngine.Attach(container, surface);

         try
         {
            // Render the annotations
            renderingEngine.Render(LeadRectD.Empty, runMode);
         }
         finally
         {
            renderingEngine.Detach();
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

      private int _containerIndex = -1;
      public virtual int AutomationContainerIndex
      {
         get { return _containerIndex; }
         set { _containerIndex = value; }
      }

      // Single container support
      private AnnContainer _container;
      public virtual void AutomationAttach(AnnContainer container)
      {
         _container = container;
      }

      public virtual void AutomationDetach()
      {
         _container = null;
      }

      // Data provider for the images
      private AnnDataProvider _dataProvider;
      public virtual AnnDataProvider AutomationDataProvider
      {
         get { return _dataProvider; }
         set { _dataProvider = value; }
      }

      // Scroll Offset values for viewer
      public virtual LeadPoint AutomationScrollOffset
      {
         get { return _imageViewer != null ? _imageViewer.ScrollOffset : LeadPoint.Create(0, 0); }
      }

      public virtual double AutomationRotateAngle
      {
         get { return _imageViewer != null ? _imageViewer.RotateAngle : 0; }
      }

      public virtual double AutomationScaleFactor
      {
         get { return _imageViewer != null ? _imageViewer.ScaleFactor : 1; }
      }

      private bool _isAutomationEventsHooked;
      public virtual bool IsAutomationEventsHooked
      {
         get { return _isAutomationEventsHooked; }
         set { _isAutomationEventsHooked = value; }
      }
   }
}
