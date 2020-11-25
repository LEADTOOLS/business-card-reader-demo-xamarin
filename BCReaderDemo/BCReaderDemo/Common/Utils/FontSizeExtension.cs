// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Xamarin.Forms;

namespace Leadtools.Demos.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [ContentProperty(nameof(Argument))]
   public class FontSizeExtension : UnitSizeExtension
   {
      #region Config

      public static double UnitSize { get; } = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

      public static double MicroFontSize { get; } = UnitSize * 0.6;
      public static double SmallFontSize { get; } = UnitSize * 0.8;
      public static double MediumFontSize { get; } = UnitSize;
      public static double LargeFontSize { get; } = UnitSize * 1.3;

      #endregion

      public FontSizeExtension() : base(UnitSize)
      {
      }
   }
}
