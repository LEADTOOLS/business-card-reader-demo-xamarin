// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomCheckBox : SvgImage
   {
      #region Config

      private static string CheckedImage { get; } = "Icons/selected.svg";
      private static string UncheckedImage { get; } = "Icons/unselected.svg";

      #endregion

      public CustomCheckBox()
      {
         // Start with unchecked image
         ResourceName = UncheckedImage;
      }

      #region Bindable properties

      #region IsChecked

      public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
         propertyName: nameof(IsChecked),
         returnType: typeof(bool),
         declaringType: typeof(CustomCheckBox),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomCheckBox self)
            {
               // Update image to match
               self.ResourceName = self.IsChecked ? CheckedImage : UncheckedImage;
            }
         }
      );
      public bool IsChecked { get => GetValue(IsCheckedProperty) is bool value ? value : false; set => SetValue(IsCheckedProperty, value); }

      #endregion

      #endregion
   }
}
