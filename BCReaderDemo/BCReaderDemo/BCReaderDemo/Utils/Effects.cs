// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Linq;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{
   /// <summary>
   /// Round corners effect. Used for creating round corners for any kind of Xamarin control
   /// </summary>
   public static class RoundCornersEffect
   {
      public static readonly BindableProperty CornerRadiusProperty =
          BindableProperty.CreateAttached(
              "CornerRadius",
              typeof(int),
              typeof(RoundCornersEffect),
              0,
              propertyChanged: OnCornerRadiusChanged);

      public static int GetCornerRadius(BindableObject view) => (int)view.GetValue(CornerRadiusProperty);

      public static void SetCornerRadius(BindableObject view, int value) => view.SetValue(CornerRadiusProperty, value);

      private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
      {
         if (!(bindable is View view))
            return;

         var cornerRadius = (int)newValue;
#if __ANDROID__
         var effect = view.Effects.OfType<BCReaderDemo.Droid.CustomRoundCornersEffect>().FirstOrDefault();
#elif __IOS__
         var effect = view.Effects.OfType<BCReaderDemo.iOS.CustomRoundCornersEffect>().FirstOrDefault();
#endif // #if __ANDROID__

         if (cornerRadius > 0 && effect == null)
            view.Effects.Add(Effect.Resolve("BCReaderDemo.RoundCornersEffect"));

         if (cornerRadius == 0 && effect != null)
            view.Effects.Remove(effect);
      }
   }
}