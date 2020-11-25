// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Page;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.Utils
{
   public enum AnimationDirection
   {
      LeftToRight,
      RightToLeft,
      TopToBottom,
      BottomToTop
   }

   public static class Animator
   {
      public static async void AnimatePanel(View panel, AnimationDirection direction, bool appearing, uint animationDuration, double originalOpacity = 1)
      {
         await AnimatePanelAsync(panel, direction, appearing, animationDuration, originalOpacity);
      }

      public static async Task AnimatePanelAsync(View panel, AnimationDirection direction, bool appearing, uint animationDuration, double originalOpacity = 1)
      {
         double offscreenX = 0;
         double offscreenY = 0;
         Easing animationEasing = null;

         switch (direction)
         {
            case AnimationDirection.LeftToRight:
               offscreenX = -panel.Width;
               animationEasing = appearing ? CustomPageAnimation.QuintIn : CustomPageAnimation.QuintOut;
               break;
            case AnimationDirection.RightToLeft:
               offscreenX = panel.Width;
               animationEasing = appearing ? CustomPageAnimation.QuintIn : CustomPageAnimation.QuintOut;
               break;
            case AnimationDirection.TopToBottom:
               offscreenY = -panel.Height;
               animationEasing = appearing ? CustomPageAnimation.QuintOut : CustomPageAnimation.QuintIn;
               break;
            case AnimationDirection.BottomToTop:
               offscreenY = panel.Height;
               animationEasing = appearing ? CustomPageAnimation.QuintOut : CustomPageAnimation.QuintIn;
               break;
         }

         if (appearing)
         {
            // Position
            panel.TranslationX = offscreenX;
            panel.TranslationY = offscreenY;

            // Display
            panel.IsEnabled = panel.IsVisible = true;
            panel.Opacity = originalOpacity;
         }

         // Animate
         bool cancelled = await panel.TranslateTo(
            x: appearing ? 0 : offscreenX,
            y: appearing ? 0 : offscreenY,
            length: animationDuration,
            easing: animationEasing
         );

         if (!cancelled && !appearing)
         {
            // Hide
            panel.IsEnabled = panel.IsVisible = false;
            panel.Opacity = 0;
         }
      }

      public static async Task AnimateResizePanelAsync(View panel, bool appearing, uint animationDuration, double originalOpacity = 1)
      {
         Rectangle originalBounds = Rectangle.Zero;

         // Control should have a known Width/Height or WidthRequest/HeightRequest
         if(panel.WidthRequest > 0 && panel.HeightRequest > 0)
            originalBounds = new Rectangle(panel.X, panel.Y, panel.WidthRequest, panel.HeightRequest);
         else if(panel.Width > 0 && panel.Height > 0)
            originalBounds = new Rectangle(panel.X, panel.Y, panel.Width, panel.Height);

         if(originalBounds.IsEmpty && appearing)
         {
            panel.IsEnabled = panel.IsVisible = true;
            panel.Opacity = originalOpacity;
            return;
         }

         if (appearing)
         {
            panel.HeightRequest = 0;

            // Display
            panel.IsEnabled = panel.IsVisible = true;
            panel.Opacity = originalOpacity;
         }

         // Animate
         bool cancelled = await panel.LayoutTo(
            bounds: originalBounds,
            length: animationDuration,
            easing: appearing ? CustomPageAnimation.QuintOut : CustomPageAnimation.QuintIn
         );

         panel.HeightRequest = originalBounds.Height;
         if (!cancelled && !appearing)
         {
            // Hide
            panel.IsEnabled = panel.IsVisible = false;
            panel.Opacity = 0;
         }
      }
   }
}
