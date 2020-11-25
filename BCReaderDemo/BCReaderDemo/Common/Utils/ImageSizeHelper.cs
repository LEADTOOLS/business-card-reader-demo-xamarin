// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;

namespace Leadtools.Demos.Utils
{
   public enum PictureSaveResolution
   {
      Default,
      Low,
      Medium,
      High,
   }

   public static class ImageSizeHelper
   {
      public static LeadSize GetImageSize(int width, int height, PictureSaveResolution resolution)
      {
         int max = 0;

         if (resolution == PictureSaveResolution.Low)
         {
            max = 1000;
         }
         else if (resolution == PictureSaveResolution.Medium)
         {
            max = 2000;
         }
         else
         {
            max = 3000;
         }

         int newWidth = width;
         int newHeight = height;

         int maxWidthHeight = Math.Max(newWidth, newHeight);
         if (maxWidthHeight > max)
         {
            if (width == maxWidthHeight)
            {
               newWidth = max;
               newHeight = (int)((float)max / maxWidthHeight * newHeight);
            }
            else
            {
               newHeight = max;
               newWidth = (int)((float)max / maxWidthHeight * newWidth);
            }
         }

         return new LeadSize(newWidth, newHeight);
      }
   }
}
