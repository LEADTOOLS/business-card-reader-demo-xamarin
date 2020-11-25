// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class CustomViewCell : ViewCell
   {
      #region Internal properties

      internal CustomListView ParentList { get; set; }

      #endregion

      #region Public properties

      private bool _showSelection = false;
      public bool ShowSelection
      {
         get { return ParentList?.ShowSelection ?? _showSelection; }
         set { _showSelection = value; if(ParentList != null) ParentList.ShowSelection = value; }
      }

      #endregion

      #region Methods

      #region OnXXX

      protected override void OnParentSet()
      {
         base.OnParentSet();

         // Update cached parent list
         Element parent = Parent;
         ParentList = null;
         while (parent != null)
            if (parent is CustomListView parentList)
            {
               ParentList = parentList;
               break;
            }
            else
               parent = parent.Parent;
      }

      #endregion

      #endregion
   }
}