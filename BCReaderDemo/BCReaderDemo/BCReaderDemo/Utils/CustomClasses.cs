// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{
   public class CheckedChangedEventArgs : EventArgs
   {
      public bool IsChecked { get; set; }

      public CheckedChangedEventArgs(bool isChecked)
      {
         IsChecked = isChecked;
      }
   }

   public class CheckboxImage : Image
   {
      public static ImageSource CheckedSource = "checked.png";
      public static ImageSource UnCheckedSource = "unchecked.png";

      public static readonly BindableProperty CheckedProperty =
         BindableProperty.CreateAttached(
             "Checked",
             typeof(bool),
             typeof(CheckboxImage),
             false);

      public delegate void CheckedChangedEventHandler(object sender, CheckedChangedEventArgs e);
      public event CheckedChangedEventHandler CheckedChanged;

      public bool Checked
      {
         get { return (bool)GetValue(CheckedProperty); }
         set
         {
            if (value)
            {
               Device.BeginInvokeOnMainThread(() => this.Source = CheckedSource);
            }
            else
            {
               Device.BeginInvokeOnMainThread(() => this.Source = UnCheckedSource);
            }

            CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));

            SetValue(CheckedProperty, value);
         }
      }

      public CheckboxImage()
      {
         TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Command = new Command(() => this.Checked = !this.Checked);

         this.GestureRecognizers.Add(tapGestureRecognizer);

         Checked = false;
      }
   }

   public class CustomTableView : TableView
   {
      public CustomTableView()
      {

      }
   }

   public class GradientContentView : ContentView
   {

      public GradientContentView()
      {

      }
   }

   public class CustomEntry : Entry
   {
      public static readonly BindableProperty ShowCautionImageProperty =
          BindableProperty.CreateAttached(
              "ShowCautionImage",
              typeof(bool),
              typeof(CustomEntry),
              false,
              BindingMode.TwoWay);

      public Regex ValidationRegex
      {
         get;
         set;
      }

      public Func<string, string> Formatter
      {
         get;
         set;
      }

      public bool ShowCautionImage
      {
         get
         {
            return (bool)GetValue(ShowCautionImageProperty);
         }
         set
         {
            SetValue(ShowCautionImageProperty, value);
         }
      }

      public CustomEntry()
      {
         this.TextChanged += CustomEntry_TextChanged;
      }

      private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
      {
         ValidateText(e.NewTextValue);

         if (Formatter != null)
         {
            this.Text = Formatter.Invoke(this.Text);
         }
      }

      public void ValidateText(string text)
      {
         if (ValidationRegex != null)
         {
            bool match = ValidationRegex.IsMatch(text);
            ShowCautionImage = !match;
         }
      }
   }

   public class ContactSavedEventArgs : EventArgs
   {
      public ContactModel Contact { get; set; }
   }

   public class SettingsFields : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      private string _fieldName = null;
      public string FieldName
      {
         get
         {
            return _fieldName;
         }
         set
         {
            _fieldName = value;
            NotifyPropertyChanged();
         }
      }

      private bool _fieldValue = false;
      public bool FieldValue
      {
         get
         {
            return _fieldValue;
         }
         set
         {
            _fieldValue = value;
            NotifyPropertyChanged();
         }
      }

      private bool _fieldEnable = false;
      public bool FieldEnabled
      {
         get
         {
            return _fieldEnable;
         }
         set
         {
            _fieldEnable = value;
            NotifyPropertyChanged();
         }
      }

      private Type _associatedPageType = null;
      public Type AssociatedPageType
      {
         get
         {
            return _associatedPageType;
         }
         set
         {
            _associatedPageType = value;
         }
      }

      private string _stringValue = string.Empty;
      public string StringValue
      {
         get
         {
            return _stringValue;
         }
         set
         {
            _stringValue = value;
            NotifyPropertyChanged();
         }
      }

      public SettingsFields()
      {
      }

      public SettingsFields(string fieldName, bool fieldValue, bool fieldEnabled, Type associatedPageType, string stringValue)
      {
         FieldName = fieldName;
         FieldValue = fieldValue;
         FieldEnabled = fieldEnabled;
         AssociatedPageType = associatedPageType;
         StringValue = stringValue;
      }
   }

   public class FollowUsFields
   {
      private string _fieldName = null;
      public string FieldName
      {
         get
         {
            return _fieldName;
         }
         set
         {
            _fieldName = value;
         }
      }

      private Color _fieldTextColor = Color.Black;
      public Color FieldTextColor
      {
         get
         {
            return _fieldTextColor;
         }
         set
         {
            _fieldTextColor = value;
         }
      }

      private string _fieldLink = string.Empty;
      public string FieldLink
      {
         get
         {
            return _fieldLink;
         }
         set
         {
            _fieldLink = value;
         }
      }

      private string _fieldIcon = string.Empty;
      public string FieldIcon
      {
         get
         {
            return _fieldIcon;
         }
         set
         {
            _fieldIcon = value;
         }
      }

      public FollowUsFields()
      {
      }

      public FollowUsFields(string fieldName, Color fieldTextColor, string fieldLink)
      {
         FieldName = fieldName;
         FieldTextColor = fieldTextColor;
         FieldLink = fieldLink;
      }
   }

   /// <summary>
   /// Custom ListView class to handle ListView item's long press event.
   /// </summary>
   public class ListViewWithLongPressGesture : ListView
   {
      public ListViewWithLongPressGesture()
      {
         ItemLongPressedAction = new Action<object>((e) =>
         {
            ContactSavedEventArgs args = new ContactSavedEventArgs()
            {
               Contact = e as ContactModel
            };
            ItemLongPressed?.Invoke(this, args);
         });
      }

      public event EventHandler<ContactSavedEventArgs> ItemLongPressed;

      public Action<object> ItemLongPressedAction { get; set; }

      public event EventHandler Clicked;
      public void OnClicked()
      {
         Clicked?.Invoke(this, null);
      }
   }

   /// <summary>
   /// Custom SearchBar class to handle SearchBar touch events.
   /// </summary>
   public class CustomSearchBar : SearchBar
   {
      public CustomSearchBar()
      {
      }

      public event EventHandler Clicked;
      public void OnClicked()
      {
         Clicked?.Invoke(this, null);
      }
   }

   public class AdsLayout : StackLayout
   {
      public string AdText
      {
         get
         {
            Label label = Children[0] as Label;
            if (label == null) return string.Empty;

            return label.FormattedText.ToString();
         }
         set
         {
            Label label = Children[0] as Label;
            label.FormattedText.Spans.Clear();
            string[] components = value.Split("LEADTOOLS");
            for(int i = 0; i < components.Length; i++)
            {
               if (string.IsNullOrWhiteSpace(components[i]))
               {
                  // This is the "LEADTOOLS" component
                  label.FormattedText.Spans.Add(new Span() { Text = "LEADTOOLS", ForegroundColor = CustomColors.AdLEADTOOLSColor, FontSize = label.FontSize });
               }

               label.FormattedText.Spans.Add(new Span() { Text = components[i], ForegroundColor = CustomColors.AdTextColor, FontSize = label.FontSize });
            }
         }
      }

      public AdsLayout()
      {
         Spacing = 0;
         Orientation = StackOrientation.Horizontal;
         HorizontalOptions = LayoutOptions.Center;
         VerticalOptions = LayoutOptions.End;

         const int maximumCharactersCount = 50;
         const double screenRatio = 1.75;
         const double maxFontSize = 15.0;
         double fontSize = Math.Min((App.DisplayScreenWidth / maximumCharactersCount) * screenRatio, maxFontSize);

         FormattedString fs = new FormattedString();
         Label adTextLabel = new Label()
         {
            FontSize = fontSize,
            TextColor = CustomColors.AdTextColor,
            LineBreakMode = LineBreakMode.HeadTruncation,
            FormattedText = fs
         };

         Children.Add(adTextLabel);
      }
   }
}
