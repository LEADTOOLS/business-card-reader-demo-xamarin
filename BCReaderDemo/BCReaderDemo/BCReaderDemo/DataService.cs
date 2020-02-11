// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System.IO;
using System.Threading.Tasks;

namespace DataService
{
   public interface IPicturePicker
   {
      Task<Stream> GetImageStreamAsync();
   }
   public interface ISaveContact
   {
      void SaveContact(ContactModel contact);
   }

   public enum PictureSaveResolution
   {
      Low,
      Medium,
      High,
   }

   public interface IPictureSaver
   {
      Task<bool> SaveImage(Stream stream, string filePath, PictureSaveResolution resolution);
   }

   public interface IShareContact
   {
      Task Show(string filePath);
   }

   public interface IStatusBar
   {
      void HideStatusBar();
      void ShowStatusBar();
   }
}
