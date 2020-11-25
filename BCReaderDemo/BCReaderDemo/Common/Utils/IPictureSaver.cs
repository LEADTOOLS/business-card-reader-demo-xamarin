// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************

using System.IO;
using System.Threading.Tasks;

namespace Leadtools.Demos.Utils
{
   public interface IPictureSaver
   {
      /// <summary>
      /// Saves image stream to device.
      /// </summary>
      /// <param name="stream">Image stream</param>
      /// <param name="name">Image path to save to, if this parameter is null or only cotnains file name without path then the image will be saved to device's gallery.</param>
      /// <returns></returns>
      Task<bool> SaveImage(Stream stream, string fileName, bool saveToPhotoGalleryAsWell);

      /// <summary>
      /// Saves raster image to device.
      /// </summary>
      /// <param name="image">Source raster image</param>
      /// <param name="name">Image path to save to, if this parameter is null or only cotnains file name without path then the image will be saved to device's 'Pictures' directory.</param>
      /// <returns></returns>
      Task<bool> SaveImage(RasterImage image, string fileName, bool saveToPhotoGalleryAsWell);

      /// <summary>
      /// Saves image stream to device.
      /// </summary>
      /// <param name="stream">Image stream</param>
      /// <param name="name">Image path to save to, if this parameter is null or only cotnains file name without path then the image will be saved to device's 'Pictures' gallery.</param>
      /// <param name="resolution">One of the already defined image resolutions that determines the saved image dimensions.</param>
      /// <returns></returns>
      Task<bool> SaveImage(Stream stream, string fileName, PictureSaveResolution resolution, bool saveToPhotoGalleryAsWell);

      /// <summary>
      /// Saves raster image to device.
      /// </summary>
      /// <param name="image">Source raster image</param>
      /// <param name="name">Image path to save to, if this parameter is null or only cotnains file name without path then the image will be saved to device's 'Pictures' directory.</param>
      /// <param name="resolution">One of the already defined image resolutions that determines the saved image dimensions.</param>
      /// <returns></returns>
      Task<bool> SaveImage(RasterImage image, string fileName, PictureSaveResolution resolution, bool saveToPhotoGalleryAsWell);

      /// <summary>
      /// ONLY used from UWP platform to select XML file save location
      /// </summary>
      /// <returns></returns>
      Task<Stream> SaveToXmlFile();
   }
}
