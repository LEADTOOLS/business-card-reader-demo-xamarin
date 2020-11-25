// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Annotations.Automation;
using Leadtools.Annotations.Engine;
using Leadtools.Annotations.Rendering;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using System;
using Xamarin.Forms;

namespace Leadtools.Annotations.Xamarin
{
   public class AutomationManagerHelper : IDisposable
   {
      // Adds extra data and updates to automation object
      private static void UpdateAutomationObject(AnnAutomationObject automationObject)
      {
         if (automationObject.ObjectTemplate != null && automationObject.ObjectTemplate.SupportsFill && automationObject.ObjectTemplate.Fill == null)
            automationObject.ObjectTemplate.Fill = AnnSolidColorBrush.Create("transparent");
      }

      private AutomationManagerHelper() { }

      public AutomationManagerHelper(AnnAutomationManager manager)
      {
         if (manager == null) throw new ArgumentNullException("manager");

         _manager = manager;


         // Attach new WinForms rendering engine to the manager
         if (_manager.RenderingEngine == null)
            _manager.RenderingEngine = new AnnDrawRenderingEngine();

         UpdateAutomationObjects();

         // Load the resources (images and rubber stamps)
         _manager.Resources = Tools.LoadResources();
      }

      public void UpdateAutomationObjects()
      {
         if (_manager == null)
            return;

         var automationObjects = _manager.Objects;

         foreach (var automationObject in automationObjects)
            UpdateAutomationObject(automationObject);
      }

      ~AutomationManagerHelper()
      {
         Dispose(false);
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (_manager != null)
            {
               _manager = null;
            }
         }
      }

      private AnnAutomationManager _manager;
      // Gets the attached AnnAutomationManager
      public AnnAutomationManager AutomationManager
      {
         get { return _manager; }
      }

      public void LoadPackage(IAnnPackage package)
      {
         if (package == null) throw new ArgumentNullException("package");

         // Update the automation objects as we load ...
         _manager.Objects.CollectionChanged += new EventHandler<AnnNotifyCollectionChangedEventArgs>(Objects_CollectionChanged);
         try
         {
            _manager.LoadPackage(package, package.ToString());
         }
         finally
         {
            _manager.Objects.CollectionChanged -= new EventHandler<AnnNotifyCollectionChangedEventArgs>(Objects_CollectionChanged);
         }
      }

      private void Objects_CollectionChanged(object sender, AnnNotifyCollectionChangedEventArgs e)
      {
         foreach (AnnAutomationObject automationObject in e.NewItems)
         {
            UpdateAutomationObject(automationObject);
         }
      }

      private enum ToolbarAnnObjectIds
      {
         SelectObjectId = -1,
         LineObjectId = -2,
         RectangleObjectId = -3,
         EllipseObjectId = -4,
         PolylineObjectId = -5,
         PolygonObjectId = -6,
         CurveObjectId = -7,
         ClosedCurveObjectId = -8,
         PointerObjectId = -9,
         FreehandObjectId = -10,
         HiliteObjectId = -11,
         TextObjectId = -12,
         TextRollupObjectId = -13,
         TextPointerObjectId = -14,
         NoteObjectId = -15,
         StampObjectId = -16,
         RubberStampObjectId = -17,
         HotspotObjectId = -18,
         FreehandHotspotObjectId = -19,
         PointObjectId = -21,
         RedactionObjectId = -22,
         RulerObjectId = -23,
         PolyRulerObjectId = -24,
         ProtractorObjectId = -25,
         CrossProductObjectId = -26,
         EncryptObjectId = -27,
         AudioObjectId = -28,
         MediaObjectId = -30,
         StickyNoteObjectId = -32,
         TextHiliteObjectId = -33,
         TextStrikeoutObjectId = -34,
         TextUnderlineObjectId = -35,
         TextRedactionObjectId = -36,
      }

      public class AnnObjectSelectedEventArgs : EventArgs
      {
         private string _objectId = "-1";
         public string ObjectId
         {
            get => _objectId;
            set => _objectId = value;
         }

         public AnnObjectSelectedEventArgs(string objectId)
         {
            _objectId = objectId;
         }
      }

      public FlexLayout ToolBar { get; private set; }
      public delegate void OnAnnObjectSelected(SvgImage sender, AnnObjectSelectedEventArgs args);
      public event OnAnnObjectSelected AnnObjectSelected;

      // Creat a toolbar for each automation object.
      public void CreateToolBar()
      {
         if (ToolBar != null)
            ToolBar.Children.Clear();

         ToolBar = new FlexLayout();
         ToolBar.Wrap = FlexWrap.Wrap;
         ToolBar.AlignItems = FlexAlignItems.Start;
         ToolBar.AlignContent = FlexAlignContent.Start;
         ToolBar.JustifyContent = FlexJustify.Start;
         ToolBar.Padding = new Thickness(GlobalMarginExtension.UnitSize * 0.5);

         SvgImage[] toolbarButtons = 
         {
            new SvgImage() { ResourceName = "Annotations/select-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.SelectObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/line-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.LineObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/rec-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.RectangleObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/ellipse-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.EllipseObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/poly-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.PolylineObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/polygon.svg", StyleId = ((int)ToolbarAnnObjectIds.PolygonObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/curve-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.CurveObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/closed-curve-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.ClosedCurveObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/pointer-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.PointerObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/freehand-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.FreehandObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/hilite-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.HiliteObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/text-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/text-rollup-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextRollupObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/text-pointer-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextPointerObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/note-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.NoteObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/stamp-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.StampObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/rubber-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.RubberStampObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/hotspot-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.HotspotObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/freehand-hot-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.FreehandHotspotObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/point-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.PointObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/ruler-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.RulerObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/polyruler-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.PolyRulerObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/protractor-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.ProtractorObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/cross-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.CrossProductObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/redaction-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.RedactionObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/encrypt-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.EncryptObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/audio-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.AudioObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/media-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.MediaObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/sticky-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.StickyNoteObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/text-hilite-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextHiliteObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/strikeout-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextStrikeoutObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/underline-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextUnderlineObjectId).ToString() },
            new SvgImage() { ResourceName = "Annotations/text-redact-ann.svg", StyleId = ((int)ToolbarAnnObjectIds.TextRedactionObjectId).ToString() }
         };

         foreach (SvgImage toolBarButtonImage in toolbarButtons)
         {
            AddObjectToToolBar(toolBarButtonImage);
         }
      }

      private void AddObjectToToolBar(SvgImage toolBarButtonImage)
      {
         toolBarButtonImage.WidthRequest = 40;
         toolBarButtonImage.HeightRequest = 40;
         toolBarButtonImage.Margin = new Thickness(5);
         TapGestureRecognizer annButtonTapped = new TapGestureRecognizer();
         annButtonTapped.Tapped += AnnObjectButton_Tapped;
         toolBarButtonImage.GestureRecognizers.Add(annButtonTapped);
         ToolBar.Children.Add(toolBarButtonImage);
      }

      private void AnnObjectButton_Tapped(object sender, EventArgs e)
      {
         SvgImage toolBarButtonImage = sender as SvgImage;
         AnnObjectSelected?.Invoke(toolBarButtonImage, new AnnObjectSelectedEventArgs(toolBarButtonImage.StyleId));
      }
   }
}