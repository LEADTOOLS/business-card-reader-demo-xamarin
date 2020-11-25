// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class NavigationListView : CustomListView
   {
      public NavigationListView()
      {
         // Setup
         ItemTapped += CustomListView_ItemTapped;
         ShowSelection = false;

         // Initialize
         InitializeComponent();
      }

      #region Methods

      #region Events

      private async void CustomListView_ItemTapped(object sender, ItemTappedEventArgs e)
      {
         if (e.Item is NavigationListViewEntry entry)
            await (entry.TapCallback?.Invoke() ?? Task.CompletedTask);
      }

      #endregion

      #endregion
   }

   public class NavigationListViewEntry
   {
      public NavigationListViewEntry(string labelText, Func<Task> tapCallback)
      {
         LabelText = labelText;
         TapCallback = tapCallback;
      }

      #region Public properties

      public string LabelText { get; }
      public Func<Task> TapCallback { get; }

      #endregion

      #region Methods

      #region Helpers

      public override string ToString() => LabelText;
      public static implicit operator string(NavigationListViewEntry entry) => entry.LabelText;

      #endregion

      #endregion
   }
}