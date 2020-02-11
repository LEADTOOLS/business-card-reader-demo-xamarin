// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Android.Content;
using BCReaderDemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(CustumButtonRenderer))]
namespace BCReaderDemo.Droid
{
   // This custom render for Android will allow make the CornerRadius property of Button to have effect in order to make round rectangular buttons
   public class CustumButtonRenderer : ButtonRenderer
   {
      public CustumButtonRenderer(Context context) : base(context)
      {
      }

      protected override void OnDraw(Android.Graphics.Canvas canvas)
      {
         base.OnDraw(canvas);
      }

      protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
      {
         base.OnElementChanged(e);
      }
   }
}