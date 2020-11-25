// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Utils;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class BorderView : ContentView
   {
      #region Config

      public static Color BorderColor { get; } = Color.FromRgb(244, 66, 79);
      public static double BorderWidth { get; } = 0.15 * GlobalMarginExtension.UnitSize;

      #endregion
   }
}