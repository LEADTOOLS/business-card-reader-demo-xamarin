// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.Reflection;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{
   public static class Helpers
   {
      public static EmailType EmailStringToType(string emailString)
      {
         if (emailString.Equals("Personal Email"))
            return EmailType.Personal;
         else
            return EmailType.Work;
      }

      public static string EmailTypeToString(EmailType type)
      {
         if (type == EmailType.Personal)
            return "Personal Email";
         else
            return "Work Email";
      }

      public static PhoneType PhoneStringToType(string phoneString)
      {
         switch (phoneString)
         {
            case "Work Fax":
               return PhoneType.WorkFax;
            case "Work Mobile":
               return PhoneType.WorkMobile;
            case "Home Fax":
               return PhoneType.HomeFax;
            case "Home Mobile":
               return PhoneType.HomeMobile;
            case "Home Tel":
               return PhoneType.Home;
            case "Work Tel":
               return PhoneType.Work;
            default:
               return PhoneType.Work;
         }
      }

      public static string PhoneTypeToString(PhoneType type)
      {
         switch (type)
         {
            case PhoneType.WorkFax:
               return "Work Fax";
            case PhoneType.WorkMobile:
               return "Work Mobile";
            case PhoneType.HomeFax:
               return "Home Fax";
            case PhoneType.HomeMobile:
               return "Home Mobile";
            default:
               return type.ToString() + " Tel";
         }
      }
   }

   public class CallDataMethod : TriggerAction<VisualElement>
   {
      public string Method { get; set; }

      protected override void Invoke(VisualElement sender)
      {
         MethodInfo method = sender.GetType().GetRuntimeMethod(Method, new Type[0]);
         if (method != null)
         {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
               Device.BeginInvokeOnMainThread(() => { method.Invoke(sender, null); });
            }
         }
      }
   }
}
