// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using Leadtools.Annotations.Engine;

namespace Leadtools.Annotations.UserMedicalPack
{
   public class AnnFourParallelLinesDrawer : AnnParallelLinesDrawer
   {
      public AnnFourParallelLinesDrawer(IAnnAutomationControl automationControl, AnnContainer container, AnnParallelLinesObject annParallelLinesObject)
         : base(automationControl, container, annParallelLinesObject)
      {
      }

      public override int LinesCount
      {
         get
         {
            return 4;
         }
      }
   }
}
