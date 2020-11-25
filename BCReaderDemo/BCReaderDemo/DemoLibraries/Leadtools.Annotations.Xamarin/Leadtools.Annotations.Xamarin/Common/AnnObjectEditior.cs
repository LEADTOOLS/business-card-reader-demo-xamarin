// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;

using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.Xamarin
{
   public delegate void AnnObjectEditorValueChangedHandler(object oldValue, object newValue);
   public delegate void AnnObjectEditorPropertyChangedHandler(string propertyName, object newValue);
   public interface IAnnEditor
   {
      string Category
      {
         get;
      }

      Dictionary<string, AnnPropertyInfo> Properties
      {
         get;
      }

      event AnnObjectEditorValueChangedHandler OnValueChanged;
   }

   public class AnnColorEditor : IAnnEditor
   {
      public event AnnObjectEditorValueChangedHandler OnValueChanged;

      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      public AnnColorEditor(string color, string category)
      {
         _category = category;
         Value = color;
      }

      object _color;

      public object Value
      {
         get { return _color; }
         set
         {
            if (OnValueChanged != null)
               OnValueChanged(_color, value);

            _color = value;
         }
      }
   }

   public class AnnBooleanEditor : IAnnEditor
   {
      bool _value;

      public bool Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
               OnValueChanged(_value, value);

            _value = value;
         }
      }

      string _category;
      public string Category
      {
         get { return _category; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      public AnnBooleanEditor(bool value, string category)
      {
         _value = value;
         _category = category;
      }

      #region IAnnEditor Members


      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return null; }
      }

      #endregion
   }

   public class AnnLengthEditor : IAnnEditor
   {
      LeadLengthD _annLength;
      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      string _category;

      public string Category
      {
         get { return _category; }
      }

      public AnnLengthEditor(LeadLengthD annLength, string category, string propertyName, string displayName)
      {
         _annLength = annLength;
         _category = category;
         AnnPropertyInfo info = new AnnPropertyInfo(propertyName, false, annLength.Value, category, "Length", displayName, true, typeof(AnnDoubleEditor));
         info.ValueChanged += new AnnObjectEditorValueChangedHandler(info_ValueChanged);
         _properties[propertyName] = info;
      }

      void info_ValueChanged(object oldValue, object newValue)
      {
         _annLength.Value = (double)newValue;
         OnValueChanged(oldValue, newValue);
      }

      private Dictionary<string, AnnPropertyInfo> _properties = new Dictionary<string, AnnPropertyInfo>();
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }
   }

   public class AnnSolidColorBrushEditor : IAnnEditor
   {
      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      string _category;

      public string Category
      {
         get { return _category; }
      }

      private AnnSolidColorBrush _annSolidColorBrush = null;
      public AnnSolidColorBrushEditor(AnnSolidColorBrush annSolidColorBrush, string category, string propertyName, string displayName)
      {
         _annSolidColorBrush = annSolidColorBrush;
         _category = category;

         AnnPropertyInfo info;
         if (annSolidColorBrush != null)
         {
            info = new AnnPropertyInfo(propertyName, false, annSolidColorBrush.Color, category, "Color", displayName, true, typeof(AnnColorEditor));
         }
         else
         {
            info = new AnnPropertyInfo(propertyName, false, "transparent", category, "Color", displayName, true, typeof(AnnColorEditor));
         }

         info.ValueChanged += new AnnObjectEditorValueChangedHandler(info_ValueChanged);
         _properties[propertyName] = info;
      }

      void info_ValueChanged(object oldValue, object newValue)
      {
         if (_annSolidColorBrush != null)
         {
            _annSolidColorBrush.Color = newValue as string;
         }
         if (OnValueChanged != null)
         {
            OnValueChanged(oldValue, newValue);
         }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = new Dictionary<string, AnnPropertyInfo>();
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }
   }

   public class AnnDoubleEditor : IAnnEditor
   {
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      string _category;

      public string Category
      {
         get { return _category; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public AnnDoubleEditor(double value, string category)
      {
         _category = category;
         Value = value;
      }

      double _value = 0;

      public double Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
            {
               OnValueChanged(_value, value);
            }

            _value = value;
         }
      }
   }

   public class AnnStringEditor : IAnnEditor
   {
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;

      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public AnnStringEditor(string value, string category)
      {
         _category = category;
         Value = value;
      }

      string _value = string.Empty;

      public string Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
            {
               OnValueChanged(_value, value);
            }

            _value = value;
         }
      }
   }

   public class AnnPictureEditor : IAnnEditor
   {
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;

      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public AnnPictureEditor(AnnPicture value, string category)
      {
         _category = category;
         Value = value;
      }

      AnnPicture _value = AnnPicture.Empty;

      public AnnPicture Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
            {
               OnValueChanged(_value, value);
            }

            _value = value;
         }
      }
   }

   public class AnnMediaEditor : IAnnEditor
   {
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;

      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public AnnMediaEditor(AnnMedia value, string category)
      {
         _category = category;
         Value = value;
      }

      AnnMedia _value = new AnnMedia();

      public AnnMedia Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
            {
               OnValueChanged(_value, value);
            }

            _value = value;
         }
      }
   }

   public class AnnIntegerEditor : IAnnEditor
   {
      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      string _category;

      public string Category
      {
         get { return _category; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      private Dictionary<string, AnnPropertyInfo> _properties = null;

      public AnnIntegerEditor(int value, string category)
      {
         _category = category;
         Value = value;
      }

      int _value = 0;

      public int Value
      {
         get { return _value; }
         set
         {
            if (OnValueChanged != null)
            {
               OnValueChanged(_value, value);
            }

            _value = value;
         }
      }
   }

   public class AnnStrokeEditor : IAnnEditor
   {
      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = new Dictionary<string, AnnPropertyInfo>();

      AnnStroke _annStroke;
      public AnnStrokeEditor(AnnStroke annStroke, string category)
      {
         _category = category;
         AnnPropertyInfo strokePropertyInfo = new AnnPropertyInfo("Stroke", false, annStroke.Stroke, category, "Stroke", string.Empty, true, typeof(AnnSolidColorBrushEditor));
         strokePropertyInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(strokePropertyInfo_ValueChanged);
         _properties["Stroke"] = strokePropertyInfo;
         AnnPropertyInfo thicknessPropertyInfo = new AnnPropertyInfo("Thickness", false, annStroke.StrokeThickness, category, "Thickness", string.Empty, true, typeof(AnnLengthEditor));
         _annStroke = annStroke;
         thicknessPropertyInfo.ValueChanged += thicknessPropertyInfo_ValueChanged;
         _properties["Thickness"] = thicknessPropertyInfo;
      }

      private void thicknessPropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         _annStroke.StrokeThickness = LeadLengthD.Create((double)newValue);

         if (OnValueChanged != null)
         {
            OnValueChanged(oldValue, newValue);
         }
      }

      void strokePropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         _annStroke.Stroke = AnnSolidColorBrush.Create((string)newValue);
         if (OnValueChanged != null)
         {
            OnValueChanged(oldValue, newValue);
         }
      }

      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      public event AnnObjectEditorValueChangedHandler OnValueChanged;
   }

   public class AnnFontEditor : IAnnEditor
   {
      public event AnnObjectEditorValueChangedHandler OnValueChanged;
      string _category;

      public string Category
      {
         get { return _category; }
      }

      private Dictionary<string, AnnPropertyInfo> _properties = new Dictionary<string, AnnPropertyInfo>();

      AnnFont _annFont = null;
      public AnnFontEditor(AnnFont annFont, string category)
      {
         _category = category;
         _annFont = annFont;

         AnnPropertyInfo fontFamilyNameInfo = new AnnPropertyInfo(string.Empty, false, annFont.FontFamilyName, category, "Stroke", "Family Name", true, typeof(AnnStringEditor));
         fontFamilyNameInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(fontFamilyName_ValueChanged);

         fontFamilyNameInfo.Values["Arial"] = "Arial";
         fontFamilyNameInfo.Values["Courier New"] = "Courier New";
         fontFamilyNameInfo.Values["Times New Roman"] = "Times New Roman";
         fontFamilyNameInfo.Values["Verdana"] = "Verdana";

         AnnPropertyInfo fontSize = new AnnPropertyInfo(string.Empty, false, annFont.FontSize, category, "FontSize", "Size in Points", true, typeof(AnnDoubleEditor));
         fontSize.Values[" 8"] = 8;
         fontSize.Values["10"] = 10;
         fontSize.Values["11"] = 11;
         fontSize.Values["12"] = 12;
         fontSize.Values["13"] = 13;
         fontSize.Values["14"] = 14;
         fontSize.Values["16"] = 16;
         fontSize.Values["18"] = 18;
         fontSize.Values["20"] = 20;
         fontSize.ValueChanged += new AnnObjectEditorValueChangedHandler(fontSize_ValueChanged);

         _properties["FontFamilyName"] = fontFamilyNameInfo;
         _properties["FontSize"] = fontSize;
      }

      void fontSize_ValueChanged(object oldValue, object newValue)
      {
         _annFont.FontSize = (double)newValue;
         OnValueChanged(oldValue, newValue);
      }

      void fontFamilyName_ValueChanged(object oldValue, object newValue)
      {
         _annFont.FontFamilyName = (string)newValue;
         OnValueChanged(oldValue, newValue);
      }

      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }
   }

   public class AnnObjectEditor
   {
      public event AnnObjectEditorPropertyChangedHandler OnPropertyChanged;
      private Dictionary<string, AnnPropertyInfo> _properties = new Dictionary<string, AnnPropertyInfo>();

      public Dictionary<string, AnnPropertyInfo> Properties
      {
         get { return _properties; }
      }

      AnnObject _annObject;
      AnnPolyRulerObject _polyRulerObj;
      public AnnObjectEditor(AnnObject annObject)
      {
         _annObject = annObject;
         if (annObject.SupportsFill && !(annObject is AnnHotspotObject))
         {
            AnnBrush fill = null;
            AnnHiliteObject annHiliteObject = _annObject as AnnHiliteObject;
            if (annHiliteObject != null)
            {
               fill = AnnSolidColorBrush.Create(annHiliteObject.HiliteColor);
            }
            else
            {
               fill = annObject.Fill;
            }

            AnnPropertyInfo fillPropertyInfo = new AnnPropertyInfo("Color", false, fill, "Fill", "Hilite Color", string.Empty, annObject.SupportsFill, typeof(AnnSolidColorBrushEditor));
            fillPropertyInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(fillPropertyInfo_ValueChanged);

            _properties["Color"] = fillPropertyInfo;
         }

         if (annObject.SupportsStroke && !(annObject is AnnHotspotObject))
         {
            AnnPropertyInfo strokePropertyInfo = new AnnPropertyInfo("Stroke", false, annObject.Stroke, "Stroke", "Stroke the object", string.Empty, annObject.SupportsStroke, typeof(AnnStrokeEditor));
            strokePropertyInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(strokePropertyInfo_ValueChanged);

            _properties["Stroke"] = strokePropertyInfo;
         }

         if (annObject.SupportsFont)
         {
            AnnPropertyInfo fontPropertyInfo = new AnnPropertyInfo(string.Empty, false, annObject.Font, "Font", "Select Font", string.Empty, annObject.SupportsFont, typeof(AnnFontEditor));
            fontPropertyInfo.ValueChanged += fontPropertyInfo_ValueChanged;
            _properties["Font"] = fontPropertyInfo;
         }

         AnnPropertyInfo hyperlink = new AnnPropertyInfo(string.Empty, false, annObject.Hyperlink, "Hyperlink", "Hyperlink", "Hyperlink", true, typeof(AnnStringEditor));
         hyperlink.ValueChanged += new AnnObjectEditorValueChangedHandler(hyperlink_ValueChanged);
         _properties["Hyperlink"] = hyperlink;

         if (annObject.SupportsOpacity)
         {
            AnnPropertyInfo opacity = new AnnPropertyInfo(string.Empty, false, annObject.Opacity, "Opacity", "Opacity", "Opacity", true, typeof(AnnDoubleEditor));
            opacity.ValueChanged += opacity_ValueChanged;
            _properties["Opacity"] = opacity;
         }

         if (annObject is AnnCurveObject)
         {
            AnnCurveObject closedCurveObject = annObject as AnnCurveObject;
            AnnPropertyInfo tensionInfo;
            if (closedCurveObject != null)
            {
               tensionInfo = new AnnPropertyInfo(string.Empty, false, closedCurveObject.Tension, "Curve", "Tension", "Tension", true, typeof(AnnDoubleEditor));
            }
            else
            {
               AnnCurveObject curveObject = annObject as AnnCurveObject;
               tensionInfo = new AnnPropertyInfo(string.Empty, false, curveObject.Tension, "Curve", "Tension", "Tension", true, typeof(AnnDoubleEditor));
            }

            tensionInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(tensionInfo_ValueChanged);
            _properties["Tension"] = tensionInfo;

         }

         if (annObject is AnnPolyRulerObject)
         {
            AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;

            AnnPropertyInfo showGauge = new AnnPropertyInfo(string.Empty, false, annPolyRulerObject.ShowGauge, "Ruler", "ShowGauge", "Show Gauge", true, typeof(AnnBooleanEditor));

            showGauge.Values["True"] = true;
            showGauge.Values["False"] = false;

            showGauge.ValueChanged += new AnnObjectEditorValueChangedHandler(showGauge_ValueChanged);
            _properties["ShowGauge"] = showGauge;

            AnnPropertyInfo gaugeLengthInfo = new AnnPropertyInfo("GaugeLength", false, annPolyRulerObject.GaugeLength, "Ruler", "GaugeLength", "Gauge Length", true, typeof(AnnLengthEditor));
            _properties["GaugeLength"] = gaugeLengthInfo;

            AnnPropertyInfo tickMarksLengthInfo = new AnnPropertyInfo("TickMarksLength", false, annPolyRulerObject.TickMarksLength, "Ruler", "TickMarksLength", "TickMarks Length", true, typeof(AnnLengthEditor));
            _properties["TickMarksLength"] = tickMarksLengthInfo;

            _polyRulerObj = annPolyRulerObject;
            gaugeLengthInfo.ValueChanged += gaugeLengthInfo_ValueChanged;
            tickMarksLengthInfo.ValueChanged += tickMarksLengthInfo_ValueChanged;

            string measurementUnit = Enum.GetName(typeof(AnnUnit), (int)annPolyRulerObject.MeasurementUnit);
            AnnPropertyInfo measurementUnitInfo = new AnnPropertyInfo(string.Empty, false, measurementUnit, "Ruler", "MeasurementUnit", "Measurement Unit", true, typeof(AnnStringEditor));
            measurementUnitInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(measurementUnitInfo_ValueChanged);

            FillEnumValue(measurementUnitInfo, typeof(AnnUnit));
            _properties["MeasurementUnit"] = measurementUnitInfo;

            AnnPropertyInfo precisionInfo = new AnnPropertyInfo(string.Empty, false, annPolyRulerObject.Precision.ToString(), "Ruler", "Precision", "Precision", true, typeof(AnnStringEditor));
            precisionInfo.Values["0"] = 0;
            precisionInfo.Values["1"] = 1;
            precisionInfo.Values["2"] = 2;
            precisionInfo.Values["3"] = 3;
            precisionInfo.Values["4"] = 4;
            precisionInfo.Values["5"] = 5;
            precisionInfo.Values["6"] = 6;

            precisionInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(precisionInfo_ValueChanged);
            _properties["Precision"] = precisionInfo;


            AnnPropertyInfo showTickMarksInfo = new AnnPropertyInfo(string.Empty, false, annPolyRulerObject.ShowTickMarks, "Ruler", "ShowTickMarks", "Show Tick Marks", true, typeof(AnnBooleanEditor));

            showTickMarksInfo.Values["True"] = true;
            showTickMarksInfo.Values["False"] = false;

            showTickMarksInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(showTickMarksInfo_ValueChanged);
            _properties["ShowTickMarks"] = showTickMarksInfo;

         }

         if (annObject is AnnProtractorObject)
         {
            AnnProtractorObject annProtractorObject = annObject as AnnProtractorObject;
            AnnPropertyInfo acuteInfo = new AnnPropertyInfo(string.Empty, false, annProtractorObject.Acute.ToString(), "Protractor", "Acute", "Acute", true, typeof(AnnStringEditor));
            acuteInfo.Values["True"] = true;
            acuteInfo.Values["False"] = false;

            acuteInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(acuteInfo_ValueChanged);
            _properties["Acute"] = acuteInfo;
            AnnPropertyInfo anglePrecisionInfo = new AnnPropertyInfo(string.Empty, false, annProtractorObject.AnglePrecision.ToString(), "Protractor", "Angle Precision", "Precision", true, typeof(AnnStringEditor));

            anglePrecisionInfo.Values["0"] = 0;
            anglePrecisionInfo.Values["1"] = 1;
            anglePrecisionInfo.Values["2"] = 2;
            anglePrecisionInfo.Values["3"] = 3;
            anglePrecisionInfo.Values["4"] = 4;
            anglePrecisionInfo.Values["5"] = 5;
            anglePrecisionInfo.Values["6"] = 6;

            anglePrecisionInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(anglePrecisionInfo_ValueChanged);
            _properties["AnglePrecision"] = anglePrecisionInfo;

            string angulartUnit = Enum.GetName(typeof(AnnAngularUnit), (int)annProtractorObject.AngularUnit);
            AnnPropertyInfo angularUnitInfo = new AnnPropertyInfo(string.Empty, false, angulartUnit, "Protractor", "AngularUnit", "Angular Unit", true, typeof(AnnStringEditor));
            angularUnitInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(angularUnitInfo_ValueChanged);

            FillEnumValue(angularUnitInfo, typeof(AnnAngularUnit));
            _properties["AngularUnit"] = angularUnitInfo;
         }
         if (annObject is AnnRubberStampObject)
         {
            AnnRubberStampObject annRubberStampObject = annObject as AnnRubberStampObject;
            string rubberStamp = Enum.GetName(typeof(AnnRubberStampType), (int)annRubberStampObject.RubberStampType);
            if (rubberStamp.StartsWith("Stamp"))
            {
               rubberStamp = rubberStamp.Replace("Stamp", "");
            }
            AnnPropertyInfo ruberStampTypeinfo = new AnnPropertyInfo(string.Empty, false, rubberStamp, "Rubber Stamp", "RuberStampType", "Rubber Stamp Type", true, typeof(AnnStringEditor));

            FillEnumValue(ruberStampTypeinfo, typeof(AnnRubberStampType));

            ruberStampTypeinfo.ValueChanged += new AnnObjectEditorValueChangedHandler(ruberStampTypeinfo_ValueChanged);
            _properties["RuberStampType"] = ruberStampTypeinfo;
         }

         if (annObject is AnnTextObject)
         {
            AnnTextObject annTextObject = annObject as AnnTextObject;
            AnnPropertyInfo textBackgroundPropertyInfo = new AnnPropertyInfo(string.Empty, false, annTextObject.TextBackground, "Text", "TextBackground", "Background", true, typeof(AnnSolidColorBrushEditor));
            _properties["TextBackground"] = textBackgroundPropertyInfo;
            textBackgroundPropertyInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(textBackgroundPropertyInfo_ValueChanged);

            AnnPropertyInfo textForegroundPropertyInfo = new AnnPropertyInfo(string.Empty, false, annTextObject.TextForeground, "Text", "TextForeground", "Foreground", true, typeof(AnnSolidColorBrushEditor));
            textForegroundPropertyInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(textForegroundPropertyInfo_ValueChanged);
            _properties["TextForeground"] = textForegroundPropertyInfo;

            AnnPropertyInfo text = new AnnPropertyInfo(string.Empty, false, annTextObject.Text, "Text", "Acute", "Text", true, typeof(AnnStringEditor));
            text.ValueChanged += new AnnObjectEditorValueChangedHandler(text_ValueChanged);
            _properties["Text"] = text;

            string vertical = Enum.GetName(typeof(AnnVerticalAlignment), (int)annTextObject.VerticalAlignment);
            AnnPropertyInfo verticalAlignment = new AnnPropertyInfo(string.Empty, false, vertical, "Text", "VerticalAlignment", "Vertical Alignment", true, typeof(AnnStringEditor));
            verticalAlignment.ValueChanged += new AnnObjectEditorValueChangedHandler(verticalAlignment_ValueChanged);
            FillEnumValue(verticalAlignment, typeof(AnnVerticalAlignment));
            _properties["VerticalAlignment"] = verticalAlignment;


            string horizontal = Enum.GetName(typeof(AnnHorizontalAlignment), (int)annTextObject.HorizontalAlignment);
            AnnPropertyInfo horizontalAlignment = new AnnPropertyInfo(string.Empty, false, horizontal, "Text", "HorizontalAlignment", "Horizontal Alignment", true, typeof(AnnStringEditor));
            horizontalAlignment.ValueChanged += new AnnObjectEditorValueChangedHandler(horizontalAlignment_ValueChanged);

            FillEnumValue(horizontalAlignment, typeof(AnnHorizontalAlignment));

            _properties["HorizontalAlignment"] = horizontalAlignment;

            AnnPropertyInfo wordWrapInfo = new AnnPropertyInfo(string.Empty, false, annTextObject.WordWrap, "Text", "Word Wrap", "Word Wrap", true, typeof(AnnBooleanEditor));
            wordWrapInfo.Values["True"] = true;
            wordWrapInfo.Values["False"] = false;

            _properties["WordWrap"] = wordWrapInfo;

            wordWrapInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(wordWrapInfo_ValueChanged);
         }

         if (annObject is AnnTextRollupObject)
         {
            AnnTextRollupObject annTextRollupObject = annObject as AnnTextRollupObject;
            AnnPropertyInfo expandedInfo = new AnnPropertyInfo(string.Empty, false, annTextRollupObject.Expanded, "TextRollup", "Expanded", "Expanded", true, typeof(AnnBooleanEditor));

            expandedInfo.Values["True"] = true;
            expandedInfo.Values["False"] = false;

            expandedInfo.ValueChanged += expandedInfo_ValueChanged;
            _properties["Expanded"] = expandedInfo;
         }

         if (annObject is AnnTextPointerObject)
         {
            AnnTextPointerObject annTextPointerObject = annObject as AnnTextPointerObject;
            AnnPropertyInfo fixedPointerInfo = new AnnPropertyInfo(string.Empty, false, annTextPointerObject.FixedPointer.ToString(), "Text Pointer", "FixedPointer", "Fixed", true, typeof(AnnStringEditor));
            fixedPointerInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(fixedPointerInfo_ValueChanged);

            fixedPointerInfo.Values["True"] = true;
            fixedPointerInfo.Values["False"] = false;

            _properties["FixedPointer"] = fixedPointerInfo;
         }

         if (annObject is AnnStampObject)
         {
            AnnStampObject annStampObject = annObject as AnnStampObject;
            AnnPropertyInfo pictureInfo = new AnnPropertyInfo(string.Empty, false, annStampObject.Picture, "Picture", "Picture", "Picture", true, typeof(AnnPictureEditor));
            pictureInfo.ValueChanged += pictureInfo_ValueChanged;
            _properties["Picture"] = pictureInfo;
         }

         if (annObject is AnnImageObject)
         {
            AnnImageObject annImageObject = annObject as AnnImageObject;
            AnnPropertyInfo pictureInfo = new AnnPropertyInfo(string.Empty, false, annImageObject.Picture, "Picture", "Picture", "Picture", true, typeof(AnnPictureEditor));
            pictureInfo.ValueChanged += annImageObject_PictureInfo_ValueChanged;
            _properties["Picture"] = pictureInfo;
         }

         if (annObject is AnnFreehandHotspotObject)
         {
            AnnFreehandHotspotObject annFreehandHotspotObject = annObject as AnnFreehandHotspotObject;
            AnnPropertyInfo pictureInfo = new AnnPropertyInfo(string.Empty, false, annFreehandHotspotObject.Picture, "Picture", "Picture", "Picture", true, typeof(AnnPictureEditor));
            pictureInfo.ValueChanged += freeHandPictureInfo_ValueChanged;
            _properties["Picture"] = pictureInfo;
         }

         if (annObject is AnnPointObject)
         {
            AnnPointObject annPointObject = annObject as AnnPointObject;
            AnnPropertyInfo showPictureInfo = new AnnPropertyInfo(string.Empty, false, annPointObject.ShowPicture, "Point", "ShowPicture", "Show Picture", true, typeof(AnnBooleanEditor));

            showPictureInfo.Values["True"] = true;
            showPictureInfo.Values["False"] = false;

            showPictureInfo.ValueChanged += new AnnObjectEditorValueChangedHandler(showPictureInfo_ValueChanged);
            _properties["ShowPicture"] = showPictureInfo;
         }

         if (annObject.Id == AnnObject.MediaObjectId || annObject.Id == AnnObject.AudioObjectId)
         {
            AnnMediaObject annMediaObject = annObject as AnnMediaObject;

            AnnPropertyInfo media = new AnnPropertyInfo(string.Empty, false, annMediaObject.Media, "Media", "Media", "Source", true, typeof(AnnMediaEditor));

            media.ValueChanged += new AnnObjectEditorValueChangedHandler(media_ValueChanged);
            _properties["Media"] = media;
         }

         if (annObject.Id == AnnObject.EncryptObjectId)
         {
            AnnEncryptObject annEncryptObject = annObject as AnnEncryptObject;
            if (annEncryptObject != null)
            {
               AnnPropertyInfo key = new AnnPropertyInfo(string.Empty, false, annEncryptObject.Key, "Encrypt", "", "Key", true, typeof(AnnIntegerEditor));
               key.ValueChanged += new AnnObjectEditorValueChangedHandler(encryptKey_ValueChanged);
               _properties["Key"] = key;

               if (!annEncryptObject.IsEncrypted)
               {
                  AnnPropertyInfo encryptor = new AnnPropertyInfo(string.Empty, false, annEncryptObject.Encryptor, "Encrypt", "", "Encryptor", true, typeof(AnnBooleanEditor));

                  encryptor.Values["True"] = true;
                  encryptor.Values["False"] = false;
                  encryptor.ValueChanged += new AnnObjectEditorValueChangedHandler(encryptor_ValueChanged);
                  _properties["Encryptor"] = encryptor;
               }
            }
         }
      }

      void fontPropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;
         if (annTextObject != null)
         {
            AnnFont newFont = annTextObject.Font.Clone();
            string fontFamilyName = newValue as string;
            if (fontFamilyName == null)
               newFont.FontSize = double.Parse(newValue.ToString());
            else
               newFont.FontFamilyName = fontFamilyName;

            annTextObject.Font = newFont;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Font", newFont);
         }
      }

      void textForegroundPropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;
         if (annTextObject != null)
         {
            AnnBrush newBrush = AnnSolidColorBrush.Create((string)newValue);
            annTextObject.TextForeground = newBrush;

            if (OnPropertyChanged != null)
               OnPropertyChanged("TextForeground", newBrush);
         }
      }

      void textBackgroundPropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;
         if (annTextObject != null)
         {
            AnnBrush newBrush = AnnSolidColorBrush.Create((string)newValue);
            annTextObject.TextBackground = newBrush;

            if (OnPropertyChanged != null)
               OnPropertyChanged("TextBackground", newBrush);
         }
      }

      void wordWrapInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;

         if (annTextObject != null)
         {
            annTextObject.WordWrap = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("WordWrap", newValue);
         }
      }

      void gaugeLengthInfo_ValueChanged(object oldValue, object newValue)
      {
         LeadLengthD newGaugeLength = LeadLengthD.Create((double)newValue);
         _polyRulerObj.GaugeLength = newGaugeLength;

         if (OnPropertyChanged != null)
            OnPropertyChanged("GaugeLength", newGaugeLength);
      }

      void tickMarksLengthInfo_ValueChanged(object oldValue, object newValue)
      {
         LeadLengthD newTickMarksLength = LeadLengthD.Create((double)newValue);
         _polyRulerObj.TickMarksLength = newTickMarksLength;

         if (OnPropertyChanged != null)
            OnPropertyChanged("TickMarksLength", newTickMarksLength);
      }

      void pictureInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnStampObject annStampObject = (AnnStampObject)_annObject;
         if (annStampObject != null)
         {
            annStampObject.Picture = (AnnPicture)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ImagePicture", newValue);
         }
      }

      void freeHandPictureInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnFreehandHotspotObject annFreehandHotspotObject = (AnnFreehandHotspotObject)_annObject;
         if (annFreehandHotspotObject != null)
         {
            annFreehandHotspotObject.Picture = (AnnPicture)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ImagePicture", newValue);
         }
      }

      void annImageObject_PictureInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnImageObject annImageObject = (AnnImageObject)_annObject;
         if (annImageObject != null)
         {
            annImageObject.Picture = (AnnPicture)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ImagePicture", newValue);
         }
      }

      void hyperlink_ValueChanged(object oldValue, object newValue)
      {
         if (_annObject != null)
         {
            _annObject.Hyperlink = (string)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Hyperlink", newValue);
         }
      }

      void opacity_ValueChanged(object oldValue, object newValue)
      {
         if (_annObject != null)
         {
            double opacity = (double)newValue;

            //the opacity should be from 0 to 1
            if (opacity > 1)
               opacity = 1;
            else if (opacity < 0)
               opacity = 0;

            _annObject.Opacity = opacity;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Opacity", opacity);
         }
      }

      void showPictureInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnPointObject annPointObject = _annObject as AnnPointObject;

         if (annPointObject != null)
         {
            annPointObject.ShowPicture = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ShowPicture", newValue);
         }
      }

      void expandedInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextRollupObject annTextRollup = _annObject as AnnTextRollupObject;

         if (annTextRollup != null)
         {
            annTextRollup.Expanded = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Expanded", newValue);
         }
      }

      void fillPropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         if (newValue != null)
         {
            if (_annObject != null)
            {
               AnnHiliteObject annHiliteObject = _annObject as AnnHiliteObject;
               if (annHiliteObject != null)
               {
                  annHiliteObject.HiliteColor = (string)newValue;

                  if (OnPropertyChanged != null)
                     OnPropertyChanged("HiliteColor", newValue);
               }
               else
               {
                  AnnBrush newFill = AnnSolidColorBrush.Create((string)newValue);
                  _annObject.Fill = newFill;

                  if (OnPropertyChanged != null)
                     OnPropertyChanged("Fill", newFill);
               }
            }
         }
      }

      void strokePropertyInfo_ValueChanged(object oldValue, object newValue)
      {
         string color = newValue as string;
         AnnStroke newStroke = null;

         newStroke = _annObject.Stroke.Clone();
         if (color == null)
            newStroke.StrokeThickness = LeadLengthD.Create(double.Parse(newValue.ToString()));
         else
            newStroke.Stroke = AnnSolidColorBrush.Create(color);

         _annObject.Stroke = newStroke.Clone();

         if (OnPropertyChanged != null)
            OnPropertyChanged("Stroke", newStroke);
      }

      void ruberStampTypeinfo_ValueChanged(object oldValue, object newValue)
      {
         AnnRubberStampObject annRubberStampObject = _annObject as AnnRubberStampObject;

         if (annRubberStampObject != null)
         {

            AnnRubberStampType newRubberStampType = (AnnRubberStampType)Enum.Parse(typeof(AnnRubberStampType), "Stamp" + this.Properties["RuberStampType"].Values[(string)newValue].ToString());
            annRubberStampObject.RubberStampType = newRubberStampType;

            if (OnPropertyChanged != null)
               OnPropertyChanged("RubberStampType", newRubberStampType);
         }
      }

      void acuteInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnProtractorObject annProtractorObject = _annObject as AnnProtractorObject;

         if (annProtractorObject != null)
         {
            annProtractorObject.Acute = (bool)this.Properties["Acute"].Values[(string)newValue];

            if (OnPropertyChanged != null)
               OnPropertyChanged("Acute", newValue);
         }
      }

      void fixedPointerInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnTextPointerObject annTextPointerObject = _annObject as AnnTextPointerObject;

         if (annTextPointerObject != null)
         {
            annTextPointerObject.FixedPointer = (bool)this.Properties["FixedPointer"].Values[(string)newValue];

            if (OnPropertyChanged != null)
               OnPropertyChanged("FixedPointer", newValue);
         }
      }

      void anglePrecisionInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnProtractorObject annProtractorObject = _annObject as AnnProtractorObject;

         if (annProtractorObject != null)
         {
            annProtractorObject.AnglePrecision = (int)this.Properties["AnglePrecision"].Values[(string)newValue];

            if (OnPropertyChanged != null)
               OnPropertyChanged("AnglePrecision", newValue);
         }
      }

      void precisionInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnPolyRulerObject annPolyRulerObject = _annObject as AnnPolyRulerObject;

         if (annPolyRulerObject != null)
         {
            annPolyRulerObject.Precision = (int)this.Properties["Precision"].Values[(string)newValue];

            if (OnPropertyChanged != null)
               OnPropertyChanged("Precision", newValue);
         }
      }

      void angularUnitInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnProtractorObject annProtractorObject = _annObject as AnnProtractorObject;

         if (annProtractorObject != null)
         {
            AnnAngularUnit newAngularUnit = (AnnAngularUnit)Enum.Parse(typeof(AnnAngularUnit), (string)this.Properties["AngularUnit"].Values[(string)newValue]);
            annProtractorObject.AngularUnit = newAngularUnit;

            if (OnPropertyChanged != null)
               OnPropertyChanged("AngularUnit", newAngularUnit);
         }
      }

      void showTickMarksInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnPolyRulerObject annPolyRulerObject = _annObject as AnnPolyRulerObject;

         if (annPolyRulerObject != null)
         {
            annPolyRulerObject.ShowTickMarks = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ShowTickMarks", newValue);
         }
      }

      void measurementUnitInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnPolyRulerObject annPolyRulerObject = _annObject as AnnPolyRulerObject;

         if (annPolyRulerObject != null)
         {
            AnnUnit newMeasurementUnit = (AnnUnit)Enum.Parse(typeof(AnnUnit), (string)this.Properties["MeasurementUnit"].Values[(string)newValue]);
            annPolyRulerObject.MeasurementUnit = newMeasurementUnit;

            if (OnPropertyChanged != null)
               OnPropertyChanged("MeasurementUnit", newValue);
         }
      }

      void showGauge_ValueChanged(object oldValue, object newValue)
      {
         AnnPolyRulerObject annPolyRulerObject = _annObject as AnnPolyRulerObject;

         if (annPolyRulerObject != null)
         {
            annPolyRulerObject.ShowGauge = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("ShowGauge", newValue);
         }
      }

      void tensionInfo_ValueChanged(object oldValue, object newValue)
      {
         AnnCurveObject curve = _annObject as AnnCurveObject;

         if (curve != null)
         {
            double tenstion = (double)newValue;

            //Tension should be from 0 to 1
            if (tenstion < 0)
               tenstion = 0;
            else if (tenstion > 1)
               tenstion = 1;

            curve.Tension = tenstion;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Tension", tenstion);
         }
      }

      void horizontalAlignment_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;

         if (annTextObject != null)
         {
            AnnHorizontalAlignment newHorizontalAlignment = (AnnHorizontalAlignment)Enum.Parse(typeof(AnnHorizontalAlignment), (string)this.Properties["HorizontalAlignment"].Values[(string)newValue]);
            annTextObject.HorizontalAlignment = newHorizontalAlignment;

            if (OnPropertyChanged != null)
               OnPropertyChanged("HorizontalAlignment", newHorizontalAlignment);
         }
      }

      void verticalAlignment_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;

         if (annTextObject != null)
         {
            AnnVerticalAlignment newVerticalAlignment = (AnnVerticalAlignment)Enum.Parse(typeof(AnnVerticalAlignment), (string)this.Properties["VerticalAlignment"].Values[(string)newValue]);
            annTextObject.VerticalAlignment = newVerticalAlignment;

            if (OnPropertyChanged != null)
               OnPropertyChanged("VerticalAlignment", newVerticalAlignment);
         }
      }

      void text_ValueChanged(object oldValue, object newValue)
      {
         AnnTextObject annTextObject = _annObject as AnnTextObject;
         annTextObject.Text = (string)newValue;

         if (OnPropertyChanged != null)
            OnPropertyChanged("Text", newValue);
      }

      void media_ValueChanged(object oldValue, object newValue)
      {
         AnnMediaObject annVideoObject = _annObject as AnnMediaObject;
         if (annVideoObject != null)
         {
            annVideoObject.Media = (AnnMedia)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Media", newValue);
         }
      }

      void encryptKey_ValueChanged(object oldValue, object newValue)
      {
         AnnEncryptObject annEncryptObject = _annObject as AnnEncryptObject;
         annEncryptObject.Key = (int)newValue;

         if (OnPropertyChanged != null)
            OnPropertyChanged("Key", newValue);
      }

      void encryptor_ValueChanged(object oldValue, object newValue)
      {
         AnnEncryptObject annEncryptObject = _annObject as AnnEncryptObject;
         if (annEncryptObject != null)
         {
            annEncryptObject.Encryptor = (bool)newValue;

            if (OnPropertyChanged != null)
               OnPropertyChanged("Encryptor", newValue);
         }
      }


      private string RubberStampTypeToString(AnnRubberStampType type)
      {
         return Enum.GetName(typeof(AnnRubberStampType), (int)type);
      }
      void FillEnumValue(AnnPropertyInfo info, Type type)
      {
         String[] names = Enum.GetNames(type);
         foreach (String enumName in names)
         {
            String member = enumName.Substring(0, 1).ToUpper() + enumName.Substring(1);
            if (member.StartsWith("Stamp"))
            {
               member = member.Replace("Stamp", "");
            }
            info.Values[member] = member;
         }
      }
   }
}
