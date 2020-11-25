// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System.IO;
using System.Threading.Tasks;

namespace DataService
{
   public interface ISaveContact
   {
      void SaveContact(ContactModel contact);
   }

   public interface IShareContact
   {
      Task Show(string filePath);
   }
}
