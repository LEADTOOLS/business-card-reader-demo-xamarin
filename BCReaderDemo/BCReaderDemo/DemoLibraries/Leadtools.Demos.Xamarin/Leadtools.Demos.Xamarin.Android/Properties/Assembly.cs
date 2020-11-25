//*************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.
// All Rights Reserved.
//*************************************************************

using Leadtools.Demos.Droid.Utils;

namespace Leadtools.Demos.Droid
{
   public static class Assembly
   {
      public static void Use(Android.App.Activity mainActivity)
      {
         DemoUtilities.Init();
         DemoUtilitiesImplementation.Init(mainActivity);
         PermissionsImplementation.Init(mainActivity);
         PicturePickerImplementation.Init(mainActivity);
         PictureSaverImplementation.Init(mainActivity);
         StatusBarImplementation.Init(mainActivity);
         ToastImplementation.Init(mainActivity);
      }
   }
}