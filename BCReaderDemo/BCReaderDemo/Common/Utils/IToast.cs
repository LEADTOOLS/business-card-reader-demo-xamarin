// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Threading.Tasks;

namespace Leadtools.Demos.Utils
{
   public interface IToast
   {
      #region Methods

      Task Show(string text, bool longDuration);

      #endregion
   }
}
