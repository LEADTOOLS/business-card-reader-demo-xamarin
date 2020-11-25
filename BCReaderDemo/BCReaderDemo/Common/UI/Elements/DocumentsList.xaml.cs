// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.Document.UI.Elements
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class DocumentItem : Grid
   {
      public new DocumentsList Parent { get; private set; }
      public DocumentItemData ItemData { get; private set; }

      public ICommand ItemLongPressedCommand { get; private set; }
      public ICommand ItemTappedCommand { get; private set; }

      public DocumentItem(DocumentsList parent, DocumentItemData itemData)
      {
         Parent = parent;
         ItemData = itemData;

         Parent.PropertyChanged += Parent_PropertyChanged;

         ItemLongPressedCommand = new Command(execute: (e) =>
         {
            DocumentItemData currentItemData = e as DocumentItemData;
            Parent.OnItemLongPressed(currentItemData);
         });

         ItemTappedCommand = new Command(execute: (e) =>
         {
            DocumentItemData currentItemData = e as DocumentItemData;

            // Change item selection state/color
            if (currentItemData != null && Parent.SelectionMode != SelectionMode.None)
            {
               if (Parent.SelectionMode == SelectionMode.Single)
               {
                  ObservableCollection<DocumentItemData> items = Parent.ItemsSource;
                  var selectedItems = items.Where(item => item.IsSelected);
                  if (selectedItems != null && selectedItems.Count() > 0)
                  {
                     // Unselect all other items
                     foreach (DocumentItemData item in selectedItems)
                     {
                        if (item != currentItemData)
                        {
                           item.IsSelected = false;
                        }
                     }
                  }
               }

               currentItemData.IsSelected = !currentItemData.IsSelected;
            }

            // Fire ItemTapped event
            Parent.OnItemTapped(currentItemData);
         });

         // Initialize
         InitializeComponent();

         MultiGesturesEffect.SetLongPressedCommand(MainItemGrid, (Parent.IsLongPressEnabled) ? ItemLongPressedCommand : null);
      }

      private void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         if(e.PropertyName.Equals(DocumentsList.IsLongPressEnabledProperty.PropertyName))
            MultiGesturesEffect.SetLongPressedCommand(MainItemGrid, (Parent.IsLongPressEnabled) ? ItemLongPressedCommand : null);
      }

      private void DocumentQuickActionsButton_Tapped(object sender, EventArgs e)
      {
         // Fire ItemQuickActionsButtonTapped event
         Parent.OnItemQuickActionsButtonTapped(ItemData);
      }
   }


   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public class DocumentsList : ContentView
   {
      private ObservableCollection<DocumentItem> _items = null;
      private ScrollView _scrollView = null;
      private FlexLayout _flexLayout = null;

      public SelectionMode SelectionMode { get; set; } = SelectionMode.None;
      public bool IsNoneSelectionMode { get => SelectionMode == SelectionMode.None; }

      private ObservableCollection<DocumentItemData> _itemsSource = null;
      public ObservableCollection<DocumentItemData> ItemsSource
      {
         get
         {
            return _itemsSource;
         }
         set
         {
            if (_itemsSource == value) return;

            _itemsSource = value;
            OnPropertyChanged("ItemsSource");
         }
      }

      public object EmptyView { get; set; }

      public static readonly BindableProperty IsLongPressEnabledProperty = BindableProperty.Create(
         propertyName: nameof(IsLongPressEnabled),
         returnType: typeof(bool),
         declaringType: typeof(CustomListView),
         defaultValue: false,
         defaultBindingMode: BindingMode.TwoWay,
         propertyChanged: null
      );
      public bool IsLongPressEnabled { get => GetValue(IsLongPressEnabledProperty) is bool value ? value : false; set => SetValue(IsLongPressEnabledProperty, value); }

      public event EventHandler<DocumentItemData> ItemQuickActionsButtonTapped;
      internal void OnItemQuickActionsButtonTapped(DocumentItemData currentItemData)
      {
         ItemQuickActionsButtonTapped?.Invoke(Parent, currentItemData);
      }

      public event EventHandler<LongPressedEventArgs> ItemLongPressed;
      internal void OnItemLongPressed(DocumentItemData currentItemData)
      {
         if(IsLongPressEnabled)
            ItemLongPressed?.Invoke(Parent, new LongPressedEventArgs(currentItemData, -1, -1));
      }

      public event EventHandler<DocumentItemData> ItemTapped;
      internal void OnItemTapped(DocumentItemData currentItemData)
      {
         ItemTapped?.Invoke(Parent, currentItemData);
      }

      public DocumentsList()
      {
         SelectionMode = SelectionMode.None;
         _items = new ObservableCollection<DocumentItem>();

         _flexLayout = new FlexLayout();
         _flexLayout.Wrap = FlexWrap.Wrap;
         _flexLayout.AlignItems = FlexAlignItems.Start;
         _flexLayout.AlignContent = FlexAlignContent.Start;
         _flexLayout.JustifyContent = FlexJustify.Start;

         _scrollView = new ScrollView();
         _scrollView.Content = _flexLayout;

         Content = ((ItemsSource == null || ItemsSource.Count == 0) && EmptyView != null) ? (EmptyView as View) : _scrollView;
      }

      protected override void OnSizeAllocated(double width, double height)
      {
         base.OnSizeAllocated(width, height);

         Content = ((ItemsSource == null || ItemsSource.Count == 0) && EmptyView != null) ? (EmptyView as View) : _scrollView;
      }

      public async void ScrollTo(object item)
      {
         if (item == null || !(item is DocumentItemData)) return;

         DocumentItemData itemData = item as DocumentItemData;
         var items = _items.Where(x => x.ItemData.DocumentId == itemData.DocumentId);
         if (items != null && items.Count() > 0)
         {
            DocumentItem docItem = items.ElementAt(0);
            await Task.Delay(10); // Give a chance to Xamarin to update UI
            if (!docItem.Bounds.IsEmpty)
               await _scrollView.ScrollToAsync(docItem, ScrollToPosition.MakeVisible, false);
            else
            {
               var lastItem = _flexLayout.Children.LastOrDefault();
               if (lastItem != null)
                  await _scrollView.ScrollToAsync(lastItem, ScrollToPosition.MakeVisible, false);
            }
         }
      }

      protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         base.OnPropertyChanged(propertyName);

         if (propertyName.Equals("ItemsSource"))
         {
            // Reset items border color
            if (ItemsSource != null)
            {
               _items.Clear();
               _flexLayout.Children.Clear();

               ObservableCollection <DocumentItemData> items = ItemsSource;
               foreach (var item in items)
               {
                  item.PropertyChanged += Item_PropertyChanged;
                  item.ItemBorderColor = (SelectionMode == SelectionMode.None) ? Color.Transparent : (item.IsSelected) ? SelectedItemBackgroundColor : UnselectedItemBackgroundColor;

                  DocumentItem documentItem = new DocumentItem(this, item);
                  documentItem.WidthRequest = ItemWidth;
                  documentItem.HeightRequest = ItemHeight;

                  _items.Add(documentItem);
                  _flexLayout.Children.Add(documentItem);
               }

               Content = (ItemsSource.Count == 0 && EmptyView != null) ? (EmptyView as View) : _scrollView;
            }
            else
            {
               _items.Clear();
               Content = ((ItemsSource == null || ItemsSource.Count == 0) && EmptyView != null) ? (EmptyView as View) : _scrollView;
            }
         }
      }

      private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         DocumentItemData item = sender as DocumentItemData;
         if (e.PropertyName.Equals("IsSelected"))
         {
            item.ItemBorderColor = (SelectionMode == SelectionMode.None) ? Color.Transparent : (item.IsSelected) ? SelectedItemBackgroundColor : UnselectedItemBackgroundColor;
         }
      }

      // SelectedItemBackgroundColor binadable property
      private static Color _defaultSelectedItemColor = Color.FromRgb(80, 131, 242);
      public static readonly BindableProperty SelectedItemBackgroundColorProperty = BindableProperty.Create(
         propertyName: nameof(SelectedItemBackgroundColor),
         returnType: typeof(Color),
         declaringType: typeof(DocumentsList),
         defaultValue: _defaultSelectedItemColor,
         defaultBindingMode: BindingMode.TwoWay
      );
      public Color SelectedItemBackgroundColor
      {
         get => GetValue(SelectedItemBackgroundColorProperty) is Color value ? value : _defaultSelectedItemColor;
         set => SetValue(SelectedItemBackgroundColorProperty, value);
      }

      // UnselectedItemBackgroundColor binadable property
      private static Color _defaultUnselectedItemColor = Color.FromRgb(114, 132, 158);
      public static readonly BindableProperty UnselectedItemBackgroundColorProperty = BindableProperty.Create(
         propertyName: nameof(UnselectedItemBackgroundColor),
         returnType: typeof(Color),
         declaringType: typeof(DocumentsList),
         defaultValue: _defaultUnselectedItemColor,
         defaultBindingMode: BindingMode.TwoWay
      );
      public Color UnselectedItemBackgroundColor
      {
         get => GetValue(UnselectedItemBackgroundColorProperty) is Color value ? value : _defaultUnselectedItemColor;
         set => SetValue(UnselectedItemBackgroundColorProperty, value);
      }

      // ShowDocumentQuickActionsButton binadable property
      public static readonly BindableProperty ShowDocumentQuickActionsButtonProperty = BindableProperty.Create(
         propertyName: nameof(ShowDocumentQuickActionsButton),
         returnType: typeof(bool),
         declaringType: typeof(DocumentsList),
         defaultValue: true,
         defaultBindingMode: BindingMode.TwoWay
      );
      public bool ShowDocumentQuickActionsButton
      {
         get => GetValue(ShowDocumentQuickActionsButtonProperty) is bool value ? value : true;
         set => SetValue(ShowDocumentQuickActionsButtonProperty, value);
      }

      // ItemWidth binadable property
      private static double _Default_ItemWidth = (Device.Idiom == TargetIdiom.Phone) ? (DemoUtilities.DisplayWidth / 2) - (3 * GlobalMarginExtension.UnitSize * 0.25) : (Device.Idiom == TargetIdiom.Tablet) ? (DemoUtilities.DisplayWidth / 4) - (5 * GlobalMarginExtension.UnitSize * 0.25) : 200;
      public static readonly BindableProperty ItemWidthProperty = BindableProperty.Create(
         propertyName: nameof(ItemWidth),
         returnType: typeof(double),
         declaringType: typeof(DocumentsList),
         defaultValue: _Default_ItemWidth,
         defaultBindingMode: BindingMode.TwoWay
      );
      public double ItemWidth
      {
         get => GetValue(ItemWidthProperty) is double value ? value : _Default_ItemWidth;
         set => SetValue(ItemWidthProperty, value);
      }

      // ItemHeight binadable property
      private static double _Default_ItemHeight = _Default_ItemWidth * 1.2;
      public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create(
         propertyName: nameof(ItemHeight),
         returnType: typeof(double),
         declaringType: typeof(DocumentsList),
         defaultValue: _Default_ItemHeight,
         defaultBindingMode: BindingMode.TwoWay
      );
      public double ItemHeight
      {
         get => GetValue(ItemHeightProperty) is double value ? value : _Default_ItemHeight;
         set => SetValue(ItemHeightProperty, value);
      }
   }
}