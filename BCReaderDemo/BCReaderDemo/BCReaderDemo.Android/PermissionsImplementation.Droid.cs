using Android;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using BCReaderDemo.Droid;
using Permissions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(PermissionsImplementation))]
namespace BCReaderDemo.Droid
{
   /// <summary>
   /// Implementation for Permissions Feature for Android Platform
   /// </summary>
   public class PermissionsImplementation : IPermissions
   {
      private object _locker = new object();
      private TaskCompletionSource<Dictionary<Permission, PermissionStatus>> _taskCompletionSource;
      private Dictionary<Permission, PermissionStatus> _permissionsResults;
      private IList<string> _requestedPermissions;
      private Activity _activity = null;

      public static PermissionsImplementation Instance { get; private set; }

      /// <summary>
      /// Requests the permissions from the users
      /// </summary>
      /// <returns>The permissions and their status.</returns>
      /// <param name="permissions">Permissions to request.</param>
      public async Task<Dictionary<Permission, PermissionStatus>> VerifyPermissionsAsync(params Permission[] permissions)
      {
         Instance = this;
         _activity = App.MainActivity;
         if (_activity == null)
            return null;

         if (_taskCompletionSource != null && !_taskCompletionSource.Task.IsCompleted)
         {
            _taskCompletionSource.SetCanceled();
            _taskCompletionSource = null;
         }

         lock (_locker)
         {
            _permissionsResults = new Dictionary<Permission, PermissionStatus>();
         }

         var permissionsToRequest = new List<string>();
         foreach (var permission in permissions)
         {
            var result = await CheckPermissionAsync(permission);
            if (result != PermissionStatus.Granted)
            {
               var names = GetPermissionNamesFromManifest(permission);
               //check to see if we can find manifest names
               //if we can't add as unknown and continue
               if ((names?.Count ?? 0) == 0)
               {
                  lock (_locker)
                  {
                     if (!_permissionsResults.ContainsKey(permission))
                        _permissionsResults.Add(permission, PermissionStatus.Unknown);
                  }
                  continue;
               }

               permissionsToRequest.AddRange(names);
            }
            else
            {
               //if we are granted you are good!
               lock (_locker)
               {
                  if (!_permissionsResults.ContainsKey(permission))
                     _permissionsResults.Add(permission, PermissionStatus.Granted);
               }
            }
         }

         if (permissionsToRequest.Count == 0)
            return _permissionsResults;

         _taskCompletionSource = new TaskCompletionSource<Dictionary<Permission, PermissionStatus>>();

         ActivityCompat.RequestPermissions(_activity, permissionsToRequest.ToArray(), UNIQUE_PERMISSION_CODE);

         await _taskCompletionSource.Task;

         if(_permissionsResults.Count > 0)
         {
            foreach (KeyValuePair<Permission, PermissionStatus> entry in _permissionsResults)
            {
               if(_permissionsResults.ContainsKey(entry.Key) && _permissionsResults[entry.Key] != PermissionStatus.Granted)
                  ShouldShowRequestPermissionRationaleAsync(entry.Key);
            }
         }

         return _permissionsResults;
      }

      private Task<PermissionStatus> CheckPermissionAsync(Permission permission)
      {
         var names = GetPermissionNamesFromManifest(permission);

         //if isn't an android specific group then go ahead and return true;
         if (names == null)
         {
            Debug.WriteLine("No android specific permissions needed for: " + permission);
            return Task.FromResult(PermissionStatus.Granted);
         }

         //if no permissions were found then there is an issue and permission is not set in Android manifest
         if (names.Count == 0)
         {
            Debug.WriteLine("No permissions found in manifest for: " + permission);
            return Task.FromResult(PermissionStatus.Unknown);
         }

         var isMarshmallowOrHigher = _activity.ApplicationInfo.TargetSdkVersion >= Android.OS.BuildVersionCodes.M;

         foreach (var name in names)
         {
            if (isMarshmallowOrHigher)
            {
               if (ContextCompat.CheckSelfPermission(_activity, name) != Android.Content.PM.Permission.Granted)
                  return Task.FromResult(PermissionStatus.Denied);
            }
            else
            {
               if (PermissionChecker.CheckSelfPermission(_activity, name) != PermissionChecker.PermissionGranted)
                  return Task.FromResult(PermissionStatus.Denied);
            }
         }
         return Task.FromResult(PermissionStatus.Granted);
      }

      private void ShouldShowRequestPermissionRationaleAsync(Permission permission)
      {
         var names = GetPermissionNamesFromManifest(permission);

         var isMarshmallowOrHigher = _activity.ApplicationInfo.TargetSdkVersion >= Android.OS.BuildVersionCodes.M;

         foreach (var name in names)
         {
            if (isMarshmallowOrHigher)
            {
               bool shouldShowRequestPermissionRationale = false;
               if(permission != Permission.Storage) // For "Storage" permission if the user didn't grant access then always show the Rationale message and exit
                  shouldShowRequestPermissionRationale = ActivityCompat.ShouldShowRequestPermissionRationale(_activity, name);

               if (!shouldShowRequestPermissionRationale)
               {
                  // User checked the "Don't ask again" option inside the request permissions system dialog, so in this
                  // case we should help the user understand why your app needs a permission and ask him/her to enable
                  // this permission inside the system "Settings".
                  Xamarin.Forms.Page activePage = HomePage.Instance.Navigation.NavigationStack.LastOrDefault();
                  if (activePage != null)
                  {
                     string title = string.Format("Need access to {0}", permission.ToString());
                     string message = string.Format((permission == Permission.Storage) ? "This app needs Storage permission to save cards images, so please turn on Storage permission from phone settings." : "Please turn on {0} permission for this app.", permission.ToString());
                     var builder = new AlertDialog.Builder(_activity);
                     builder.SetTitle(title);
                     builder.SetMessage(message);
                     builder.SetPositiveButton("Go to Settings", (sender, args) =>
                     {
                        OpenAppSettings();
                        if (permission == Permission.Storage)
                           Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                     });
                     builder.SetNegativeButton((permission == Permission.Storage) ? "Exit" : "Cancel", (sender, args) =>
                     {
                        if (permission == Permission.Storage)
                           Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                     });
                     builder.SetCancelable(false);
                     builder.Show();
                  }
               }
            }
         }
      }

      const int UNIQUE_PERMISSION_CODE = 25;
      /// <summary>
      /// Callback for the result from requesting permissions. This method is invoked for every call on ActivityCompat.RequestPermissions
      /// </summary>
      /// <param name="requestCode"></param>
      /// <param name="permissions"></param>
      /// <param name="grantResults"></param>
      public void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
      {
         if (requestCode != UNIQUE_PERMISSION_CODE || grantResults.Length <= 0)
            return;

         if (_taskCompletionSource == null)
            return;

         for (var i = 0; i < permissions.Length; i++)
         {
            if (_taskCompletionSource.Task.Status == TaskStatus.Canceled)
               return;

            var permission = GetPermissionForManifestName(permissions[i]);
            if (permission == Permission.Unknown)
               continue;

            lock (_locker)
            {
               if (permission == Permission.Microphone)
               {
                  if (!_permissionsResults.ContainsKey(Permission.Speech))
                     _permissionsResults.Add(Permission.Speech, grantResults[i] == Android.Content.PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
               }
               else if (permission == Permission.Location)
               {
                  if (!_permissionsResults.ContainsKey(Permission.LocationAlways))
                     _permissionsResults.Add(Permission.LocationAlways, grantResults[i] == Android.Content.PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);

                  if (!_permissionsResults.ContainsKey(Permission.LocationWhenInUse))
                     _permissionsResults.Add(Permission.LocationWhenInUse, grantResults[i] == Android.Content.PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
               }

               if (!_permissionsResults.ContainsKey(permission))
                  _permissionsResults.Add(permission, grantResults[i] == Android.Content.PM.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
            }
         }

         _taskCompletionSource.TrySetResult(_permissionsResults);
      }

      static Permission GetPermissionForManifestName(string permission)
      {
         switch (permission)
         {
            case Manifest.Permission.ReadCalendar:
            case Manifest.Permission.WriteCalendar:
               return Permission.Calendar;
            case Manifest.Permission.Camera:
               return Permission.Camera;
            case Manifest.Permission.ReadContacts:
            case Manifest.Permission.WriteContacts:
            case Manifest.Permission.GetAccounts:
               return Permission.Contacts;
            case Manifest.Permission.AccessCoarseLocation:
            case Manifest.Permission.AccessFineLocation:
            case Manifest.Permission.AccessLocationExtraCommands:
            case Manifest.Permission.AccessMockLocation:
               return Permission.Location;
            case Manifest.Permission.RecordAudio:
               return Permission.Microphone;
            case Manifest.Permission.ReadPhoneState:
            case Manifest.Permission.CallPhone:
            case Manifest.Permission.ReadCallLog:
            case Manifest.Permission.WriteCallLog:
            case Manifest.Permission.AddVoicemail:
            case Manifest.Permission.UseSip:
            case Manifest.Permission.ProcessOutgoingCalls:
               return Permission.Phone;
            case Manifest.Permission.BodySensors:
               return Permission.Sensors;
            case Manifest.Permission.SendSms:
            case Manifest.Permission.ReceiveSms:
            case Manifest.Permission.ReadSms:
            case Manifest.Permission.ReceiveWapPush:
            case Manifest.Permission.ReceiveMms:
               return Permission.Sms;
            case Manifest.Permission.ReadExternalStorage:
            case Manifest.Permission.WriteExternalStorage:
               return Permission.Storage;
         }

         return Permission.Unknown;
      }

      List<string> GetPermissionNamesFromManifest(Permission permission)
      {
         var permissionNames = new List<string>();
         switch (permission)
         {
            case Permission.Calendar:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadCalendar))
                     permissionNames.Add(Manifest.Permission.ReadCalendar);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.WriteCalendar))
                     permissionNames.Add(Manifest.Permission.WriteCalendar);
               }
               break;
            case Permission.Camera:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.Camera))
                     permissionNames.Add(Manifest.Permission.Camera);
               }
               break;
            case Permission.Contacts:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadContacts))
                     permissionNames.Add(Manifest.Permission.ReadContacts);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.WriteContacts))
                     permissionNames.Add(Manifest.Permission.WriteContacts);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.GetAccounts))
                     permissionNames.Add(Manifest.Permission.GetAccounts);
               }
               break;
            case Permission.LocationAlways:
            case Permission.LocationWhenInUse:
            case Permission.Location:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.AccessCoarseLocation))
                     permissionNames.Add(Manifest.Permission.AccessCoarseLocation);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.AccessFineLocation))
                     permissionNames.Add(Manifest.Permission.AccessFineLocation);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.AccessLocationExtraCommands))
                     permissionNames.Add(Manifest.Permission.AccessLocationExtraCommands);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.AccessMockLocation))
                     permissionNames.Add(Manifest.Permission.AccessMockLocation);
               }
               break;
            case Permission.Speech:
            case Permission.Microphone:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.RecordAudio))
                     permissionNames.Add(Manifest.Permission.RecordAudio);
               }
               break;
            case Permission.Phone:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadPhoneState))
                     permissionNames.Add(Manifest.Permission.ReadPhoneState);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.CallPhone))
                     permissionNames.Add(Manifest.Permission.CallPhone);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadCallLog))
                     permissionNames.Add(Manifest.Permission.ReadCallLog);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.WriteCallLog))
                     permissionNames.Add(Manifest.Permission.WriteCallLog);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.AddVoicemail))
                     permissionNames.Add(Manifest.Permission.AddVoicemail);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.UseSip))
                     permissionNames.Add(Manifest.Permission.UseSip);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ProcessOutgoingCalls))
                     permissionNames.Add(Manifest.Permission.ProcessOutgoingCalls);
               }
               break;
            case Permission.Sensors:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.BodySensors))
                     permissionNames.Add(Manifest.Permission.BodySensors);
               }
               break;
            case Permission.Sms:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.SendSms))
                     permissionNames.Add(Manifest.Permission.SendSms);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReceiveSms))
                     permissionNames.Add(Manifest.Permission.ReceiveSms);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadSms))
                     permissionNames.Add(Manifest.Permission.ReadSms);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReceiveWapPush))
                     permissionNames.Add(Manifest.Permission.ReceiveWapPush);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReceiveMms))
                     permissionNames.Add(Manifest.Permission.ReceiveMms);
               }
               break;
            case Permission.Storage:
               {
                  if (PermissionSelectedInsideManifest(Manifest.Permission.ReadExternalStorage))
                     permissionNames.Add(Manifest.Permission.ReadExternalStorage);

                  if (PermissionSelectedInsideManifest(Manifest.Permission.WriteExternalStorage))
                     permissionNames.Add(Manifest.Permission.WriteExternalStorage);
               }
               break;
            default:
               return null;
         }

         return permissionNames;
      }

      private bool PermissionSelectedInsideManifest(string permission)
      {
         try
         {
            if (_requestedPermissions != null)
               return _requestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));

            var info = _activity.PackageManager.GetPackageInfo(_activity.PackageName, Android.Content.PM.PackageInfoFlags.Permissions);
            if (info == null)
            {
               Debug.WriteLine("Unable to get Package info, will not be able to determine permissions to request.");
               return false;
            }

            _requestedPermissions = info.RequestedPermissions;

            if (_requestedPermissions == null)
            {
               Debug.WriteLine("There are no requested permissions, please check to ensure you have marked permissions you want to request.");
               return false;
            }

            return _requestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));
         }
         catch (Exception ex)
         {
            Console.Write("Unable to check manifest for permission: " + ex);
         }
         return false;
      }

      /// <summary>
      /// Opens settings to app page
      /// </summary>
      /// <returns>true if could open.</returns>
      private bool OpenAppSettings()
      {
         try
         {
            var settingsIntent = new Intent();
            settingsIntent.SetAction(Android.Provider.Settings.ActionApplicationDetailsSettings);
            settingsIntent.AddCategory(Intent.CategoryDefault);
            settingsIntent.SetData(Android.Net.Uri.Parse("package:" + _activity.PackageName));
            settingsIntent.AddFlags(ActivityFlags.NewTask);
            settingsIntent.AddFlags(ActivityFlags.NoHistory);
            settingsIntent.AddFlags(ActivityFlags.ExcludeFromRecents);
            Android.App.Application.Context.StartActivity(settingsIntent);
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            return false;
         }
      }
   }
}