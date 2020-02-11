// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.iOS;
using DataService;
using Foundation;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShareImplementation))]
namespace BCReaderDemo.iOS
{
   public class ShareImplementation : IShareContact
   {
      // MUST BE CALLED FROM THE UI THREAD
      public async Task Show(string filePath)
      {
         var items = new NSObject[] { NSUrl.FromFilename(filePath) };
         var activityController = new UIActivityViewController(items, null);
         var vc = GetVisibleViewController();

         NSString[] excludedActivityTypes = null;

         if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
            activityController.ExcludedActivityTypes = excludedActivityTypes;

         if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
         {
            if (activityController.PopoverPresentationController != null)
            {
               activityController.PopoverPresentationController.SourceView = vc.View;
            }
         }
         await vc.PresentViewControllerAsync(activityController, true);
      }

      UIViewController GetVisibleViewController()
      {
         var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

         if (rootController.PresentedViewController == null)
            return rootController;

         if (rootController.PresentedViewController is UINavigationController)
         {
            return ((UINavigationController)rootController.PresentedViewController).TopViewController;
         }

         if (rootController.PresentedViewController is UITabBarController)
         {
            return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
         }

         return rootController.PresentedViewController;
      }
   }
}