// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Demos.Utils
{
   /// <summary>
   /// Permission group that can be requested
   /// </summary>
   public enum PermissionType
   {
      /// <summary>
      /// The unknown permission only used for return type, never requested
      /// </summary>
      Unknown,
      /// <summary>
      /// Android: Calendar
      /// iOS: None/Not Supported
      /// UWP: None/Not Supported
      /// </summary>
      Calendar,
      /// <summary>
      /// <summary>
      /// Android: Camera
      /// iOS: Photos (Camera Roll and Camera)
      /// UWP: None
      /// </summary>
      Camera,
      /// <summary>
      /// Android: Contacts
      /// iOS: AddressBook
      /// UWP: ContactManager
      /// </summary>
      Contacts,
      /// <summary>
      /// Android: Fine and Coarse Location
      /// iOS: CoreLocation (Always and WhenInUse)
      /// UWP: Geolocator
      /// </summary>
      Location,
      /// <summary>
      /// Android: Microphone
      /// iOS: Microphone
      /// UWP: None
      /// </summary>
      Microphone,
      /// <summary>
      /// Android: Phone
      /// iOS: Nothing
      /// </summary>
      Phone,
      /// <summary>
      /// Android: Nothing
      /// iOS: Photos
      /// UWP: None
      /// </summary>
      Photos,
      /// <summary>
      /// Android: Nothing
      /// iOS: Nothing
      /// UWP: None
      /// </summary>
      Reminders,
      /// <summary>
      /// Android: Body Sensors
      /// iOS: CoreMotion
      /// UWP: DeviceAccessInformation
      /// </summary>
      Sensors,
      /// <summary>
      /// Android: Sms
      /// iOS: Nothing
      /// UWP: None
      /// </summary>
      Sms,
      /// <summary>
      /// Android: External Storage
      /// iOS: Nothing
      /// </summary>
      Storage,
      /// <summary>
      /// Android: Microphone
      /// iOS: Speech
      /// UWP: None
      /// </summary>
      Speech,
      /// <summary>
      /// Android: Fine and Coarse Location
      /// iOS: CoreLocation - Always
      /// UWP: Geolocator
      /// </summary>
      LocationAlways,
      /// <summary>
      /// Android: Fine and Coarse Location
      /// iOS: CoreLocation - WhenInUse
      /// UWP: Geolocator
      /// </summary>
      LocationWhenInUse,
      /// <summary>
      /// Android: None/Not Supported
      /// iOS: None/Not Supported
      /// UWP: None/Not Supported
      /// </summary>
      MediaLibrary
   }

   /// <summary>
   /// Status of a permission
   /// </summary>
   public enum PermissionStatus
   {
      /// <summary>
      /// Denied by user
      /// </summary>
      Denied,
      /// <summary>
      /// Feature is disabled on device
      /// </summary>
      Disabled,
      /// <summary>
      /// Granted by user
      /// </summary>
      Granted,
      /// <summary>
      /// Restricted (only iOS)
      /// </summary>
      Restricted,
      /// <summary>
      /// Permission is in an unknown state
      /// </summary>
      Unknown
   }

   public interface IPermissions
   {
      #region Methods

      /// <summary>
      /// This function will check for requested permission and do other necessary calls to try to grant permission 
      /// or show necessary messages for users.
      /// </summary>
      /// <param name="permissions to verify their status"></param>
      /// <returns></returns>
      Task<Dictionary<PermissionType, PermissionStatus>> VerifyPermissionsAsync(bool exitIfFailedToObtainPermission, params PermissionType[] permissions);

      #endregion
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public static class PermissionTypeExtensions
   {
      #region Methods

      #region Helpers

      public static async Task<bool> VerifyAsync(this PermissionType permission, bool exitIfFailedToObtainPermission = false)
      {
         var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(exitIfFailedToObtainPermission, permission);
         PermissionType permissionType = permission;
         if (Device.RuntimePlatform == Device.Android)
         {
            if (permission == PermissionType.Photos && !results.TryGetValue(permissionType, out PermissionStatus status))
            {
               // Photos and Storage are the same for Android, so if the user sent the Photos permission and the return 
               // permission in the results in Storage then handle them the same.
               permissionType = PermissionType.Storage;
            }
         }

         return results != null && results.TryGetValue(permissionType, out PermissionStatus permissionStatus) && permissionStatus == PermissionStatus.Granted;
      }

#endregion

#endregion
   }
}
