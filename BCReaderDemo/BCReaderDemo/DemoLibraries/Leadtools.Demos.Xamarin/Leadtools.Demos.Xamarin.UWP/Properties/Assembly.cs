//*************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.
// All Rights Reserved.
//*************************************************************

using Leadtools.Demos.UWP.Utils;

namespace Leadtools.Demos.UWP
{
   public static class Assembly
   {
      public static void Use(Windows.UI.Xaml.Window mainWindow)
      {
         DemoUtilities.Init();
         DemoUtilitiesImplementation.Init(mainWindow);
      }
   }
}