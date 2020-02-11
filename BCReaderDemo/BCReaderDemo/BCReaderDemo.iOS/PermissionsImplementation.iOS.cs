using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;
using AVFoundation;
using Foundation;
using AddressBook;
using CoreMotion;
using UIKit;
using Photos;
using System.Diagnostics;
using Speech;
using Xamarin.Forms;
using BCReaderDemo.iOS;
using Permissions;
using System.Linq;
using System.Threading;

[assembly: Dependency(typeof(PermissionsImplementation))]
namespace BCReaderDemo.iOS
{
   /// <summary>
   /// Implementation for Permissions Feature for iOS Platform
   /// </summary>
   public class PermissionsImplementation : IPermissions
   {
      CLLocationManager locationManager;
      ABAddressBook addressBook;
      CMMotionActivityManager activityManager;

      /// <summary>
      /// Requests the permissions from the users
      /// </summary>
      /// <returns>The permissions and their status.</returns>
      /// <param name="permissions">Permissions to request.</param>
      public async Task<Dictionary<Permission, PermissionStatus>> VerifyPermissionsAsync(params Permission[] permissions)
      {
         var results = new Dictionary<Permission, PermissionStatus>();
         foreach (var permission in permissions)
         {
            if (results.ContainsKey(permission))
               continue;

            PermissionStatus status = PermissionStatus.Unknown;
            status = await CheckPermissionAsync(permission);
            switch (status)
            {
               case PermissionStatus.Unknown:
                  status = await RequestPermissionsAsync(permission);
                  break;

               case PermissionStatus.Denied:
               case PermissionStatus.Disabled:
               case PermissionStatus.Restricted:
                  Page activePage = HomePage.Instance.Navigation.NavigationStack.LastOrDefault();
                  if (activePage != null)
                  {
                     string title = string.Format("Cannot Access the {0}", permission.ToString());
                     string message = string.Format((permission == Permission.Storage) ? "This app needs Storage permission to save cards images, so please turn on Storage permission from phone settings." : "Please turn on {0} permission for this app.", permission.ToString());
                     var alertView =  UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                     alertView.AddAction(UIAlertAction.Create((permission == Permission.Storage) ? "Exit" : "Cancel", UIAlertActionStyle.Default, alert =>
                     {
                        if (permission == Permission.Storage)
                           Thread.CurrentThread.Abort();
                     }));

                     alertView.AddAction(UIAlertAction.Create("Go to Settings", UIAlertActionStyle.Cancel, alert =>
                     {
                        OpenAppSettings();
                        if (permission == Permission.Storage)
                           Thread.CurrentThread.Abort();
                     }));
                     UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertView, true, null);
                  }
                  break;

               default:
                  break;
            }

            if (!results.ContainsKey(permission))
               results.Add(permission, status);
         }

         return results;
      }

      private async Task<PermissionStatus> RequestPermissionsAsync(Permission permission)
      {
         PermissionStatus status = PermissionStatus.Unknown;
         switch (permission)
         {
            case Permission.Camera:
               try
               {
                  var authCamera = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
                  status = (authCamera) ? PermissionStatus.Granted : PermissionStatus.Denied;
               }
               catch (Exception ex)
               {
                  Debug.WriteLine(ex.Message);
               }
               break;
            case Permission.Contacts:
               status = await RequestContactsPermission();
               break;
            case Permission.LocationWhenInUse:
            case Permission.LocationAlways:
            case Permission.Location:
               status = await RequestLocationPermission(permission);
               break;
            case Permission.Microphone:
               try
               {
                  var authMic = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Audio);
                  status = (authMic) ? PermissionStatus.Granted : PermissionStatus.Denied;
               }
               catch (Exception ex)
               {
                  Debug.WriteLine(ex.Message);
               }
               break;
            case Permission.Photos:
               status = await RequestPhotosPermission();
               break;
            case Permission.Sensors:
               status = await RequestSensorsPermission();
               break;
            case Permission.Speech:
               status = await RequestSpeechPermission();
               break;
         }

         return status;
      }

      public Task<PermissionStatus> CheckPermissionAsync(Permission permission)
      {
         switch (permission)
         {
            case Permission.Camera:
               return Task.FromResult(GetAVPermissionStatus(AVMediaType.Video));
            case Permission.Contacts:
               return Task.FromResult(ContactsPermissionStatus);
            case Permission.Location:
            case Permission.LocationAlways:
            case Permission.LocationWhenInUse:
               return Task.FromResult(GetLocationPermissionStatus(permission));
            case Permission.Microphone:
               return Task.FromResult(GetAVPermissionStatus(AVMediaType.Audio));
            case Permission.Photos:
               return Task.FromResult(PhotosPermissionStatus);
            case Permission.Sensors:
               return Task.FromResult(SensorsPermissionStatus);
            case Permission.Speech:
               return Task.FromResult(SpeechPermissionStatus);
         }
         return Task.FromResult(PermissionStatus.Granted);
      }

      #region Camera and Microphone

      PermissionStatus GetAVPermissionStatus(NSString mediaType)
      {
         var status = AVCaptureDevice.GetAuthorizationStatus(mediaType);
         switch (status)
         {
            case AVAuthorizationStatus.Authorized:
               return PermissionStatus.Granted;
            case AVAuthorizationStatus.Denied:
               return PermissionStatus.Denied;
            case AVAuthorizationStatus.Restricted:
               return PermissionStatus.Restricted;
            default:
               return PermissionStatus.Unknown;
         }
      }
      #endregion

      #region Contacts
      PermissionStatus ContactsPermissionStatus
      {
         get
         {
            var status = ABAddressBook.GetAuthorizationStatus();
            switch (status)
            {
               case ABAuthorizationStatus.Authorized:
                  return PermissionStatus.Granted;
               case ABAuthorizationStatus.Denied:
                  return PermissionStatus.Denied;
               case ABAuthorizationStatus.Restricted:
                  return PermissionStatus.Restricted;
               default:
                  return PermissionStatus.Unknown;
            }
         }
      }

      Task<PermissionStatus> RequestContactsPermission()
      {

         if (ContactsPermissionStatus != PermissionStatus.Unknown)
            return Task.FromResult(ContactsPermissionStatus);

         addressBook = new ABAddressBook();

         var taskCompletionSource = new TaskCompletionSource<PermissionStatus>();


         addressBook.RequestAccess((success, error) =>
             {
                taskCompletionSource.TrySetResult((success ? PermissionStatus.Granted : PermissionStatus.Denied));
             });

         return taskCompletionSource.Task;
      }
      #endregion

      #region Location
      public static TimeSpan LocationPermissionTimeout { get; set; } = new TimeSpan(0, 0, 8);
      Task<PermissionStatus> RequestLocationPermission(Permission permission = Permission.Location)
      {
         if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse && permission == Permission.LocationAlways)
         {
            // do nothing, just request it
         }
         else if (GetLocationPermissionStatus(permission) != PermissionStatus.Unknown)
            return Task.FromResult(GetLocationPermissionStatus(permission));

         if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
         {
            return Task.FromResult(PermissionStatus.Unknown);
         }

         locationManager = new CLLocationManager();

         var taskCompletionSource = new TaskCompletionSource<PermissionStatus>();

         var previousState = CLLocationManager.Status;

         locationManager.AuthorizationChanged += AuthorizationChanged;

         void AuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
         {
            Console.WriteLine(e.Status);

            if (e.Status == CLAuthorizationStatus.NotDetermined)
               return;

            if (previousState == CLAuthorizationStatus.AuthorizedWhenInUse && permission == Permission.LocationAlways)
            {
               if (e.Status == CLAuthorizationStatus.AuthorizedWhenInUse)
               {
                  WithTimeout(taskCompletionSource.Task, LocationPermissionTimeout).ContinueWith((t) =>
                  {
                           //wait 10 seconds and check to see if it is completed or not.
                           if (!taskCompletionSource.Task.IsCompleted)
                     {
                        locationManager.AuthorizationChanged -= AuthorizationChanged;
                        taskCompletionSource.TrySetResult(GetLocationPermissionStatus(permission));
                     }
                  });
                  return;
               }
            }

            locationManager.AuthorizationChanged -= AuthorizationChanged;

            taskCompletionSource.TrySetResult(GetLocationPermissionStatus(permission));
         }


         var info = NSBundle.MainBundle.InfoDictionary;


         if (permission == Permission.Location)
         {
            if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
               locationManager.RequestAlwaysAuthorization();
            else if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
               locationManager.RequestWhenInUseAuthorization();
            else
               throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
         }
         else if (permission == Permission.LocationAlways)
         {
            if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
               locationManager.RequestAlwaysAuthorization();
            else
               throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");

         }
         else
         {
            if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
               locationManager.RequestWhenInUseAuthorization();
            else
               throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");

         }


         return taskCompletionSource.Task;
      }

      async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeSpan)
      {
         var retTask = await Task.WhenAny(task, Task.Delay(timeSpan))
            .ConfigureAwait(false);

         return retTask is Task<T> ? task.Result : default(T);
      }



      PermissionStatus GetLocationPermissionStatus(Permission permission)
      {

         if (!CLLocationManager.LocationServicesEnabled)
            return PermissionStatus.Disabled;

         var status = CLLocationManager.Status;

         if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
         {
            //if checking for always then check to see if we have it really, else denied
            if (permission == Permission.LocationAlways)
            {
               switch (status)
               {
                  case CLAuthorizationStatus.AuthorizedAlways:
                     return PermissionStatus.Granted;
                  case CLAuthorizationStatus.AuthorizedWhenInUse:
                  case CLAuthorizationStatus.Denied:
                     return PermissionStatus.Denied;
                  case CLAuthorizationStatus.Restricted:
                     return PermissionStatus.Restricted;
                  default:
                     return PermissionStatus.Unknown;
               }
            }

            switch (status)
            {
               case CLAuthorizationStatus.AuthorizedAlways:
               case CLAuthorizationStatus.AuthorizedWhenInUse:
                  return PermissionStatus.Granted;
               case CLAuthorizationStatus.Denied:
                  return PermissionStatus.Denied;
               case CLAuthorizationStatus.Restricted:
                  return PermissionStatus.Restricted;
               default:
                  return PermissionStatus.Unknown;
            }
         }

         switch (status)
         {
            case CLAuthorizationStatus.Authorized:
               return PermissionStatus.Granted;
            case CLAuthorizationStatus.Denied:
               return PermissionStatus.Denied;
            case CLAuthorizationStatus.Restricted:
               return PermissionStatus.Restricted;
            default:
               return PermissionStatus.Unknown;
         }



      }
      #endregion

      #region Photos
      PermissionStatus PhotosPermissionStatus
      {
         get
         {
            var status = PHPhotoLibrary.AuthorizationStatus;
            switch (status)
            {
               case PHAuthorizationStatus.Authorized:
                  return PermissionStatus.Granted;
               case PHAuthorizationStatus.Denied:
                  return PermissionStatus.Denied;
               case PHAuthorizationStatus.Restricted:
                  return PermissionStatus.Restricted;
               default:
                  return PermissionStatus.Unknown;
            }
         }
      }

      Task<PermissionStatus> RequestPhotosPermission()
      {
         if (PhotosPermissionStatus != PermissionStatus.Unknown)
            return Task.FromResult(PhotosPermissionStatus);

         var taskCompletionSource = new TaskCompletionSource<PermissionStatus>();

         PHPhotoLibrary.RequestAuthorization(status =>
             {
                switch (status)
                {
                   case PHAuthorizationStatus.Authorized:
                      taskCompletionSource.TrySetResult(PermissionStatus.Granted);
                      break;
                   case PHAuthorizationStatus.Denied:
                      taskCompletionSource.TrySetResult(PermissionStatus.Denied);
                      break;
                   case PHAuthorizationStatus.Restricted:
                      taskCompletionSource.TrySetResult(PermissionStatus.Restricted);
                      break;
                   default:
                      taskCompletionSource.TrySetResult(PermissionStatus.Unknown);
                      break;
                }
             });

         return taskCompletionSource.Task;
      }

      #endregion

      #region Sensors

      PermissionStatus SensorsPermissionStatus
      {
         get
         {
            var sensorStatus = PermissionStatus.Unknown;

            //return disabled if not available.
            if (!CMMotionActivityManager.IsActivityAvailable)
               sensorStatus = PermissionStatus.Disabled;
            else if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
               switch (CMMotionActivityManager.AuthorizationStatus)
               {
                  case CMAuthorizationStatus.Authorized:
                     sensorStatus = PermissionStatus.Granted;
                     break;
                  case CMAuthorizationStatus.Denied:
                     sensorStatus = PermissionStatus.Denied;
                     break;
                  case CMAuthorizationStatus.NotDetermined:
                     sensorStatus = PermissionStatus.Unknown;
                     break;
                  case CMAuthorizationStatus.Restricted:
                     sensorStatus = PermissionStatus.Restricted;
                     break;
               }
            }

            return sensorStatus;
         }
      }
      async Task<PermissionStatus> RequestSensorsPermission()
      {
         if (SensorsPermissionStatus != PermissionStatus.Unknown)
            return SensorsPermissionStatus;

         activityManager = new CMMotionActivityManager();

         try
         {
            var results = await activityManager.QueryActivityAsync(NSDate.DistantPast, NSDate.DistantFuture, NSOperationQueue.MainQueue);
            if (results != null)
               return PermissionStatus.Granted;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Unable to query activity manager: " + ex.Message);
            return PermissionStatus.Denied;
         }

         return PermissionStatus.Unknown;
      }
      #endregion

      #region Speech
      Task<PermissionStatus> RequestSpeechPermission()
      {
         if (SpeechPermissionStatus != PermissionStatus.Unknown)
            return Task.FromResult(SpeechPermissionStatus);


         if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
         {
            return Task.FromResult(PermissionStatus.Unknown);
         }

         var taskCompletionSource = new TaskCompletionSource<PermissionStatus>();

         SFSpeechRecognizer.RequestAuthorization(status =>
         {
            switch (status)
            {
               case SFSpeechRecognizerAuthorizationStatus.Authorized:
                  taskCompletionSource.TrySetResult(PermissionStatus.Granted);
                  break;
               case SFSpeechRecognizerAuthorizationStatus.Denied:
                  taskCompletionSource.TrySetResult(PermissionStatus.Denied);
                  break;
               case SFSpeechRecognizerAuthorizationStatus.Restricted:
                  taskCompletionSource.TrySetResult(PermissionStatus.Restricted);
                  break;
               default:
                  taskCompletionSource.TrySetResult(PermissionStatus.Unknown);
                  break;
            }
         });
         return taskCompletionSource.Task;
      }

      PermissionStatus SpeechPermissionStatus
      {
         get
         {
            var status = SFSpeechRecognizer.AuthorizationStatus;
            switch (status)
            {
               case SFSpeechRecognizerAuthorizationStatus.Authorized:
                  return PermissionStatus.Granted;
               case SFSpeechRecognizerAuthorizationStatus.Denied:
                  return PermissionStatus.Denied;
               case SFSpeechRecognizerAuthorizationStatus.Restricted:
                  return PermissionStatus.Restricted;
               default:
                  return PermissionStatus.Unknown;
            }
         }
      }
      #endregion


      public bool OpenAppSettings()
      {
         try
         {
            if (Device.IsInvokeRequired)
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  NSUrl settingsUrl = new NSUrl(UIApplication.OpenSettingsUrlString);
                  if (UIApplication.SharedApplication.CanOpenUrl(settingsUrl))
                  {
                     UIApplication.SharedApplication.OpenUrl(settingsUrl);
                  }
               });
            }
            else
            {
               NSUrl settingsUrl = new NSUrl(UIApplication.OpenSettingsUrlString);
               if (UIApplication.SharedApplication.CanOpenUrl(settingsUrl))
               {
                  UIApplication.SharedApplication.OpenUrl(settingsUrl);
               }
            }

            return true;
         }
         catch(Exception ex)
         {
            Debug.WriteLine(ex.Message);
            return false;
         }
      }
   }
}
