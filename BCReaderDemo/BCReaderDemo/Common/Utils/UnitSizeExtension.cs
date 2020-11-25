// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [ContentProperty(nameof(Argument))]
   public abstract class UnitSizeExtension : IMarkupExtension
   {
      public UnitSizeExtension(double unitSize)
      {
         UnitSize = unitSize;
      }

      #region Internal properties

      private double UnitSize { get; }

      #endregion

      #region Public properties

      public string Argument { get; set; }

      #endregion

      #region Methods

      #region IMarkupExtension

      public object ProvideValue(IServiceProvider serviceProvider)
      {
         // For throwing parsing error
         IXmlLineInfo lineInfo;
         if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider)
            lineInfo = lineInfoProvider.XmlLineInfo;
         else
            lineInfo = new XmlLineInfo();

         // Null Argument?
         if (string.IsNullOrWhiteSpace(Argument))
            throw new XamlParseException($"{GetType().Name} requires {nameof(Argument)} property to be set", lineInfo);

         // Parse Argument
         object value = ParseValue(lineInfo);
         CheckValue(serviceProvider, lineInfo, value);
         return value;
      }

      #endregion

      #region Helpers

      private static void CheckValue(IServiceProvider serviceProvider, IXmlLineInfo lineInfo, object value)
      {
         if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget && provideValueTarget.TargetProperty is BindableProperty bindableProperty && bindableProperty.ReturnType != value.GetType())
            throw new XamlParseException($"Unable to parse {nameof(Argument)} as {bindableProperty.ReturnType.Name} for {provideValueTarget.TargetObject.GetType().FullName}.{bindableProperty.PropertyName}", lineInfo);
      }

      private object ParseValue(IXmlLineInfo lineInfo)
      {
         // Thickness?
         if (Argument.IndexOf(',') >= 0)
         {
            string[] parts = Argument.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (parts.Length)
            {
               case 1:
                  // All,
                  if (!double.TryParse(parts[0], out double all))
                     throw new XamlParseException($"Unable to parse {nameof(Argument)} as All,", lineInfo);
                  return new Thickness(all * UnitSize);
               case 2:
                  // Horizontal,Vertical
                  if (!double.TryParse(parts[0], out double horizontal) || !double.TryParse(parts[1], out double vertical))
                     throw new XamlParseException($"Unable to parse {nameof(Argument)} as H,V", lineInfo);
                  return new Thickness(horizontal * UnitSize, vertical * UnitSize);
               case 4:
                  // L,T,R,B
                  if (!double.TryParse(parts[0], out double left) || !double.TryParse(parts[1], out double top) ||
                     !double.TryParse(parts[2], out double right) || !double.TryParse(parts[3], out double bottom))
                     throw new XamlParseException($"Unable to parse {nameof(Argument)} as L,T,R,B");
                  return new Thickness(left * UnitSize, top * UnitSize, right * UnitSize, bottom * UnitSize);
               default:
                  throw new XamlParseException($"Unable to parse {nameof(Argument)} with {parts.Length} comma(s)", lineInfo);
            }
         }

         // GridLength?
         if (Argument.EndsWith("GL", StringComparison.InvariantCultureIgnoreCase))
            if (double.TryParse(Argument.Substring(0, Argument.Length - 2), out double gridLengthFactor))
               return new GridLength(gridLengthFactor * UnitSize);
            else
               throw new XamlParseException($"Unable to parse {nameof(Argument)} for GridLength", lineInfo);

         // Float?
         if (Argument.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
            if (double.TryParse(Argument.Substring(0, Argument.Length - 1), out double floatFactor))
               return (float)(floatFactor * UnitSize);
            else
               throw new XamlParseException($"Unable to parse {nameof(Argument)} for float", lineInfo);

         // Fallback to double
         if (double.TryParse(Argument, out double doubleFactor))
            return doubleFactor * UnitSize;
         throw new XamlParseException($"Unable to parse {nameof(Argument)}", lineInfo);
      }

      #endregion

      #endregion
   }
}
