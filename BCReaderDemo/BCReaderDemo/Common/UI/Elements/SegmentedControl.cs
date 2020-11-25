// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Utils;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Leadtools.Demos.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   public class SegmentedControl : Frame, INotifyPropertyChanged
   {
      private Color SelectedSegmentColor = Color.FromHex("#56a7fd");
      private double SegmentLabelFontSize = 12;

      private ContentView _firstSegmentView = null;
      private ContentView _secondSegmentView = null;
      private Label _firstSegmentLabel = null;
      private Label _secondSegmentLabel = null;

      public event EventHandler SegmentChanged;
      public SegmentedControl()
      {
         var tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Tapped += SegmentedControl_Tapped;

         // First segment controls
         _firstSegmentLabel = new Label()
         {
            Text = "First",
            FontSize = SegmentLabelFontSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
         };
         _firstSegmentView = new ContentView()
         {
            BackgroundColor = SelectedSegmentColor,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            StyleId = "FirstSegment",
            Content = _firstSegmentLabel,
         };
         _firstSegmentView.GestureRecognizers.Add(tapGestureRecognizer);

         // Second segment controls
         _secondSegmentLabel = new Label()
         {
            Text = "Second",
            FontSize = SegmentLabelFontSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
         };
         _secondSegmentView = new ContentView()
         {
            BackgroundColor = Color.Transparent,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            StyleId = "SecondSegment",
            Content = _secondSegmentLabel
         };
         _secondSegmentView.GestureRecognizers.Add(tapGestureRecognizer);

         Grid containerGrid = new Grid()
         {
            ColumnDefinitions =
            {
               new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
               new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },

            ColumnSpacing = 0,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,

            Children =
            {
               _firstSegmentView,
               _secondSegmentView
            }
         };

         Grid.SetColumn(_firstSegmentView, 0);
         Grid.SetColumn(_secondSegmentView, 1);

         BackgroundColor = Color.Transparent;
         BorderColor = SelectedSegmentColor;
         Margin = new Thickness(0);
         Padding = new Thickness(0);
         Content = containerGrid;
         IsClippedToBounds = true;
         CornerRadius = 15;
      }

      private void SegmentedControl_Tapped(object sender, EventArgs e)
      {
         ContentView view = sender as ContentView;
         if (view == null) return;

         bool segmentChanged = false;
         if (view.StyleId.Equals("FirstSegment"))
         {
            if (_selectedSegment != 0)
            {
               _selectedSegment = 0;
               segmentChanged = true;
            }
         }
         else
         {
            if (_selectedSegment != 1)
            {
               _selectedSegment = 1;
               segmentChanged = true;
            }
         }

         if (segmentChanged)
            OnSelectedSegmentChanged();
      }

      private void OnSelectedSegmentChanged()
      {
         ContentView selectedSegmentView = (_firstSegmentView.Parent as Grid).Children[_selectedSegment] as ContentView;
         foreach (ContentView v in (selectedSegmentView.Parent as Grid).Children)
            v.BackgroundColor = Color.Transparent;

         selectedSegmentView.BackgroundColor = SelectedSegmentColor;
         SegmentChanged?.Invoke(this, new EventArgs());
      }

      private int _selectedSegment = 0;
      public int SelectedSegment
      {
         get { return _selectedSegment; }
         set
         {
            if (value != _selectedSegment)
            {
               _selectedSegment = value;
               OnSelectedSegmentChanged();
            }
         }
      }

      public string FirstSegmentText
      {
         get { return (string.IsNullOrWhiteSpace(_firstSegmentLabel.Text) ? "First" : _firstSegmentLabel.Text); }
         set { _firstSegmentLabel.Text = value; }
      }

      public string SecondSegmentText
      {
         get { return (string.IsNullOrWhiteSpace(_secondSegmentLabel.Text) ? "Second" : _secondSegmentLabel.Text); }
         set { _secondSegmentLabel.Text = value; }
      }
   }
}