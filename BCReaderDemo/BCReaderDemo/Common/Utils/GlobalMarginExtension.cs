// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Xamarin.Forms;

namespace Leadtools.Demos.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [ContentProperty(nameof(Argument))]
   public class GlobalMarginExtension : UnitSizeExtension
   {
      #region Config

      public static double UnitSize { get; } = FontSizeExtension.UnitSize * 1.75;

      #endregion

      public GlobalMarginExtension() : base(UnitSize)
      {
      }
   }
}
