// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Leadtools.Annotations.Xamarin
{
   public class AnnPropertyInfo
   {
      public event AnnObjectEditorValueChangedHandler ValueChanged;
      IAnnEditor _editorType;

      public IAnnEditor EditorType
      {
         get { return _editorType; }
      }

      public AnnPropertyInfo(string propertyName, bool readOnly, object value, string category, string description, string displayName, bool visible, Type editorType)
      {
         _isReadOnly = readOnly;
         _type = (value == null) ? typeof(object) : value.GetType();
         _category = category;
         _description = description;
         _isVisible = visible;

         if (string.IsNullOrEmpty(displayName))
         {
            _displayName = propertyName;
         }
         else
         {
            _displayName = displayName;
         }

         IEnumerable<ConstructorInfo> list = editorType.GetTypeInfo().DeclaredConstructors;
         foreach (ConstructorInfo constructorInfo in list)
         {
            ParameterInfo[] paramsInfo = constructorInfo.GetParameters();
            if (paramsInfo.Length == 4)
            {
               _editorType = Activator.CreateInstance(editorType, value, category, propertyName, _displayName) as IAnnEditor;
            }
            else if (paramsInfo.Length == 3)
            {
               _editorType = Activator.CreateInstance(editorType, value, category, propertyName) as IAnnEditor;
            }
            else if (paramsInfo.Length == 2)
            {
               _editorType = Activator.CreateInstance(editorType, value, category) as IAnnEditor;
            }
            else if (paramsInfo.Length == 1)
            {
               _editorType = Activator.CreateInstance(editorType, value) as IAnnEditor;
            }
            if (_editorType != null)
               break;
         }

         _editorType.OnValueChanged += new AnnObjectEditorValueChangedHandler(editorType_OnValueChanged);

         _value = value;
      }

      void editorType_OnValueChanged(object oldValue, object newValue)
      {
         _value = newValue;
         if (ValueChanged != null)
            ValueChanged(oldValue, newValue);
      }

      bool _isReadOnly;

      public bool IsReadOnly
      {
         get { return _isReadOnly; }
      }

      bool _isVisible;

      public bool IsVisible
      {
         get { return _isVisible; }
      }

      object _value;

      public object Value
      {
         get { return _value; }
         set
         {
            if (ValueChanged != null)
               ValueChanged(_value, value);

            _value = value;
         }
      }

      string _displayName;

      public string DisplayName
      {
         get { return _displayName; }
         set { _displayName = value; }
      }

      Dictionary<string, object> _values = new Dictionary<string, object>();

      public Dictionary<string, object> Values
      {
         get { return _values; }
      }

      private Type _type;

      public Type Type
      {
         get { return _type; }
      }

      bool _hasValues = false;
      public bool HasValues
      {
         get { return _hasValues; }
      }
      string _category;

      public string Category
      {
         get { return _category; }
         set { _category = value; }
      }

      string _description;

      public string Description
      {
         get { return _description; }
         set { _description = value; }
      }
   }
}
