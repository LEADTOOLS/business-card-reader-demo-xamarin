// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Leadtools.Demos.Utils
{
   public enum LoadFilesSource
   {
      Gallery,
      Disk
   }

   public interface IPicturePicker
   {
      #region Methods

      // Used for loading single image from gallery as stream object.
      Task<Stream> GetImageStreamAsync();

      // Used for loading single or multiple files from gallery or disk/cloud, it will return list of the selected images paths
      // and its up to you to load them the way you see fit.
      Task<List<string>> LoadFilesAsync(LoadFilesSource source, bool allowMultiSelect);

      // Used inside the CameraDemo to view/play files inside the gallery application itself.
      Task<bool> BringUpGallery(string path);

      // Load specific file/document from phone and preview it (Used inside ConverterDemo).
      Task<bool> PreviewFile(string filePath);

      #endregion
   }
}
