// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Windows.Input;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   public class MultiGesturesEffect : RoutingEffect
   {
      public MultiGesturesEffect() : base($"Leadtools.Demos.UI.Elements.{nameof(MultiGesturesEffect)}")
      {
      }

      // LongPressed Command
      public static readonly BindableProperty LongPressedCommandProperty = BindableProperty.CreateAttached("LongPressedCommand", typeof(ICommand), typeof(MultiGesturesEffect), (object)null);
      public static ICommand GetLongPressedCommand(BindableObject view)
      {
         return (ICommand)view.GetValue(LongPressedCommandProperty);
      }

      public static void SetLongPressedCommand(BindableObject view, ICommand value)
      {
         view.SetValue(LongPressedCommandProperty, value);
      }

      public static readonly BindableProperty LongPressedCommandParameterProperty = BindableProperty.CreateAttached("LongPressedCommandParameter", typeof(object), typeof(MultiGesturesEffect), (object)null);
      public static object GetLongPressedCommandParameter(BindableObject view)
      {
         return view.GetValue(LongPressedCommandParameterProperty);
      }

      public static void SetLongPressedCommandParameter(BindableObject view, object value)
      {
         view.SetValue(LongPressedCommandParameterProperty, value);
      }

      // Tapped Command
      public static readonly BindableProperty TappedCommandProperty = BindableProperty.CreateAttached("TappedCommand", typeof(ICommand), typeof(MultiGesturesEffect), (object)null);
      public static ICommand GetTappedCommand(BindableObject view)
      {
         return (ICommand)view.GetValue(TappedCommandProperty);
      }

      public static void SetTappedCommand(BindableObject view, ICommand value)
      {
         view.SetValue(TappedCommandProperty, value);
      }

      public static readonly BindableProperty TappedCommandParameterProperty = BindableProperty.CreateAttached("TappedCommandParameter", typeof(object), typeof(MultiGesturesEffect), (object)null);
      public static object GetTappedCommandParameter(BindableObject view)
      {
         return view.GetValue(TappedCommandParameterProperty);
      }

      public static void SetTappedCommandParameter(BindableObject view, object value)
      {
         view.SetValue(TappedCommandParameterProperty, value);
      }
   }
}
