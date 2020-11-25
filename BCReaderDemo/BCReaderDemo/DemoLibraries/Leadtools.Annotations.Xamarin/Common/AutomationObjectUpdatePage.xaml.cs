// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Annotations.Automation;
using Leadtools.Annotations.Engine;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Leadtools.Annotations.Xamarin
{
   public class CustomExtension
   {
      public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(object), typeof(CustomExtension), null);

      public static object GetTag(BindableObject bindable)
      {
         if (bindable is Button)
            return ((Button)bindable).CommandParameter;

         return bindable.GetValue(TagProperty);
      }

      public static void SetTag(BindableObject bindable, object value)
      {
         if (bindable is Button)
            ((Button)bindable).CommandParameter = value;
         else
            bindable.SetValue(TagProperty, value);
      }
   }

   public partial class AutomationUpdateObjectPage : PopupPage
   {
      private bool[] pageVisible = new bool[3] {true, true, true};

      private AutomationUpdateObjectDialogPage _initialPage = AutomationUpdateObjectDialogPage.Properties;
      private AnnAutomation _activeAutomation;
      private AnnObject _editObject;

      private bool _isModified;
      public bool IsModified
      {
         get { return _isModified; }
      }

      private AnnObject _targetObject;

      private const int NorbergObjectId = -1008;// Leadtools.Annotations.UserMedicalPack.AnnNorbergObjcet

      //private AutomationUpdateObjectDialogPage _page;
      public bool GetPageVisible(AutomationUpdateObjectDialogPage page)
      {
         return pageVisible[(int)page];
      }

      public void SetPageVisible(AutomationUpdateObjectDialogPage page, bool value)
      {
         pageVisible[(int)page] = value;
      }

      private TreeView _treeView = new TreeView();
      public AutomationUpdateObjectPage(AnnAutomation activeAutomation, AnnObject editObject)
      {
         Init(activeAutomation, editObject);
      }

      public AutomationUpdateObjectPage(AnnAutomation activeAutomation, AnnObject editObject, bool[] visibility)
      {
         if (visibility.Length != pageVisible.Length)
            throw new Exception("Invalid Argument");

         pageVisible = visibility;
         Init(activeAutomation, editObject);
      }

      private async void Init(AnnAutomation activeAutomation, AnnObject editObject)
      {
         try
         {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
               HasSystemPadding = false;

            this._activeAutomation = activeAutomation;
            this._editObject = editObject;

            if (GetPageVisible(AutomationUpdateObjectDialogPage.Reviews))
            {
               _treeView.HeightRequest = 150;
               _treeView.BackgroundColor = Color.DarkGray;
               _treeView.SelectedItemChanged += _treeView_SelectedItemChanged;
               _reviewsTreeContainer.Content = _treeView;
            }

            InitializeAutomationObjectUpdateWindow();

            // If the object type is polyruler, then update the Tickmarks Stroke Thickness value
            AnnPolyRulerObject polyRulerObj = _targetObject as AnnPolyRulerObject;
            if (polyRulerObj != null)
            {
               polyRulerObj.TickMarksStroke.StrokeThickness = LeadLengthD.Create(polyRulerObj.Stroke.StrokeThickness.Value);
            }
         }
         catch(Exception ex)
         {
            await DisplayAlert("Exception", $"Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", "OK");
         }
      }

      private async void BackButton_Tapped(object sender, EventArgs e)
      {
         // Go back
         await PopupNavigation.Instance.PopAsync();
      }

      private void InitializeAutomationObjectUpdateWindow()
      {
         _isModified = false;
         if (_activeAutomation != null && _editObject == null)
         {
            _editObject = _activeAutomation.CurrentEditObject;
         }

         _targetObject = _editObject;

         // check if its content dialog only 
         if (pageVisible[1] == true && pageVisible[0] == false && pageVisible[2] == false) // content dialog 
         {
            _tabsPanel.Children.Remove(_propertiesTabButton);
            _initialPage = AutomationUpdateObjectDialogPage.Content;
         }
         else
         {
            AnnAutomationObject automationObject = _activeAutomation.Manager.FindObjectById(_targetObject.Id);

            if (automationObject != null)
               this.Title = string.Format("{0} {1}", automationObject.Name, "Properties");

            _initialPage = AutomationUpdateObjectDialogPage.Properties;
         }

         // Init the properties page
         if (GetPageVisible(AutomationUpdateObjectDialogPage.Properties) && _activeAutomation != null && _targetObject != null)
         {
            if (_targetObject is AnnSelectionObject)
            {
               AnnSelectionObject selection = _editObject as AnnSelectionObject;

               _targetObject = selection.SelectedObjects[0];
               if (selection.SelectedObjects.Count > 0)
               {
                  foreach (AnnObject annObject in selection.SelectedObjects)
                  {
                     ShowObjectPropertiesTabs(annObject);
                  }

                  OnChangeSelectedTab(_commonTab);
               }
            }
            else
            {
               ShowObjectPropertiesTabs(_targetObject);
               AnnObjectEditor annEditObject = new AnnObjectEditor(_targetObject);
               EnumEditObject(annEditObject.Properties);
               annEditObject.OnPropertyChanged += AnnEditObject_OnPropertyChanged;

               OnChangeSelectedTab(_commonTab);
            }
         }
         else
         {
            _tabsPanel.Children.Remove(_propertiesTabButton);
         }

         var isSelectionObject = _targetObject is AnnSelectionObject;

         // Init the content and review pages
         if (GetPageVisible(AutomationUpdateObjectDialogPage.Content) && !isSelectionObject)
         {
            var content = _targetObject.Metadata.ContainsKey(AnnObject.ContentMetadataKey) ? _targetObject.Metadata[AnnObject.ContentMetadataKey] : null;
            _contentTextBox.Text = content;
         }
         else
         {
            _tabsPanel.Children.Remove(_contentTabButton);
         }

         if (GetPageVisible(AutomationUpdateObjectDialogPage.Reviews) && !isSelectionObject)
            CopyReviewsFrom(_targetObject);
         else
            _tabsPanel.Children.Remove(_reviewsTabButton);

         if (_initialPage == AutomationUpdateObjectDialogPage.Properties && _tabsPanel.Children.Contains(_propertiesTabButton))
            ShowProperties();
         else if (_initialPage == AutomationUpdateObjectDialogPage.Content && _tabsPanel.Children.Contains(_contentTabButton))
            ShowContent();
         else if (_initialPage == AutomationUpdateObjectDialogPage.Reviews && _tabsPanel.Children.Contains(_reviewsTabButton))
            ShowProperties();
      }

      private async void Done_Click(object sender, EventArgs e)
      {
         // If the object type is polyruler, then update the TickMarks Stroke Thickness value
         AnnPolyRulerObject polyRulerObj = _targetObject as AnnPolyRulerObject;
         if (polyRulerObj != null)
         {
            polyRulerObj.TickMarksStroke.StrokeThickness = LeadLengthD.Create(polyRulerObj.Stroke.StrokeThickness.Value);
         }

         if (GetPageVisible(AutomationUpdateObjectDialogPage.Content))
         {
            _targetObject.Metadata[AnnObject.ContentMetadataKey] = _contentTextBox.Text;
         }

         if (GetPageVisible(AutomationUpdateObjectDialogPage.Reviews))
         {
            ReplacesReviewsIn(_targetObject);
         }

         if (_activeAutomation != null)
            _activeAutomation.Invalidate(LeadRectD.Empty);

         // Go back
         await PopupNavigation.Instance.PopAsync();
      }

      private void PropertiesTabButton_Click(object sender, EventArgs e)
      {
         ShowProperties();
      }

      private void ContentTabButton_Click(object sender, EventArgs e)
      {
         ShowContent();
      }

      private void ReviewsTabButton_Click(object sender, EventArgs e)
      {
         ShowReviews();
      }

      private void ShowProperties()
      {
         _propertiesContainerPanel.IsVisible = true;
         _contentContainerPanel.IsVisible = false;
         _reviewsContainerPanel.IsVisible = false;
      }

      private void ShowContent()
      {
         _propertiesContainerPanel.IsVisible = false;
         _contentContainerPanel.IsVisible = true;
         _reviewsContainerPanel.IsVisible = false;
      }

      private void ShowReviews()
      {
         _propertiesContainerPanel.IsVisible = false;
         _contentContainerPanel.IsVisible = false;
         _reviewsContainerPanel.IsVisible = true;
      }

      #region Properties Page Panel

      private void EnumEditObject(Dictionary<String, AnnPropertyInfo> properties)
      {
         foreach (String propertyName in properties.Keys)
         {
            AnnPropertyInfo propetyInfo = properties[propertyName];
            String[] values = null;
            if (propetyInfo != null && propetyInfo.IsVisible)
            {
               if (propetyInfo.EditorType != null && propetyInfo.EditorType.Properties != null)
               {
                  EnumEditObject(propetyInfo.EditorType.Properties);
               }
               else
               {
                  IAnnEditor editorType = propetyInfo.EditorType;
                  View uiEditor = null;
                  if (editorType is AnnColorEditor)
                  {
                     values = new String[] { "transparent", "red", "blue", "green", "yellow", "black", "white" };
                  }
                  else
                  {
                     values = new string[propetyInfo.Values.Keys.Count];
                     propetyInfo.Values.Keys.CopyTo(values, 0);
                     Array.Sort(values);
                  }

                  if (values.Length > 0)
                  {
                     if (editorType is AnnBooleanEditor)
                        uiEditor = CreateToggleButton(propetyInfo, values);
                     else
                        uiEditor = CreateComboBox(propetyInfo, values);
                  }
                  else if (editorType is AnnStringEditor)
                  {
                     uiEditor = CreateEditor(propetyInfo, propetyInfo.Value.ToString());
                  }
                  else if (editorType is AnnMediaEditor)
                  {
                     uiEditor = CreateTextBox(propetyInfo, (propetyInfo.Value as AnnMedia).Source1);
                  }
                  else if (editorType is AnnDoubleEditor)
                  {
                     uiEditor = CreateTextBox(propetyInfo, propetyInfo.Value.ToString());

                     (uiEditor as Entry).TextChanged += TxtBoxDouble_TextChanged;
                  }
                  else if (editorType is AnnIntegerEditor)
                  {
                     uiEditor = CreateTextBox(propetyInfo, propetyInfo.Value.ToString());
                     (uiEditor as Entry).TextChanged += TxtBoxInteger_TextChanged;
                  }
                  else if (editorType is AnnPictureEditor)
                  {
                     uiEditor = CreatePictureView(propetyInfo, propetyInfo.Value);
                  }

                  uiEditor.Margin = new Thickness(0, 0, 0, 10);

                  Label propertyLabel = CreateTextBlock(propetyInfo.DisplayName);
                  propertyLabel.Margin = new Thickness(0, 0, 0, 10);

                  AddProperty(propetyInfo.Category, uiEditor, propertyLabel);
               }
            }
         }
      }

      private void TxtBoxDouble_TextChanged(object sender, TextChangedEventArgs e)
      {
         Entry txtBox = sender as Entry;

         bool isFormated = true;
         double result = 0;
         string text = txtBox.Text;
         int dotIndex = text.IndexOf(".");
         if (dotIndex != -1)
         {
            string integerDigitPart = text.Substring(0, dotIndex);
            if (integerDigitPart.Length > 6)
               isFormated = false;

            string fractionDigitPart = text.Substring(dotIndex + 1);
            if (fractionDigitPart.Length > 2)
               isFormated = false;
         }
         else
         {
            if (text.Length > 6)
               txtBox.Text = text = text.Substring(0, 6);
         }

         bool isValid = isFormated && (text == "." || Double.TryParse(text, out result));
         if (!isValid)
         {
            if (text.Length > 0)
            {
               int cursorIndex = Math.Abs(txtBox.CursorPosition - 1);
               if (txtBox.Text.Length > cursorIndex)
                  txtBox.Text = txtBox.Text.Remove(cursorIndex, 1);
               txtBox.CursorPosition = cursorIndex;
               txtBox.SelectionLength = 0;
            }
         }
         
         AnnPropertyInfo annPropertyInfo = CustomExtension.GetTag(txtBox) as AnnPropertyInfo;
         if (annPropertyInfo.DisplayName == "Opacity")
         {
            double opacity = 1.0;
            if (double.TryParse(txtBox.Text, out opacity))
            {
               if (opacity > 1 || opacity < 0)
                  txtBox.Text = "1";
            }
         }
      }

      private void TxtBoxInteger_TextChanged(object sender, TextChangedEventArgs e)
      {
         Entry txtBox = sender as Entry;
         if (txtBox.Text != null && txtBox.Text.Length > 6)
         {
            txtBox.Text = txtBox.Text.Remove(txtBox.Text.Length - 1);
         }
      }

      private void ToggleButton_Toggled(object sender, ToggledEventArgs e)
      {
         Switch toggleButton = sender as Switch;
         AnnPropertyInfo propetyInfo = (AnnPropertyInfo)CustomExtension.GetTag(toggleButton);
         propetyInfo.Value = toggleButton.IsToggled;
      }

      private Switch CreateToggleButton(AnnPropertyInfo propetyInfo, String[] values)
      {
         Switch toggleButton = new Switch();
         CustomExtension.SetTag(toggleButton, propetyInfo);
         toggleButton.Toggled += ToggleButton_Toggled;
         toggleButton.IsToggled = (bool)propetyInfo.Value;

         return toggleButton;
      }

      private void ComboBox_SelectionChanged(object sender, EventArgs e)
      {
         Picker comboBox = sender as Picker;
         String value = comboBox.SelectedItem.ToString();
         AnnPropertyInfo propetyInfo = (AnnPropertyInfo)CustomExtension.GetTag(comboBox);
         if (propetyInfo.EditorType is AnnDoubleEditor)
            propetyInfo.Value = Double.Parse(value);
         else
            propetyInfo.Value = value;
      }

      private Picker CreateComboBox(AnnPropertyInfo propertyInfo, String[] values)
      {
         Object defaulteValue = propertyInfo.Value;
         Picker comboBox = new Picker();
         foreach (String value in values)
            comboBox.Items.Add(value);

         string defaultValueString = defaulteValue.ToString();
         if (propertyInfo.DisplayName == "Fill" || propertyInfo.DisplayName == "Color" || propertyInfo.DisplayName == "Background")
            defaultValueString = defaultValueString.ToLower();

         comboBox.SelectedItem = defaultValueString;
         if (comboBox.SelectedItem == null)
            comboBox.SelectedIndex = 0;

         comboBox.SelectedIndexChanged += ComboBox_SelectionChanged;
         CustomExtension.SetTag(comboBox, propertyInfo);

         return comboBox;
      }

      private Label CreateTextBlock(String text)
      {
         Label textBlock = new Label();
         textBlock.Text = text;
         textBlock.HorizontalOptions = LayoutOptions.Center;
         textBlock.VerticalOptions = LayoutOptions.Center;

         return textBlock;
      }

      private void Editor_TextChanged(object sender, TextChangedEventArgs e)
      {
         Editor editor = sender as Editor;
         AnnPropertyInfo propetyInfo = (AnnPropertyInfo)CustomExtension.GetTag(editor);
         propetyInfo.Value = editor.Text;
      }

      private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
      {
         Entry textBox = sender as Entry;
         AnnPropertyInfo propetyInfo = (AnnPropertyInfo)CustomExtension.GetTag(textBox);
         if (propetyInfo.EditorType is AnnDoubleEditor)
         {
            double dResult = 0;
            if (Double.TryParse(textBox.Text, out dResult))
               propetyInfo.Value = dResult;
         }
         else if (propetyInfo.EditorType is AnnIntegerEditor)
         {
            int nResult = 0;
            if (Int32.TryParse(textBox.Text, out nResult))
               propetyInfo.Value = nResult;
         }
         else if (propetyInfo.EditorType is AnnMediaEditor)
         {
            AnnMedia media = new AnnMedia();
            media.Source1 = textBox.Text;
            propetyInfo.Value = media;
         }
         else
         {
            propetyInfo.Value = textBox.Text;
         }
      }

      private Editor CreateEditor(AnnPropertyInfo propetyInfo, String text)
      {
         Editor editor = new Editor();
         editor.Text = text == null ? string.Empty : text;
         editor.SizeChanged += (sender, args) =>
         {
            AnnPropertyInfo info = CustomExtension.GetTag(editor) as AnnPropertyInfo;
            editor.Text = info.Value as string;
         };

         CustomExtension.SetTag(editor, propetyInfo);
         editor.TextChanged += Editor_TextChanged;
         return editor;
      }

      private Entry CreateTextBox(AnnPropertyInfo propetyInfo, String text)
      {
         Entry textBox = new Entry();
         textBox.Text = text == null ? string.Empty : text;
         CustomExtension.SetTag(textBox, propetyInfo);
         textBox.TextChanged += TextBox_TextChanged;

         return textBox;
      }

      private async void OpenImageBtn_Click(object sender, EventArgs e)
      {
         Button openImageBtn = sender as Button;
         try
         {
            Stream stream = await DependencyService.Get<Leadtools.Demos.Utils.IPicturePicker>().GetImageStreamAsync();
            if (stream == null)
               return;

            stream.Position = 0;
            // Set Data
            byte[] pngData = new byte[stream.Length];
            stream.Read(pngData, 0, pngData.Length);

            // Update Image
            Image image = (openImageBtn.Parent as Grid).Children[0] as Image;
            image.Source = ImageSource.FromStream(() => new MemoryStream(pngData));
            AnnPicture picture = new AnnPicture(pngData);
            AnnPropertyInfo propetyInfo = (AnnPropertyInfo)CustomExtension.GetTag(openImageBtn);
            propetyInfo.Value = picture;

            // Update Delete button
            Button deleteButton = (openImageBtn.Parent as Grid).Children[2] as Button;
            deleteButton.IsEnabled = image.Source != null;
         }
         catch (Exception ex)
         {
            await DisplayAlert("AnnPicture", ex.Message, "OK");
         }
      }

      private void DeleteImageBtn_Click(object sender, EventArgs e)
      {
         Button deleteImageBtn = sender as Button;
         deleteImageBtn.IsEnabled = false;

         // Update Image
         Image image = (deleteImageBtn.Parent as Grid).Children[0] as Image;
         image.Source = null;

         AnnPropertyInfo propertyInfo = (AnnPropertyInfo)CustomExtension.GetTag(deleteImageBtn);
         propertyInfo.Value = null;
      }

      private Grid CreatePictureView(AnnPropertyInfo propetyInfo, Object value)
      {
         Grid grid = new Grid();
         ColumnDefinition imageColDef = new ColumnDefinition();
         imageColDef.Width = new GridLength(2, GridUnitType.Star);
         ColumnDefinition openImageButtonColDef = new ColumnDefinition();
         openImageButtonColDef.Width = new GridLength(1, GridUnitType.Star);
         ColumnDefinition deleteImageButtonColDef = new ColumnDefinition();
         deleteImageButtonColDef.Width = new GridLength(1, GridUnitType.Star);

         grid.ColumnDefinitions.Add(imageColDef);
         grid.ColumnDefinitions.Add(openImageButtonColDef);
         grid.ColumnDefinitions.Add(deleteImageButtonColDef);

         Image image = new Image();
         image.HeightRequest = 150;
         image.WidthRequest = 150;
         if (value != null && value is AnnPicture)
         {
            AnnPicture picture = (AnnPicture)value;
            if(picture.PictureData != null)
            {
               byte[] data = Convert.FromBase64String(picture.PictureData);
               image.Source = ImageSource.FromStream(() => new MemoryStream(data));
            }
         }

         Button openImageBtn = new Button();
         CustomExtension.SetTag(openImageBtn, propetyInfo);
         openImageBtn.BackgroundColor = Color.DarkGray;
         openImageBtn.TextColor = Color.Black;
         openImageBtn.FontSize = 12;
         openImageBtn.VerticalOptions = LayoutOptions.Center;
         openImageBtn.Text = "...";
         openImageBtn.Clicked += OpenImageBtn_Click;

         Button deleteImageBtn = new Button();
         CustomExtension.SetTag(deleteImageBtn, propetyInfo);
         deleteImageBtn.BackgroundColor = Color.DarkGray;
         deleteImageBtn.TextColor = Color.Black;
         deleteImageBtn.FontSize = 12;
         deleteImageBtn.VerticalOptions = LayoutOptions.Center;
         deleteImageBtn.Text = "X";
         deleteImageBtn.IsEnabled = image.Source != null;
         deleteImageBtn.Clicked += DeleteImageBtn_Click;

         image.SetValue(Grid.ColumnProperty, 0);
         grid.Children.Add(image);
         openImageBtn.SetValue(Grid.ColumnProperty, 1);
         grid.Children.Add(openImageBtn);
         deleteImageBtn.SetValue(Grid.ColumnProperty, 2);
         grid.Children.Add(deleteImageBtn);
         return grid;
      }

      private void ExpandableButton_Click(object sender, EventArgs e)
      {
         Button button = sender as Button;
         Frame content = (Frame)CustomExtension.GetTag(button);
         if (content.IsVisible)
         {
            content.IsVisible = false;
            button.Text = "+ " + content.ClassId;
         }
         else
         {
            content.IsVisible = true;
            button.Text = "- " + content.ClassId;
         }
      }

      private Button CreateExpandableButton(String text, Frame content)
      {
         Button button = new Button();
         button.HorizontalOptions = LayoutOptions.Fill;
         button.Text = "+ " + text;
         CustomExtension.SetTag(button, content);
         button.Clicked += ExpandableButton_Click;

         return button;
      }

      private void AddProperty(String groupName, View editor, View propertyLabel)
      {
         string groupName_noSpaces = groupName.Replace(" ", "");
         StackLayout containerPanel = GetProperteisContatinerTabPanel(groupName_noSpaces);
         Grid grid = null;
         foreach(var child in containerPanel.Children)
         {
            if(child is Frame)
            {
               foreach (var frameChild in ((Frame)child).Children)
               {
                  if (frameChild.ClassId != null && frameChild.ClassId.CompareTo(groupName_noSpaces) == 0)
                     grid = frameChild as Grid;
                  break;
               }
            }
         }

         if (grid == null)
         {
            Frame border = new Frame();
            border.CornerRadius = 10;
            border.IsVisible = false;
            border.VerticalOptions = LayoutOptions.Fill;
            border.ClassId = groupName;

            grid = new Grid();
            grid.ClassId = groupName;
            grid.HorizontalOptions = LayoutOptions.Fill;
            grid.VerticalOptions = LayoutOptions.Start;
            grid.Margin = new Thickness(10, 0, 10, 0);

            border.Content = grid;

            Button category = CreateExpandableButton(groupName, border);
            containerPanel.Children.Add(category);

            ColumnDefinition labelColDef = new ColumnDefinition();
            labelColDef.Width = new GridLength(2, GridUnitType.Star);
            ColumnDefinition editorColDef = new ColumnDefinition();
            editorColDef.Width = new GridLength(3, GridUnitType.Star);

            grid.ColumnDefinitions.Add(labelColDef);
            grid.ColumnDefinitions.Add(editorColDef);

            containerPanel.Children.Add(border);
         }

         grid.RowDefinitions.Add(new RowDefinition());

         propertyLabel.SetValue(Grid.ColumnProperty, 0);
         propertyLabel.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
         editor.SetValue(Grid.ColumnProperty, 1);
         editor.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);

         grid.Children.Add(propertyLabel);
         grid.Children.Add(editor);
      }

      private StackLayout GetProperteisContatinerTabPanel(string groupName)
      {
         switch (groupName)
         {
            case "Font":
            case "Text":
               return _textPropertiesContainer;

            case "Curve":
               return _curvePropertiesContainer;

            case "Ruler":
               return _rulerPropertiesContainer;

            case "Protractor":
               return _protractorPropertiesContainer;

            case "RubberStamp":
               return _rubberStampPropertiesContainer;

            case "TextRollup":
               return _textRollupPropertiesContainer;

            case "TextPointer":
               return _textPointerPropertiesContainer;

            case "Picture":
               return _picturePropertiesContainer;

            case "Point":
               return _pointPropertiesContainer;

            case "Media":
               return _mediaPropertiesContainer;

            case "Encrypt":
               return _encryptPropertiesContainer;

            default:
               return _commonPropertiesContainer;
         }
      }

      private void ShowObjectPropertiesTabs(AnnObject annObject)
      {
         _commonTab.IsVisible = true;

         if (annObject is AnnPolyRulerObject && annObject.Id != NorbergObjectId)
         {
            _rulerTab.IsVisible = true;
         }

         if (annObject is AnnTextObject)
         {
            _textTab.IsVisible = true;
         }

         if (annObject is AnnTextRollupObject)
         {
            _textRollupTab.IsVisible = true;
         }

         if (annObject is AnnCurveObject)
         {
            _curveTab.IsVisible = true;
         }

         if (annObject is AnnRubberStampObject)
         {
            _rubberStampTab.IsVisible = true;
         }

         if (annObject is AnnTextPointerObject)
         {
            _textPointerTab.IsVisible = true;
         }

         if (annObject is AnnStampObject)
         {
            _pictureTab.IsVisible = true;
            CustomExtension.SetTag(_pictureTab, AnnObject.StampObjectId);
         }

         if (annObject is AnnHotspotObject)
         {
            _pictureTab.IsVisible = true;
            CustomExtension.SetTag(_pictureTab, AnnObject.HotspotObjectId);
         }

         if (annObject is AnnFreehandHotspotObject)
         {
            _pictureTab.IsVisible = true;
            CustomExtension.SetTag(_pictureTab, AnnObject.FreehandHotspotObjectId);
         }

         if (annObject is AnnMediaObject)
         {
            _mediaTab.IsVisible = true;
         }

         if (annObject is AnnEncryptObject)
         {
            _encryptTab.IsVisible = true;
         }

         if (annObject is AnnPointObject)
         {
            _pointTab.IsVisible = true;
         }

         if (annObject is AnnProtractorObject)
         {
            _protractorTab.IsVisible = true;
         }
      }

      private void _propertiesTabButtons_Click(object sender, EventArgs e)
      {
         Button tabButton = sender as Button;

         OnChangeSelectedTab(tabButton);
      }

      void OnChangeSelectedTab(Button currentTabPage)
      {
         AnnSelectionObject selectionObject = _activeAutomation.CurrentEditObject as AnnSelectionObject;

         if (selectionObject != null && selectionObject.SelectedObjects.Count > 0)
         {
            int tag = Convert.ToInt32(CustomExtension.GetTag(currentTabPage));

            foreach (AnnObject annObject in selectionObject.SelectedObjects)
            {
               if (annObject.Id == tag || GetAnnObjectBaseID(annObject.Id, currentTabPage) == tag)
               {
                  _targetObject = annObject.Clone();
                  break;
               }
            }

            foreach (StackLayout panel in _propertiesTabPanel.Children)
            {
               panel.Children.Clear();
            }

            AnnObjectEditor annEditObject = new AnnObjectEditor(_targetObject);
            EnumEditObject(annEditObject.Properties);
            annEditObject.OnPropertyChanged += AnnEditObject_OnPropertyChanged;
         }

         ShowCurrentTabPanel(currentTabPage);
      }

      private void AnnEditObject_OnPropertyChanged(string propertyName, object newValue)
      {
         AnnSelectionObject selection = _activeAutomation.CurrentEditObject as AnnSelectionObject;
         if (selection != null)
         {
            foreach (AnnObject annObject in selection.SelectedObjects)
            {
               UpdateObjectProperties(annObject, propertyName, newValue);
            }
         }
         else
         {
            UpdateObjectProperties(_targetObject, propertyName, newValue);
         }

         AnnObjectCollection modifiedObjects = new AnnObjectCollection();
         modifiedObjects.Add(_targetObject);
         _activeAutomation.InvokeObjectModified(modifiedObjects, AnnObjectChangedType.Modified);
      }

      private void ShowCurrentTabPanel(Button currentTabPage)
      {
         foreach (StackLayout panel in _propertiesTabPanel.Children)
         {
            panel.IsVisible = false;
         }

         string tabName = currentTabPage.Text;
         switch (tabName)
         {
            case "Ruler":
               _rulerPropertiesContainer.IsVisible = true;
               break;
            case "Text":
               _textPropertiesContainer.IsVisible = true;
               break;
            case "Text Rollup":
               _textRollupPropertiesContainer.IsVisible = true;
               break;
            case "Curve":
               _curvePropertiesContainer.IsVisible = true;
               break;
            case "RubberStamp":
               _rubberStampPropertiesContainer.IsVisible = true;
               break;
            case "Picture":
               _picturePropertiesContainer.IsVisible = true;
               break;
            case "Media":
               _mediaPropertiesContainer.IsVisible = true;
               break;
            case "Encrypt":
               _encryptPropertiesContainer.IsVisible = true;
               break;
            case "Point":
               _pointPropertiesContainer.IsVisible = true;
               break;
            case "Protractor":
               _protractorPropertiesContainer.IsVisible = true;
               break;
            case "Text Pointer":
               _textPointerPropertiesContainer.IsVisible = true;
               break;
            default:
               _commonPropertiesContainer.IsVisible = true;
               break;
         }
      }

      private void UpdateObjectProperties(AnnObject annObject, string propertyName, object newValue)
      {
         switch (propertyName)
         {
            case "Font":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.Font = newValue as AnnFont != null ? (newValue as AnnFont).Clone() : null;
               }
               break;

            case "TextForeground":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.TextForeground = newValue as AnnBrush != null ? (newValue as AnnBrush).Clone() : null;
               }
               break;

            case "TextBackground":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.TextBackground = newValue as AnnBrush != null ? (newValue as AnnBrush).Clone() : null;
               }
               break;

            case "WordWrap":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  bool wordWrap = annTextObject.WordWrap;
                  bool.TryParse(newValue.ToString(), out wordWrap);
                  annTextObject.WordWrap = wordWrap;
               }
               break;

            case "GaugeLength":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  annPolyRulerObject.GaugeLength = (LeadLengthD)newValue;
               }
               break;

            case "TickMarksLength":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  annPolyRulerObject.TickMarksLength = (LeadLengthD)newValue;
               }
               break;

            case "ImagePicture":
               if (annObject is AnnImageObject)
               {
                  AnnImageObject annImageObject = annObject as AnnImageObject;
                  annImageObject.Picture = newValue as AnnPicture != null ? (newValue as AnnPicture).Clone() : null;
               }
               else if (annObject is AnnFreehandHotspotObject)
               {
                  AnnFreehandHotspotObject annFreehandHotspotObject = annObject as AnnFreehandHotspotObject;
                  annFreehandHotspotObject.Picture = newValue as AnnPicture != null ? (newValue as AnnPicture).Clone() : null;
               }
               else if (annObject is AnnStampObject)
               {
                  AnnStampObject annStampObject = annObject as AnnStampObject;
                  annStampObject.Picture = newValue as AnnPicture != null ? (newValue as AnnPicture).Clone() : null;
               }
               break;

            case "Hyperlink":
               annObject.Hyperlink = newValue.ToString();
               break;

            case "Opacity":
               double opacity = annObject.Opacity;
               double.TryParse(newValue.ToString(), out opacity);
               annObject.Opacity = opacity;
               break;

            case "ShowPicture":
               if (annObject is AnnPointObject)
               {
                  AnnPointObject annPointObject = annObject as AnnPointObject;
                  bool showPicture = annPointObject.ShowPicture;
                  bool.TryParse(newValue.ToString(), out showPicture);
                  annPointObject.ShowPicture = showPicture;
               }
               break;

            case "Expanded":
               if (annObject is AnnTextRollupObject)
               {
                  AnnTextRollupObject annTextRollupObject = annObject as AnnTextRollupObject;
                  bool expanded = annTextRollupObject.Expanded;
                  bool.TryParse(newValue.ToString(), out expanded);
                  annTextRollupObject.Expanded = expanded;
               }
               break;

            case "HiliteColor":
               if (annObject is AnnHiliteObject)
               {
                  AnnHiliteObject annHiliteObject = annObject as AnnHiliteObject;
                  annHiliteObject.HiliteColor = newValue.ToString();
               }
               break;

            case "Fill":
               AnnBrush brush = newValue as AnnBrush != null ? (newValue as AnnBrush).Clone() : null;
               if (annObject.Id == AnnObject.HiliteObjectId)
               {
                  AnnHiliteObject annHiliteObject = annObject as AnnHiliteObject;
                  if (brush != null)
                  {
                     AnnSolidColorBrush solidBrush = brush as AnnSolidColorBrush;
                     if (solidBrush != null)
                     {
                        annHiliteObject.HiliteColor = solidBrush.Color;
                     }
                  }
               }
               else
               {
                  annObject.Fill = brush;
               }

               break;

            case "Stroke":
               annObject.Stroke = newValue as AnnStroke != null ? (newValue as AnnStroke).Clone() : null;
               SetPolyRulerTickMarks(annObject);
               break;

            case "RubberStampType":
               if (annObject is AnnRubberStampObject)
               {
                  AnnRubberStampObject annRubberStampObject = annObject as AnnRubberStampObject;
                  annRubberStampObject.RubberStampType = (AnnRubberStampType)Enum.Parse(typeof(AnnRubberStampType), newValue.ToString());
               }
               break;

            case "Acute":
               if (annObject is AnnProtractorObject)
               {
                  AnnProtractorObject annProtractorObject = annObject as AnnProtractorObject;
                  bool acute = annProtractorObject.Acute;
                  bool.TryParse(newValue.ToString(), out acute);
                  annProtractorObject.Acute = acute;
               }
               break;

            case "FixedPointer":
               if (annObject is AnnTextPointerObject)
               {
                  AnnTextPointerObject annTextPointerObject = annObject as AnnTextPointerObject;
                  bool fixedPointer = annTextPointerObject.FixedPointer;
                  bool.TryParse(newValue.ToString(), out fixedPointer);
                  annTextPointerObject.FixedPointer = fixedPointer;
               }
               break;

            case "AnglePrecision":
               if (annObject is AnnProtractorObject)
               {
                  AnnProtractorObject annProtractorObject = annObject as AnnProtractorObject;
                  int anglePrecision = annProtractorObject.AnglePrecision;
                  int.TryParse(newValue.ToString(), out anglePrecision);
                  annProtractorObject.AnglePrecision = anglePrecision;
               }
               break;

            case "Precision":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  int precision = annPolyRulerObject.Precision;
                  int.TryParse(newValue.ToString(), out precision);
                  annPolyRulerObject.Precision = precision;
               }
               break;

            case "AngularUnit":
               if (annObject is AnnProtractorObject)
               {
                  AnnProtractorObject annProtractorObject = annObject as AnnProtractorObject;
                  annProtractorObject.AngularUnit = (AnnAngularUnit)Enum.Parse(typeof(AnnAngularUnit), newValue.ToString());
               }
               break;

            case "ShowTickMarks":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  bool showTickMarks = annPolyRulerObject.ShowTickMarks;
                  bool.TryParse(newValue.ToString(), out showTickMarks);
                  annPolyRulerObject.ShowTickMarks = showTickMarks;
               }
               break;

            case "MeasurementUnit":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  annPolyRulerObject.MeasurementUnit = (AnnUnit)Enum.Parse(typeof(AnnUnit), newValue.ToString());
               }
               break;

            case "ShowGauge":
               if (annObject is AnnPolyRulerObject)
               {
                  AnnPolyRulerObject annPolyRulerObject = annObject as AnnPolyRulerObject;
                  bool showGauge = annPolyRulerObject.ShowGauge;
                  bool.TryParse(newValue.ToString(), out showGauge);
                  annPolyRulerObject.ShowGauge = showGauge;
               }
               break;

            case "Tension":
               if (annObject is AnnCurveObject)
               {
                  AnnCurveObject annCurveObject = annObject as AnnCurveObject;
                  double tension = annCurveObject.Tension;
                  double.TryParse(newValue.ToString(), out tension);
                  annCurveObject.Tension = tension;
               }
               break;

            case "HorizontalAlignment":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.HorizontalAlignment = (AnnHorizontalAlignment)Enum.Parse(typeof(AnnHorizontalAlignment), newValue.ToString());
               }
               break;

            case "VerticalAlignment":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.VerticalAlignment = (AnnVerticalAlignment)Enum.Parse(typeof(AnnVerticalAlignment), newValue.ToString());
               }
               break;

            case "Text":
               if (annObject is AnnTextObject)
               {
                  AnnTextObject annTextObject = annObject as AnnTextObject;
                  annTextObject.Text = newValue.ToString();
               }
               break;

            case "Media":
               if (annObject is AnnMediaObject)
               {
                  AnnMediaObject annMediaObject = annObject as AnnMediaObject;
                  annMediaObject.Media = newValue as AnnMedia != null ? (newValue as AnnMedia).Clone() : null;
               }
               break;

            case "Key":
               if (annObject is AnnEncryptObject)
               {
                  AnnEncryptObject annEncryptObject = annObject as AnnEncryptObject;
                  int key = annEncryptObject.Key;
                  int.TryParse(newValue.ToString(), out key);
                  annEncryptObject.Key = key;
               }
               break;

            case "Encryptor":
               if (annObject is AnnEncryptObject)
               {
                  AnnEncryptObject annEncryptObject = annObject as AnnEncryptObject;
                  bool encryptor = annEncryptObject.Encryptor;
                  bool.TryParse(newValue.ToString(), out encryptor);
                  annEncryptObject.Encryptor = encryptor;
               }
               break;
         }
      }

      private void SetPolyRulerTickMarks(AnnObject annObject)
      {
         var polyRulerObject = annObject as AnnPolyRulerObject;
         if (polyRulerObject != null)
         {
            polyRulerObject.TickMarksStroke = polyRulerObject.Stroke;
         }
      }

      private int GetAnnObjectBaseID(int currentID, Button currentTab)
      {
         if (currentTab == null)
            return currentID;

         int baseID = currentID;
         switch (currentID)
         {
            case AnnObject.StampObjectId:
               baseID = AnnObject.TextObjectId;
               break;
            case AnnObject.CrossProductObjectId:
               baseID = AnnObject.PolyRulerObjectId;
               break;
            case AnnObject.TextPointerObjectId:
               baseID = AnnObject.TextObjectId;
               break;
            case AnnObject.NoteObjectId:
               baseID = AnnObject.TextObjectId;
               break;
            case AnnObject.ProtractorObjectId:
               baseID = AnnObject.PolyRulerObjectId;
               break;
            case AnnObject.TextRollupObjectId:
               if (currentTab.ToString() == "TabPage: {Note}")
                  baseID = AnnObject.NoteObjectId;
               else
                  baseID = AnnObject.TextObjectId;
               break;
            case AnnObject.AudioObjectId:
            case AnnObject.MediaObjectId:
               baseID = AnnObject.HotspotObjectId;
               break;
            case NorbergObjectId:
               baseID = AnnObject.ProtractorObjectId;
               break;
            default:
               break;
         }

         return baseID;
      }

      #endregion //Properties Page Panel

      #region Content Page Panel

      private void ContentText_TextChanged(object sender, TextChangedEventArgs e)
      {
         _targetObject.Metadata[AnnObject.ContentMetadataKey] = _contentTextBox.Text;
      }

      #endregion //Content Page Panel

      #region Reviews Page Panel

      public void CopyReviewsFrom(AnnObject annObject)
      {
         if (annObject == null) throw new ArgumentNullException("annObject");

         // Add the content (if available)
         UpdateContent(annObject);

         _treeView.Items.Clear();

         foreach (var review in annObject.Reviews)
         {
            AddItem(_treeView, null, true, review);
         }

         if (_treeView.SelectedItem == null && _treeView.Items.Count > 0)
            _treeView.SelectedItem = _treeView.Items[0];

         UpdateUIState();
      }

      private void UpdateContent(AnnObject annObject)
      {
         var metadata = annObject.Metadata;

         string author = null;
         if (metadata.ContainsKey(AnnObject.AuthorMetadataKey))
            author = metadata[AnnObject.AuthorMetadataKey];

         if (string.IsNullOrEmpty(author))
            author = "[author]";

         string lastModified = null;
         if (metadata.ContainsKey(AnnObject.ModifiedMetadataKey))
            lastModified = metadata[AnnObject.ModifiedMetadataKey];

         if (string.IsNullOrEmpty(lastModified))
            lastModified = "[date]";

         _contetnHeader.Text = string.Format("By {0} at {1}", author, lastModified);

         string text = null;

         var textObject = annObject as AnnTextObject;
         if (textObject != null)
         {
            text = textObject.Text;
         }
         else
         {
            // Get it from the content
            if (metadata.ContainsKey(AnnObject.ContentMetadataKey))
               text = metadata[AnnObject.ContentMetadataKey];
         }

         _reviewContentTextBox.Text = text;
      }

      public void ReplacesReviewsIn(AnnObject annObject)
      {
         if (annObject == null) throw new ArgumentNullException("annObject");

         annObject.Reviews.Clear();

         foreach (TreeViewItem item in _treeView.Items)
         {
            GetItem(_treeView, item, annObject, null);
         }
      }

      private static void GetItem(TreeView treeView, TreeViewItem item, AnnObject annObject, AnnReview parentReview)
      {
         var itemReview = item != null ? CustomExtension.GetTag(item) as AnnReview : null;
         var review = itemReview != null ? itemReview.Clone() : null;

         foreach (TreeViewItem childitem in item.Items)
         {
            GetItem(treeView, childitem, annObject, review);
         }

         if (parentReview != null)
            parentReview.Replies.Add(review);
         else
            annObject.Reviews.Add(review);
      }

      private static TreeViewItem AddItem(TreeView treeView, TreeViewItem relativeItem, bool sibling, AnnReview review)
      {
         // Add some text to make it wide. We will custom-draw this anyway
         TreeViewItem item = new TreeViewItem();
         item.Text = GetReviewItemText(review);
         // add a copy so we don't change the original reviews if the user cancels
         CustomExtension.SetTag(item, review != null ? review.Clone() : null);
         review = CustomExtension.GetTag(item) as AnnReview;

         foreach (var reply in review.Replies)
         {
            AddItem(treeView, item, true, reply);
         }

         // Clean its replies, we dont need them here
         review.Replies.Clear();

         if (sibling)
         {
            if (relativeItem != null)
               relativeItem.AddItem(item);
            else
               treeView.AddItem(item);
         }
         else
         {
            if (relativeItem != null)
            {
               ITreeObject obj;

               if (relativeItem.RelativeItem is TreeViewItem)
                  obj = (relativeItem.RelativeItem as TreeViewItem);
               else
                  obj = treeView;

               var index = obj.Items.IndexOf(relativeItem);
               if (index != -1)
                  obj.InsertItem(index + 1, item);
               else
                  obj.AddItem(item);
            }
            else
            {
               treeView.AddItem(item);
            }
         }

         return item;
      }

      private static string GetReviewItemText(AnnReview review)
      {
         var lines = new string[3];

         var author = review != null ? review.Author : null;
         if (string.IsNullOrEmpty(author))
            author = "[author]";

         lines[0] = author;

         if (review != null)
         {
            lines[1] = string.Format("{0} {1}", review.Status, review.Date);
            lines[2] = review.Comment;
         }
         else
         {
            lines[1] = string.Empty;
            lines[2] = string.Empty;
         }

         string text = string.Format("{0} @ {1} @ {2}", lines[0], lines[1], lines[2]);
         text = text.Replace("@", System.Environment.NewLine);

         return text;
      }

      void _treeView_SelectedItemChanged(object sender, EventArgs e)
      {
         UpdateUIState();
         ReviewToDetails();
      }

      private void UpdateUIState()
      {
         var item = _treeView.SelectedItem;
         _replyButton.IsEnabled = item != null;
         _deleteButton.IsEnabled = item != null;
         _authorTextBox.IsEnabled = item != null;

         _dateDayText.IsEnabled = item != null;
         _dateMonthText.IsEnabled = item != null;
         _dateYearText.IsEnabled = item != null;

         _statusPicker.IsEnabled = item != null;
         _checkedCheckBox.IsEnabled = item != null;
         _commentTextBox.IsEnabled = item != null;
      }

      private void ReplyButton_Click(object sender, EventArgs e)
      {
         AddOrReply(_treeView.SelectedItem as TreeViewItem, true);
      }

      private void AddButton_Click(object sender, EventArgs e)
      {
         AddOrReply(_treeView.SelectedItem as TreeViewItem, false);
      }

      private void DeleteButton_Click(object sender, EventArgs e)
      {
         DeleteReview(_treeView.SelectedItem as TreeViewItem);
      }

      private void DeleteReview(TreeViewItem item)
      {
         if (item == null)
            return;

         // Remove it from its parent
         if (item.RelativeItem is TreeViewItem)
         {
            var parentReview = CustomExtension.GetTag((item.RelativeItem as TreeViewItem)) as AnnReview;
            (item.RelativeItem as TreeViewItem).RemoveItem(item);
         }
         else
            _treeView.RemoveItem(item);

         UpdateUIState();
         ReviewToDetails();
      }

      private void AddOrReply(TreeViewItem item, bool isReply)
      {
         _isModified = true;

         // add after selected
         var review = new AnnReview();
         review.Author = "User";
         review.Date = DateTime.Now;
         review.Status = AnnReview.Reply;
         review.Comment = "";

         var newItem = AddItem(_treeView, item, isReply, review);
         _treeView.SelectedItem = newItem;
      }

      private void DetailsTextBox_TextChanged(object sender, TextChangedEventArgs e)
      {
         _isModified = true;
         DetailsToReview();
      }

      private void StatusPicker_SelectionChanged(object sender, EventArgs e)
      {
         DetailsToReview();
      }

      private void CheckedCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         _isModified = true;
         DetailsToReview();
      }

      private void ReviewToDetails()
      {
         TreeViewItem item = _treeView.SelectedItem as TreeViewItem;
         var review = item != null ? CustomExtension.GetTag(item) as AnnReview : null;

         _ignoreChanges = true;

         _authorTextBox.Text = review != null ? review.Author : string.Empty;

         _dateDayText.Text = review != null ? review.Date.Day.ToString() : DateTime.Now.Day.ToString();
         _dateMonthText.Text = review != null ? review.Date.Month.ToString() : DateTime.Now.Month.ToString();
         _dateYearText.Text = review != null ? review.Date.Year.ToString() : DateTime.Now.Year.ToString();

         var reviewText = review != null ? review.Status : string.Empty;
         if (string.IsNullOrWhiteSpace(reviewText))
            _statusPicker.SelectedIndex = 0;
         else
            _statusPicker.SelectedItem = reviewText;
         _checkedCheckBox.IsToggled = review != null ? review.IsChecked : false;
         _commentTextBox.Text = review != null ? review.Comment : string.Empty;

         _ignoreChanges = false;
      }

      private bool _ignoreChanges;

      private void DetailsToReview()
      {
         if (_ignoreChanges)
            return;

         TreeViewItem item = _treeView.SelectedItem as TreeViewItem;
         if (item == null)
            return;

         var review = CustomExtension.GetTag(item) as AnnReview;

         if (review != null)
         {
            if (review.Author != _authorTextBox.Text) review.Author = _authorTextBox.Text;
            DateTime dateTime = GetValidDateTime();
            if (review.Date != dateTime) review.Date = dateTime;
            if (review.Status != _statusPicker.SelectedItem.ToString()) review.Status = _statusPicker.SelectedItem.ToString();
            if (review.IsChecked != _checkedCheckBox.IsToggled) review.IsChecked = _checkedCheckBox.IsToggled;
            if (review.Comment != _commentTextBox.Text) review.Comment = _commentTextBox.Text;
         }

         item.Text = GetReviewItemText(review);
      }

      private bool CheckDateTimeEntry(Entry entry, int maxLength)
      {
         if(!string.IsNullOrEmpty(entry.Text) && (!entry.Text.All(char.IsDigit) || entry.Text.Length > maxLength))
         {
            entry.Text = string.Empty;
            DisplayAlert("Error", "Please Insert Valid Number", "OK");
            return false;
         }

         return true;
      }

      private DateTime GetValidDateTime()
      {
         DateTime dateTime = DateTime.Now;
         if (!CheckDateTimeEntry(_dateDayText, 2) ||
            !CheckDateTimeEntry(_dateMonthText, 2) ||
            !CheckDateTimeEntry(_dateYearText, 4))
         {
            return dateTime.Date;
         }

         int day = 1, month = 1, year = 2019;

         int.TryParse(_dateDayText.Text, out day);
         int.TryParse(_dateMonthText.Text, out month);
         int.TryParse(_dateYearText.Text, out year);

         if (day == 0 || month == 0 || year == 0)
            return dateTime.Date;
         try
         {
            dateTime = new DateTime(year, month, day);
         }
         catch (Exception exception)
         {
            DisplayAlert("Please Insert Valid Number", exception.Message, "OK");
         }

         return dateTime.Date;
      }

      #endregion //Reviews Page Panel
   }

   public enum AutomationUpdateObjectDialogPage
   {
      Properties,
      Content,
      Reviews
   }
}
