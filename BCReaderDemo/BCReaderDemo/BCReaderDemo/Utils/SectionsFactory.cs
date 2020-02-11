// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BCReaderDemo.Utils
{
   public static class SectionsFactory
   {
      // Load image source to avoid blinking
      private static ImageSource deleteImageSource = ImageSource.FromFile("delete.png");

      public static TableSection CreateSection(Section section)
      {
         TableSection tableSection = new TableSection() { Title = section.Header };

         foreach (ISectionRow row in section.Rows)
         {
            if (row is NameRow)
            {
               tableSection.Add(GetNameRow(row as NameRow));
            }
            else if (row is EditorRow)
            {
               tableSection.Add(GetEntryRow(row as EditorRow));
            }
            else if (row is NotesRow)
            {
               tableSection.Add(GetNotesRow(row as NotesRow));
            }
            else if (row is PickerRow)
            {
               tableSection.Add(GetPickerRow(row as PickerRow));
            }
            else if (row is ButtonRow)
            {
               tableSection.Add(GetButtonRow(row as ButtonRow, tableSection.Count > 0 ? 0 : 10));
            }
            else if(row is DividerRow)
            {
               if(tableSection.Count > 0)
                  tableSection.Add(GetDividerRow(row as DividerRow));
            }
         }

         return tableSection;
      }

      private static Cell GetDividerRow(DividerRow dividerRow)
      {
         Grid grid = new Grid
         {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(0, 0, 0, 0),

            RowDefinitions =
            {
               new RowDefinition
               {
                  Height = new GridLength(dividerRow.HeightRequest > 0 ? dividerRow.HeightRequest : 20, GridUnitType.Absolute),
               },
            },
         };

         if(dividerRow.VisibleDivider)
         {
            grid.Children.Add(new BoxView
            {
               HorizontalOptions = LayoutOptions.FillAndExpand,
               VerticalOptions = LayoutOptions.Center,
               HeightRequest = 0.5,
               BackgroundColor = CustomColors.LightSharkonColor,
            });
         }
         else
         {
            grid.Children.Add(new ContentView
            {
               HorizontalOptions = LayoutOptions.FillAndExpand,
               VerticalOptions = LayoutOptions.FillAndExpand,
               BackgroundColor = CustomColors.PagesBackgroundColor,
            });
         }
         
         return new ViewCell
         {
            View = grid,
         };
      }

      private static Cell GetNameRow(NameRow nameRow)
      {
         Grid grid = new Grid()
         {
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(15, 0, 15, 0),
            RowDefinitions =
            {
               new RowDefinition { Height = 60 },
            },
            ColumnDefinitions =
            {
               new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
               new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
         };

         Label label = new Label
         {
            Text = "NAME",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.LinkBlueColor,
            FontSize = PlatformsConstants.TableViewFieldLabelFontSize,
         };

         CustomEntry entry = new CustomEntry
         {
            Text = nameRow.Text,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Keyboard = Keyboard.Default,
            TextColor = CustomColors.DarkSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldEntryFontSize,
         };

         Image profileImage = new Image
         {
            Aspect = Aspect.AspectFill,
            Source = String.IsNullOrEmpty(nameRow.ImagePath) ? "profile_placeholder.png" : nameRow.ImagePath,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 50,
            HeightRequest = 50,
         };

         profileImage.Effects.Add(Effect.Resolve("BCReaderDemo.RoundCornersEffect"));
         RoundCornersEffect.SetCornerRadius(profileImage, 25);

         profileImage.GestureRecognizers.Add(new TapGestureRecognizer
         {
            NumberOfTapsRequired = 1,
            Command = new Command<object>((obj) => nameRow.AddImageAction?.Invoke(null)),
         });

         StackLayout stackLayout = new StackLayout
         {
            Orientation = StackOrientation.Vertical,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 7,

            Children =
            {
               label,
               entry,
            },
         };

         grid.Children.Add(profileImage, 0, 0);
         grid.Children.Add(stackLayout, 1, 0);

         TapGestureRecognizer labelTapGestureRecognizer = new TapGestureRecognizer()
         {
            Command = new Command((obj) => nameRow.RowTappedAction?.Invoke(null)),
            NumberOfTapsRequired = 1,
         };
         label.GestureRecognizers.Add(labelTapGestureRecognizer);

         entry.Focused += (sender, e) => nameRow.OnFocusAction?.Invoke(e);
         entry.Unfocused += (sender, e) => nameRow.OnUnfocusAction?.Invoke(e);
         entry.TextChanged += (sender, e) => nameRow.TextChangedAction?.Invoke(sender, e);
         
         return new ViewCell()
         {
            View = grid,
         };
      }

      private static Cell GetPickerRow(PickerRow pickerRow)
      {
         Grid grid = new Grid()
         {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(15, 0, 15, 0),
            RowDefinitions =
            {
               new RowDefinition { Height = PlatformsConstants.RowHeight },
            },
         };

         Label label = new Label
         {
            Text = pickerRow.Title.ToUpper(),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.LightSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldLabelFontSize,
         };

         Label pickerLabel = new Label
         {
            Text = pickerRow.Title.ToUpper(),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.LightSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldLabelFontSize,
         };

         Picker picker = new Picker()
         {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.DarkSharkonColor,
         };

         picker.ItemsSource = pickerRow.Items.DisplayItems;
         if (pickerRow.SelectedIndex == -1)
            picker.SelectedIndex = 0;
         else
            picker.SelectedIndex = pickerRow.SelectedIndex;

         picker.SelectedIndexChanged += (sender, e) => pickerRow.SelectedIndexChanged?.Invoke(sender, e);

         CustomEntry newFieldEntry = new CustomEntry()
         {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Placeholder = "New " + pickerRow.Title,
            TextColor = CustomColors.DarkSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldEntryFontSize,
         };

         Image deleteFieldButton = new Image
         {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Source = deleteImageSource,
            Aspect = Aspect.AspectFill,
            WidthRequest = 24,
            HeightRequest = 24,
         };

         StackLayout editStack = new StackLayout
         {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, -5, 0, 0),

            Children =
            {
               newFieldEntry,
               deleteFieldButton,
            },
         };

         StackLayout pickerStack = new StackLayout
         {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Spacing = 7,
            IsVisible = false,

            Children =
            {
               pickerLabel,
               editStack,
            },
         };

         StackLayout stackLayout = new StackLayout
         {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Spacing = 7,

            Children =
            {
               label,
               picker,
            },
         };

         grid.Children.Add(stackLayout, 0, 0);
         grid.Children.Add(pickerStack, 0, 0);

         picker.SelectedIndexChanged += (sender, e) =>
         {
            if (picker.SelectedIndex != -1 && picker.SelectedIndex == picker.Items.Count - 1)
            {
               Device.BeginInvokeOnMainThread(() =>
               {
                  pickerStack.IsVisible = true;
                  stackLayout.IsVisible = false;

                  newFieldEntry.Focus();

                  Action<object, EventArgs> CompletedAction = ((obj, eventArgs) =>
                  {
                     if (!String.IsNullOrEmpty(newFieldEntry.Text))
                     {
                        pickerRow.Items.AddItem(newFieldEntry.Text);
                        picker.ItemsSource = pickerRow.Items.DisplayItems;
                        picker.SelectedIndex = picker.Items.Count - 2;
                     }
                     else
                     {
                        if (pickerRow.SelectedIndex == -1)
                           picker.SelectedIndex = 0;
                        else
                           picker.SelectedIndex = pickerRow.SelectedIndex;
                     }

                     stackLayout.IsVisible = true;
                     pickerStack.IsVisible = false;

                     picker.Focus();
                  });

                  newFieldEntry.Completed += (obj, eventArgs) => CompletedAction(obj, eventArgs);

                  deleteFieldButton.GestureRecognizers.Add(new TapGestureRecognizer()
                  {
                     Command = new Command(() =>
                     {
                        if (pickerRow.SelectedIndex == -1)
                           picker.SelectedIndex = 0;
                        else
                           picker.SelectedIndex = pickerRow.SelectedIndex;

                        newFieldEntry.Unfocus();

                        stackLayout.IsVisible = true;
                        pickerStack.IsVisible = false;

                        picker.Focus();
                     })
                  });
               });
            }
         };

         ViewCell viewCell = new ViewCell()
         {
            View = grid
         };

         return viewCell;
      }

      private static Cell GetNotesRow(NotesRow notesRow)
      {
         Label label = new Label
         {
            Text = notesRow.Title.ToUpper(),
            HorizontalOptions = LayoutOptions.StartAndExpand,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.LightSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldLabelFontSize,
            Margin = new Thickness(0, 5, 0, 0),
         };

         Editor editor = new Editor()
         {
            Text = notesRow.Text,
            Keyboard = Keyboard.Default,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Start,
            HeightRequest = 75,
            Margin = new Thickness(0, -5, 0, 0),
            FontSize = PlatformsConstants.TableViewFieldEntryFontSize,
         };

         StackLayout stackLayout = new StackLayout
         {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(15, 0, 15, 0),
            Spacing = 7,

            Children =
            {
               label,
               editor,
            },
         };

         ViewCell viewCell = new ViewCell
         {
            View = stackLayout,
         };

         editor.TextChanged += (sender, e) => notesRow.TextChangedAction?.Invoke(sender, e);
         editor.Focused += (sender, e) => notesRow.OnFocusAction?.Invoke(e);
         editor.Unfocused += (sender, e) => notesRow.OnUnfocusAction?.Invoke(e);

         return viewCell;
      }

      private static Cell GetButtonRow(ButtonRow buttonRow, double topMargin)
      {
         ViewCell cell = new ViewCell();

         Button rowButton = new Button
         {
            HorizontalOptions = LayoutOptions.StartAndExpand,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Color.Transparent,
            BorderColor = CustomColors.LinkBlueColor,
            BorderWidth = 1,
            CornerRadius = 6,
#if __IOS_
            HeightRequest = 30,
#else
            HeightRequest = 32,
#endif // #if __IOS_
            WidthRequest = 150,
            Text = buttonRow.Text,
            FontSize = PlatformsConstants.TableViewFieldEntryFontSize,
            TextColor = CustomColors.LinkBlueColor,
         };

         rowButton.Clicked += (sender, e) => buttonRow.RowTappedAction?.Invoke(e);

         Grid grid = new Grid
         {
            VerticalOptions = LayoutOptions.CenterAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(15, topMargin, 0, 10),

            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            }
         };

         grid.Children.Add(rowButton, 0, 0);

         cell.View = grid;
         
         return cell;
      }

      private static Cell GetEntryRow(EditorRow editorRow)
      {
         Grid grid = new Grid()
         {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Margin = new Thickness(15, 0, 15, 0),
            RowDefinitions =
            {
               new RowDefinition { Height = PlatformsConstants.RowHeight },
            },
         };

         Label label = new Label
         {
            Text = editorRow.Title.ToUpper(),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CustomColors.LinkBlueColor,
            FontSize = PlatformsConstants.TableViewFieldLabelFontSize,
         };

         CustomEntry entry = new CustomEntry
         {
            Text = editorRow.Text,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Keyboard = editorRow.Keyboard,
            TextColor = CustomColors.DarkSharkonColor,
            FontSize = PlatformsConstants.TableViewFieldEntryFontSize,
         };

         if (label.Text.Contains("EMAIL"))
         {
            entry.ValidationRegex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);
         }

         StackLayout stackLayout = new StackLayout
         {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            Spacing = 7,

            Children =
            {
               label,
               entry,
            },
         };

         grid.Children.Add(stackLayout, 0, 0);

         ViewCell viewCell = new ViewCell
         {
            View = grid
         };

         TapGestureRecognizer labelTapGestureRecognizer = new TapGestureRecognizer()
         {
            Command = new Command((obj) => editorRow.RowTappedAction?.Invoke(null)),
            NumberOfTapsRequired = 1,
         };
         label.GestureRecognizers.Add(labelTapGestureRecognizer);

         entry.Focused += (sender, e) => editorRow.OnFocusAction?.Invoke(e);
         entry.Unfocused += (sender, e) => editorRow.OnUnfocusAction?.Invoke(e);
         entry.TextChanged += (sender, e) => editorRow.TextChangedAction?.Invoke(sender, e);
         
         return viewCell;
      }
   }
}
