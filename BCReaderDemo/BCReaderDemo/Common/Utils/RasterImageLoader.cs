// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Codecs;
using System.IO;
using System.Threading.Tasks;

namespace Leadtools.Demos.Utils
{
   public static class RasterImageLoader
   {
      public static async Task<RasterImage> RasterImageFromStream(Stream stream, bool loadAllPages = false, int firstPageNumber = 1, int lastPageNumber = 1)
      {
         RasterImage rasterImage = null;

         using (RasterCodecs codecs = new RasterCodecs())
         {
            codecs.Options.Png.Save.QualityFactor = 10;
            codecs.Options.RasterizeDocument.Load.Resolution = 300;
            codecs.Options.Load.Resolution = 300;
            codecs.Options.Load.AllPages = loadAllPages;
            using (var leadStream = LeadStream.Factory.FromStream(stream))
            {
               rasterImage = await codecs.LoadAsync(leadStream, 0, CodecsLoadByteOrder.Bgr, firstPageNumber, lastPageNumber);
               if (rasterImage.ViewPerspective != RasterViewPerspective.TopLeft)
                  rasterImage.ChangeViewPerspective(RasterViewPerspective.TopLeft);
            }
         }

         return rasterImage;
      }

      public static async Task<RasterImage> RasterImageFromFile(string file, bool loadAllPages = false, int firstPageNumber = 1, int lastPageNumber = 1)
      {
         RasterImage rasterImage = null;

         using (var codecs = new RasterCodecs())
         {
            codecs.Options.Png.Save.QualityFactor = 10;
            codecs.Options.RasterizeDocument.Load.Resolution = 300;
            codecs.Options.Load.Resolution = 300;
            codecs.Options.Load.AllPages = loadAllPages;
            using (var inputStream = LeadStream.Factory.OpenFile(file))
            {
               rasterImage = await codecs.LoadAsync(inputStream, 0, CodecsLoadByteOrder.Bgr, firstPageNumber, lastPageNumber);
               if (rasterImage.ViewPerspective != RasterViewPerspective.TopLeft)
                  rasterImage.ChangeViewPerspective(RasterViewPerspective.TopLeft);
            }
         }

         return rasterImage;
      }

      public static async Task<Stream> StreamFromRasterImage(RasterImage image)
      {
         using (RasterCodecs codecs = new RasterCodecs())
         {
            codecs.Options.Png.Save.QualityFactor = 10;
            var stream = new MemoryStream();
            using (var leadStream = LeadStream.Factory.FromStream(stream))
            {
               await codecs.SaveAsync(image, leadStream, RasterImageFormat.Png, 0);
               stream.Seek(0, SeekOrigin.Begin);
               return stream;
            }
         }
      }
   }
}
