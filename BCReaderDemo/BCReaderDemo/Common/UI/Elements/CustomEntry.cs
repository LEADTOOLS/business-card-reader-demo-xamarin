// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomEntry : Entry
   {
      public event EventHandler KeyboardWillShow;
      public event EventHandler KeyboardWillHide;

      internal void OnKeyboardWillShow()
      {
         KeyboardWillShow?.Invoke(this, null);
      }

      internal void OnKeyboardWillHide()
      {
         KeyboardWillHide?.Invoke(this, null);
      }
   }
}
