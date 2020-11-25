// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Leadtools.Demos.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DateConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         DateTime date = (DateTime)value;
         if (date.Equals(DateTime.Today))
         {
            return "Today";
         }
         return date.Day.ToString().PadLeft(2, '0') + @"/" + date.Month.ToString().PadLeft(2, '0') + "-" + date.Year;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class NegateBooleanConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return !(bool)value;
      }
      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return !(bool)value;
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class StringCaseConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (string.IsNullOrEmpty(value as string))
            return string.Empty;

         string param = System.Convert.ToString(parameter) ?? "u";

         switch (param.ToUpper())
         {
            case "U":
               return ((string)value).ToUpper();
            case "L":
               return ((string)value).ToLower();
            default:
               return ((string)value);
         }

      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotSupportedException();
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class NullStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter /* Parameter should contain default return value string*/, CultureInfo culture)
      {
         string retValue = (parameter != null) ? System.Convert.ToString(parameter) : "N/A";

         if (value is char)
         {
            char c = (char)value;
            if (c != '\0')
               retValue = c.ToString();
         }
         else
         {
            if (!string.IsNullOrWhiteSpace(value as string))
               retValue = (value as string);
         }

         return retValue;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotSupportedException();
      }
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class OpacityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return !(value is bool @bool) || !@bool ? 0.0 : 1.0;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return !(value is double @double) || @double == 0.0 ? false : true;
      }
   }
}
