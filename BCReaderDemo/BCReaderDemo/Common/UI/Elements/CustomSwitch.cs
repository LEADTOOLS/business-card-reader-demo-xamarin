// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomSwitch : SvgImage
   {
      #region Config

      private static string OnImage { get; } = "Icons/on.svg";
      private static string OffImage { get; } = "Icons/off.svg";

      #endregion

      public CustomSwitch()
      {
         // Start with off image
         ResourceName = OffImage;
      }

      #region Bindable properties

      #region IsToggled

      public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
         propertyName: nameof(IsToggled),
         returnType: typeof(bool),
         declaringType: typeof(CustomSwitch),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: (bindable, oldObject, newObject) =>
         {
            if (bindable is CustomSwitch self)
               // Update image to match
               self.ResourceName = self.IsToggled ? OnImage : OffImage;
         }
      );
      public bool IsToggled { get => GetValue(IsToggledProperty) is bool value ? value : false; set => SetValue(IsToggledProperty, value); }

      #endregion

      #endregion

      #region Public Properties

      public object Tag;

      #endregion // Public Properties
   }
}
