// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   /// <summary>
   /// Custom SearchBar class to handle SearchBar touch events.
   /// </summary>
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomSearchBar : SearchBar
   {
      public CustomSearchBar()
      {
         base.PlaceholderColor = Color.FromHex("#b6c6d1");
         base.TextColor = Color.FromHex("#57687f");
      }

      public static readonly BindableProperty UseAsEntryFieldProperty = BindableProperty.Create(
         propertyName: nameof(UseAsEntryField),
         returnType: typeof(bool),
         declaringType: typeof(CustomSearchBar),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool UseAsEntryField { get => GetValue(UseAsEntryFieldProperty) is bool value ? value : false; set => SetValue(UseAsEntryFieldProperty, value); }

      public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
         propertyName: nameof(BorderColor),
         returnType: typeof(Color),
         declaringType: typeof(CustomSearchBar),
         defaultValue: Color.FromHex("#ccd8e2"),
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public Color BorderColor { get => (Color)GetValue(BorderColorProperty); set => SetValue(BorderColorProperty, value); }

      public new Color PlaceholderColor { get => base.PlaceholderColor; set => base.PlaceholderColor = value; }
      public new Color TextColor { get => base.TextColor; set => base.TextColor = value; }

      public event EventHandler Clicked;
      public void OnClicked()
      {
         Clicked?.Invoke(this, null);
      }
   }
}