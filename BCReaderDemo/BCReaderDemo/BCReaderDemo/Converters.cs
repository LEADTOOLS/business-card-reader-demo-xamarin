// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Globalization;
using Xamarin.Forms;

namespace BCReaderDemo
{
   public static class PlatformsConstants
   {
      public static readonly double SwitchControlFontSize            = (Device.Idiom == TargetIdiom.Phone && Device.RuntimePlatform == Device.iOS) ? Device.GetNamedSize(NamedSize.Micro  , typeof(Label)) : Device.GetNamedSize(NamedSize.Small   , typeof(Label));
      public static readonly double HeaderTextFontSize               = (Device.Idiom == TargetIdiom.Phone && Device.RuntimePlatform == Device.iOS) ? Device.GetNamedSize(NamedSize.Micro  , typeof(Label)) : Device.GetNamedSize(NamedSize.Small   , typeof(Label));
      public static readonly double PageTitleFontSize                = (Device.Idiom == TargetIdiom.Phone && Device.RuntimePlatform == Device.iOS) ? Device.GetNamedSize(NamedSize.Micro  , typeof(Label)) : Device.GetNamedSize(NamedSize.Small   , typeof(Label));
      public static readonly double AboutTextHeaderFontSize          = (Device.Idiom == TargetIdiom.Phone)                                         ? Device.GetNamedSize(NamedSize.Default, typeof(Label)) : Device.GetNamedSize(NamedSize.Medium  , typeof(Label));
      public static readonly double AboutTextFontSize                = (Device.Idiom == TargetIdiom.Phone)                                         ? 13                                                    : Device.GetNamedSize(NamedSize.Small   , typeof(Label));
      public static readonly double FontSizeForPhonesOnly            = (Device.Idiom == TargetIdiom.Phone)                                         ? Device.GetNamedSize(NamedSize.Micro  , typeof(Label)) : Device.GetNamedSize(NamedSize.Default , typeof(Label));
      public static readonly double SmallFontSizeForPhonesOnly       = (Device.Idiom == TargetIdiom.Phone)                                         ? Device.GetNamedSize(NamedSize.Small  , typeof(Label)) : Device.GetNamedSize(NamedSize.Default , typeof(Label));
      public static readonly double TableViewGroupHeaderFontSize     = (Device.RuntimePlatform == Device.iOS)                                      ? Device.GetNamedSize(NamedSize.Small  , typeof(Label)) : Device.GetNamedSize(NamedSize.Default , typeof(Label));

      public static readonly double TableViewFieldLabelFontSize      = 12;
      public static readonly double TableViewFieldEntryFontSize      = 12;
      public static readonly double ListViewGroupHeaderFontSize      = (Device.Idiom == TargetIdiom.Phone) ? 12 : 14;

      public static readonly double CardsPageListViewItemHeight      = (Device.Idiom == TargetIdiom.Phone) ? 70 : 80;
      public static readonly double CardThumbnailWidth               = 75;
      public static readonly double CardThumbnailHeight              = 50;
      public static readonly double CardHolderNameLabelFontSize      = (Device.Idiom == TargetIdiom.Phone) ? 12 : 14;
      public static readonly double CardHolderCompanyLabelFontSize   = (Device.Idiom == TargetIdiom.Phone) ? 9 : 12;

      public static readonly double AdRowHeight                      = 25;
      public static readonly double RowHeight                        = 50;
      public static readonly double PagesHeaderTitleRowHeight        = 50;
      public static readonly double PagesHeaderOtherRowsHeight       = 40;

      public static readonly GridLength ListViewRowHeight            = new GridLength(50, GridUnitType.Absolute);
      public static readonly GridLength SwitchListViewRowHeight      = new GridLength(40, GridUnitType.Absolute);
   }

   public static class CustomColors
   {
      public static readonly Color LightSharkonColor                                = Color.FromHex("#8ea5ba");
      public static readonly Color DarkSharkonColor                                 = Color.FromHex("#57687f");
      public static readonly Color LightSilverColor                                 = Color.FromHex("#ccd8e2");
      public static readonly Color LightBlueColor                                   = Color.FromHex("#72c3f8");
      public static readonly Color RedButtonColor                                   = Color.FromHex("#ff6d68");
      public static readonly Color BlueButtonColor                                  = Color.FromHex("#14b2e1");
      public static readonly Color PinkButtonColor                                  = Color.FromHex("#ff5782");
      public static readonly Color SettingsLabelsColor                              = Color.FromHex("#a0b3c7");
      public static readonly Color SearchBarPlaceHolderInactiveLightBlueTextColor   = Color.FromHex("#89d8f0");
      public static readonly Color SearchBarPlaceHolderInactiveTextColor            = Color.FromHex("#b6c6d1");
      public static readonly Color SearchBarPlaceHolderActiveTextColor              = DarkSharkonColor;
      public static readonly Color PagesBackgroundColor                             = Color.FromHex("#f2f6ff");
      public static readonly Color DimmedPageBackgroundColor                        = Color.FromHex("#f5f7fa");
      public static readonly Color RetakeButtonColor                                = Color.FromHex("#5cc4cc");
      public static readonly Color LinkBlueColor                                    = Color.FromHex("#69a2fd");
      public static readonly Color AdLEADTOOLSColor                                 = Color.FromHex("#4444e0");
      public static readonly Color AdTextColor                                      = Color.FromHex("#2f3d4c");

      public static readonly Color FacebookTextColor                                = Color.FromHex("#3e5c9a");
      public static readonly Color LinkedInTextColor                                = Color.FromHex("#117bb8");
      public static readonly Color TwitterTextColor                                 = Color.FromHex("#29a9ff");
      public static readonly Color YoutubeTextColor                                 = Color.FromHex("#c3271a");
      public static readonly Color SmsTextColor                                     = Color.FromHex("#54eadf");
      public static readonly Color EmailTextColor                                   = Color.FromHex("#ff6d68");
      public static readonly Color ScanQRLabelTextColor                             = Color.FromHex("#698198");
      public static readonly Color RecommendToFriendIconsBackgroundColor            = Color.FromHex("#f7f8f9");
      public static readonly Color CopyrightTextColor                               = DarkSharkonColor;
   }
        
   class DateConverter : IValueConverter
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

   class NegateBooleanConverter : IValueConverter
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

   public class NullStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         string retValue = "[Unknown]";
         string param = System.Convert.ToString(parameter) ?? "c";

         if (param == "c")
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
}
