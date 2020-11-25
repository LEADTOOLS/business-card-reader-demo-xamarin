// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Document.Writer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace Leadtools.Demos.Document.Utils
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public enum DocumentScanType
   {
      Text,
      Document
   }

   [XmlRoot]
   public enum OcrOutputFormat
   {
      None,
      Pdf,
      PdfEmbeddedFonts,
      PdfA,
      PdfImageOverText,
      PdfImageOverTextEmbeddedFonts,
      PdfAImageOverText,
      Docx,
      DocxFramed,
      Rtf,
      RtfFramed,
      Text,
      TextFormatted,
      Svg
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public static class OcrOutputFormatExtensions
   {
      #region Internal properties

      private static Dictionary<OcrOutputFormat, DocumentOptions> OptionsCache { get; } = new Dictionary<OcrOutputFormat, DocumentOptions>();
      private static DocumentWriter Writer { get; } = new DocumentWriter();

      #endregion

      #region Methods

      #region Helpers

      public static string GetDescription(this OcrOutputFormat format)
      {
         switch (format)
         {
            default:
            case OcrOutputFormat.None:
               return "None";
            case OcrOutputFormat.Docx:
               return "DOCX";
            case OcrOutputFormat.DocxFramed:
               return "DOCX Framed";
            case OcrOutputFormat.Pdf:
               return "PDF";
            case OcrOutputFormat.PdfA:
               return "PDF/A";
            case OcrOutputFormat.PdfAImageOverText:
               return "PDF/A Image Over Text";
            case OcrOutputFormat.PdfEmbeddedFonts:
               return "PDF Embedded Fonts";
            case OcrOutputFormat.PdfImageOverText:
               return "PDF Image Over Text";
            case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
               return "PDF Image Over Text | Embedded Fonts";
            case OcrOutputFormat.Rtf:
               return "RTF";
            case OcrOutputFormat.RtfFramed:
               return "RTF Framed";
            case OcrOutputFormat.Svg:
               return "SVG";
            case OcrOutputFormat.Text:
               return "Text";
            case OcrOutputFormat.TextFormatted:
               return "Text Formatted";
         }
      }

      public static string GetFormatName(this OcrOutputFormat format)
      {
         switch (format)
         {
            default:
            case OcrOutputFormat.None:
               return "None";
            case OcrOutputFormat.Docx:
            case OcrOutputFormat.DocxFramed:
               return "DOCX";
            case OcrOutputFormat.Pdf:
            case OcrOutputFormat.PdfA:
            case OcrOutputFormat.PdfAImageOverText:
            case OcrOutputFormat.PdfEmbeddedFonts:
            case OcrOutputFormat.PdfImageOverText:
            case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
               return "PDF";
            case OcrOutputFormat.Rtf:
            case OcrOutputFormat.RtfFramed:
               return "RTF";
            case OcrOutputFormat.Svg:
               return "SVG";
            case OcrOutputFormat.Text:
            case OcrOutputFormat.TextFormatted:
               return "Text";
         }
      }

      public static string GetFormatExtention(this OcrOutputFormat format)
      {
         switch (format)
         {
            default:
            case OcrOutputFormat.None:
               return string.Empty;
            case OcrOutputFormat.Docx:
            case OcrOutputFormat.DocxFramed:
               return ".docx";
            case OcrOutputFormat.Pdf:
            case OcrOutputFormat.PdfA:
            case OcrOutputFormat.PdfAImageOverText:
            case OcrOutputFormat.PdfEmbeddedFonts:
            case OcrOutputFormat.PdfImageOverText:
            case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
               return ".pdf";
            case OcrOutputFormat.Rtf:
            case OcrOutputFormat.RtfFramed:
               return ".rtf";
            case OcrOutputFormat.Svg:
               return ".svg";
            case OcrOutputFormat.Text:
            case OcrOutputFormat.TextFormatted:
               return ".txt";
         }
      }

      public static DocumentFormat GetDocumentFormat(this OcrOutputFormat format)
      {
         switch (format)
         {
            case OcrOutputFormat.Docx:
            case OcrOutputFormat.DocxFramed:
               return DocumentFormat.Docx;
            case OcrOutputFormat.Pdf:
            case OcrOutputFormat.PdfA:
            case OcrOutputFormat.PdfAImageOverText:
            case OcrOutputFormat.PdfEmbeddedFonts:
            case OcrOutputFormat.PdfImageOverText:
            case OcrOutputFormat.PdfImageOverTextEmbeddedFonts:
               return DocumentFormat.Pdf;
            case OcrOutputFormat.Rtf:
            case OcrOutputFormat.RtfFramed:
               return DocumentFormat.Rtf;
            case OcrOutputFormat.Svg:
               return DocumentFormat.Svg;
            case OcrOutputFormat.Text:
            case OcrOutputFormat.TextFormatted:
            default:
               return DocumentFormat.Text;
         }
      }

      public static DocumentOptions GetDocumentOptions(this OcrOutputFormat format)
      {
         // Not in cache?
         if (!OptionsCache.TryGetValue(format, out DocumentOptions options))
         {
            // Configure the options
            options = Writer.GetOptions(format.GetDocumentFormat()).Clone();
            switch (options)
            {
               case DocxDocumentOptions docxOptions:
                  if (format == OcrOutputFormat.DocxFramed)
                     docxOptions.TextMode = DocumentTextMode.Framed;
                  break;
               case PdfDocumentOptions pdfOptions:
                  if (format == OcrOutputFormat.PdfA || format == OcrOutputFormat.PdfAImageOverText)
                     pdfOptions.DocumentType = PdfDocumentType.PdfA;
                  if (format == OcrOutputFormat.PdfEmbeddedFonts || format == OcrOutputFormat.PdfImageOverTextEmbeddedFonts)
                     pdfOptions.FontEmbedMode = DocumentFontEmbedMode.All;
                  if (format == OcrOutputFormat.PdfAImageOverText || format == OcrOutputFormat.PdfImageOverText || format == OcrOutputFormat.PdfImageOverTextEmbeddedFonts)
                     pdfOptions.ImageOverText = true;
                  break;
               case RtfDocumentOptions rtfOptions:
                  if (format == OcrOutputFormat.RtfFramed)
                     rtfOptions.TextMode = DocumentTextMode.Framed;
                  break;
               case TextDocumentOptions textOptions:
                  if (format == OcrOutputFormat.TextFormatted)
                     textOptions.Formatted = true;
                  break;
            }

            // Add to cache
            OptionsCache.Add(format, options);
         }

         // Return a copy
         return options.Clone();
      }

      #endregion

      #endregion
   }

   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class DocumentFormatToIsVisibleConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         string param = System.Convert.ToString(parameter) ?? string.Empty;

         bool ret = (!string.IsNullOrWhiteSpace(param) && param.Equals("Equal")) ? (OcrOutputFormat)value == OcrOutputFormat.None : (OcrOutputFormat)value != OcrOutputFormat.None;

         return ret;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotSupportedException();
      }
   }
}
