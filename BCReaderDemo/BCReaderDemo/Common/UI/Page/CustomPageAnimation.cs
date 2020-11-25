// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Rg.Plugins.Popup.Interfaces.Animations;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Page
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomPageAnimation : IPopupAnimation
   {
      #region Config

      private static uint HorizontalDuration { get; } = 250U;
      private static uint VerticalDuration { get; } = 350U;
      public static Easing QuintIn { get; } = new Easing(t => Math.Pow(t, 5));
      public static Easing QuintOut { get; } = new Easing(t => 1 - Math.Pow(1 - t, 5));

      #endregion

      #region Internal properties

      private CustomPage Page { get; set; }
      private List<PropertyCache> PageContentCaches { get; set; }
      private View PageFooter { get; set; }
      private PropertyCache PageRootCache { get; set; }
      private CustomPageAnimation PreviousPageAnimation { get; set; }

      #endregion

      #region Methods

      #region IPopupAnimation

      public void Preparing(View _, PopupPage page)
      {
         // Get the previous page's animation
         IReadOnlyList<PopupPage> stack = PopupNavigation.Instance.PopupStack;
         PreviousPageAnimation = stack.Count > 1 ? stack[stack.Count - 2]?.Animation as CustomPageAnimation : null;

         // Create the caches
         Page = page as CustomPage;
         CreateCaches();

         // Get the footer element
         PageFooter = Page.GetTemplateChild("PageFooter") as View;

         // Hide the page
         Page.IsVisible = false;
      }

      public void Disposing(View _, PopupPage _2)
      {
         // Restore each cache
         PageRootCache.Restore();
         foreach (PropertyCache cache in PageContentCaches)
            cache.Restore();
      }

      public async Task Appearing(View _, PopupPage _2)
      {
         if (PreviousPageAnimation == null)
            await AppearFromBottom();
         else
            await MoveToSide(PreviousPageAnimation, this, true);
      }

      public async Task Disappearing(View _, PopupPage _2)
      {
         if (PreviousPageAnimation == null)
            await DisappearToBottom();
         else
            await MoveToSide(PreviousPageAnimation, this, false);
      }

      #endregion

      #region Helpers

      private async Task AppearFromBottom()
      {
         // Position to animation start
         PageRootCache.View.TranslationY = GetNewTranslationY(PageRootCache);

         // Show page
         Page.IsVisible = true;

         // Animate
         await PageRootCache.View.TranslateTo(PageRootCache.TranslationX, PageRootCache.TranslationY, VerticalDuration, QuintOut);
      }

      private void CreateCaches()
      {
         PageRootCache = new PropertyCache(Page.GetTemplateChild("PageRoot") as View);
         PageContentCaches = new List<PropertyCache>();
         foreach (string name in new string[] { "PageBackButton", "PageTitle", "PageContent" })
            PageContentCaches.Add(new PropertyCache(Page.GetTemplateChild(name) as View));
      }

      private async Task DisappearToBottom()
      {
         // Update the cache
         PageRootCache.Update();

         // Animate
         await PageRootCache.View.TranslateTo(PageRootCache.TranslationX, GetNewTranslationY(PageRootCache), VerticalDuration, QuintIn);

         // Hide page
         Page.IsVisible = false;
      }

      private double GetNewTranslationX(PropertyCache cache, bool rightSide) => (rightSide ? Page.Width : -Page.Width) + cache.TranslationX;
      private double GetNewTranslationY(PropertyCache cache) => Page.Height + cache.TranslationY;

      private async Task MoveToSide(bool rightSide, bool appearing)
      {
         if (appearing)
         {
            // Position each element
            foreach (PropertyCache cache in PageContentCaches)
               cache.View.TranslationX = GetNewTranslationX(cache, rightSide);

            // Show page
            Page.IsVisible = true;

            // Animate each element
            List<Task> animations = new List<Task>();
            foreach (PropertyCache cache in PageContentCaches)
               animations.Add(cache.View.TranslateTo(cache.TranslationX, cache.TranslationY, HorizontalDuration, QuintOut));
            await Task.WhenAll(animations);
         }
         else
         {
            // Update each cache
            PageRootCache.Update();
            foreach (PropertyCache cache in PageContentCaches)
               cache.Update();

            // Animate each element
            List<Task> animations = new List<Task>();
            foreach (PropertyCache cache in PageContentCaches)
               animations.Add(cache.View.TranslateTo(GetNewTranslationX(cache, rightSide), cache.TranslationY, HorizontalDuration, QuintOut));
            await Task.WhenAll(animations);

            // Hide page
            Page.IsVisible = false;
         }
      }

      private static async Task MoveToSide(CustomPageAnimation leftPage, CustomPageAnimation rightPage, bool goingForward)
      {
         // Clear the background colors
         Color pageBackgroundColor = rightPage.Page.BackgroundColor;
         rightPage.Page.BackgroundColor = Color.Transparent;
         Color pageRootBackgroundColor = rightPage.PageRootCache.View.BackgroundColor;
         rightPage.PageRootCache.View.BackgroundColor = Color.Transparent;

         // Hide the footer
         double opacity = rightPage.PageFooter.Opacity;
         rightPage.PageFooter.Opacity = 0;

         // Animate
         await Task.WhenAll(leftPage.MoveToSide(false, !goingForward), rightPage.MoveToSide(true, goingForward));

         // Restore the footer
         rightPage.PageFooter.Opacity = opacity;

         // Restore the background colors
         rightPage.Page.BackgroundColor = pageBackgroundColor;
         rightPage.PageRootCache.View.BackgroundColor = pageRootBackgroundColor;
      }

      #endregion

      #endregion

      #region PropertyCache

      private class PropertyCache
      {
         public PropertyCache(View view)
         {
            View = view;
            Update();
         }

         #region Public properties

         public double TranslationX { get; set; }
         public double TranslationY { get; set; }
         public View View { get; }

         #endregion

         #region Methods

         public void Restore()
         {
            View.TranslationX = TranslationX;
            View.TranslationY = TranslationY;
         }

         public void Update()
         {
            TranslationX = View.TranslationX;
            TranslationY = View.TranslationY;
         }

         #endregion
      }

      #endregion
   }
}
