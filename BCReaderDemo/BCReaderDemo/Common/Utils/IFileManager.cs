// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************

using System.Threading.Tasks;

namespace Leadtools.Demos.Utils
{
   public interface IFileManager
   {
      Task MoveFile(string sourceFilePath, string targetFilePath);
      Task DeleteFile(string filePath);
   }
}
