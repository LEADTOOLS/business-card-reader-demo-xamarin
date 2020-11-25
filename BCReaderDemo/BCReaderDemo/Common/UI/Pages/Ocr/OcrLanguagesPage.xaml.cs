// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.Document.Utils;
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.UI.Page;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.Document.UI.Pages.Ocr
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OcrLanguagesPage : CustomPage
   {
      private const double _cellVerticalMargin = 10;
      private double _defaultCellHeight = ((Device.RuntimePlatform == Device.iOS) ? 16 : (Device.RuntimePlatform == Device.Android) ? 24 : 25) + _cellVerticalMargin;

      public ObservableCollection<OcrLanguage> ActiveLanguages { get; private set; }
      public ObservableCollection<OcrLanguage> InactiveLanguages { get; private set; } = new ObservableCollection<OcrLanguage>();
      public ObservableCollection<OcrLanguage> DownloadableLanguages { get; private set; } = new ObservableCollection<OcrLanguage>();

      public OcrLanguagesPage(ObservableCollection<OcrLanguage> activeLanguages)
      {
         BindingContext = this;

         if(activeLanguages == null)
            ActiveLanguages = new ObservableCollection<OcrLanguage>();
         else
            ActiveLanguages = new ObservableCollection<OcrLanguage>(activeLanguages);

         if (OcrLanguageManager.DownloadableLanguages == null)
         {
            string embeddedLanguagesManifestFilePath = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(rn => rn.EndsWith("embedded_ocr_languages_manifest.json"));
            if (!string.IsNullOrWhiteSpace(embeddedLanguagesManifestFilePath))
            {
               OcrLanguageManager.UpdateDownloadableLanguagesInfo(embeddedLanguagesManifestFilePath, null);
            }
         }

         DownloadableLanguages = OcrLanguageManager.DownloadableLanguages;

         InitializeComponent();
         AfterInitializeComponent();

         TitleExtraButtonTapped += EditButton_TitleExtraButtonTapped;
         PageClosing += OcrLanguagesPage_PageClosing;

         ReloadCachedLanguagesInfo();
      }

      private void OcrLanguagesPage_PageClosing(object sender, EventArgs e)
      {
         IsInEditMode = false;
      }

      private void ReloadCachedLanguagesInfo()
      {
         foreach (OcrLanguage lang in DownloadableLanguages)
         {
            lang.PropertyChanged += DownloadableLanguage_PropertyChanged;
         }

         PopulateActiveInactiveLanguages();

         PopulateDownloadableLanguages();
      }

      private void PopulateActiveInactiveLanguages()
      {
         InactiveLanguages.Clear();

         foreach (OcrLanguage localLanguage in OcrLanguageManager.LocalLanguages)
         {
            localLanguage.IsDefaultLanguage = false;
            if (!ActiveLanguages.Any(l => l.Language.Equals(localLanguage.Language)))
               InactiveLanguages.Add(localLanguage);
         }

         foreach (OcrLanguage activeLanguage in ActiveLanguages)
         {
            activeLanguage.IsDefaultLanguage = ActiveLanguages[0].Equals(activeLanguage);
         }

         var inactiveLanguages = InactiveLanguages.OrderBy(firstOrder => firstOrder.CanBeDeleted).ThenBy(secondOrder => secondOrder.LocalizedName);
         InactiveLanguages = new ObservableCollection<OcrLanguage>(inactiveLanguages);

         RefreshActiveInactiveListViews();
      }

      private void RefreshActiveInactiveListViews()
      {
         ActiveLanguagesListView.ItemsSource = null;
         InactiveLanguagesListView.ItemsSource = null;

         ActiveLanguagesListView.ItemsSource = ActiveLanguages;
         InactiveLanguagesListView.ItemsSource = InactiveLanguages;

         ActiveLanguagesListView.IsDragDropEnabled = IsInEditMode;
         ActiveLanguagesListView.CanDragItems = ActiveLanguages.Count > 1;
         InactiveLanguagesListView.IsDragDropEnabled = IsInEditMode;

         ActiveLanguagesListView.HeightRequest = (Math.Max(1, ActiveLanguages.Count) * _defaultCellHeight + 5);
         InactiveLanguagesListView.HeightRequest = (Math.Max(1, InactiveLanguages.Count) * _defaultCellHeight + 5);
      }

      private void ListView_DropCompleted(object sender, object e)
      {
         if ((sender as CustomListView).Equals(ActiveLanguagesListView))
         {
            foreach (OcrLanguage ocrLanguage in ActiveLanguages)
            {
               ocrLanguage.IsDefaultLanguage = ActiveLanguages[0].Equals(ocrLanguage);
            }
         }
         else if ((sender as CustomListView).Equals(InactiveLanguagesListView))
         {
            foreach (OcrLanguage ocrLanguage in InactiveLanguages)
            {
               ocrLanguage.IsDefaultLanguage = false;
            }
         }

         RefreshActiveInactiveListViews();
      }

      private void PopulateDownloadableLanguages()
      {
         DownloadableLanguagesListView.ItemsSource = null;
         var downloadableLanguages = DownloadableLanguages.OrderBy(l => l.LocalizedName);
         DownloadableLanguages = new ObservableCollection<OcrLanguage>(downloadableLanguages);
         DownloadableLanguagesListView.ItemsSource = DownloadableLanguages;
         DownloadableLanguagesListView.HeightRequest = Math.Max(1, DownloadableLanguages.Count) * _defaultCellHeight + 5;
      }

      private async void DownloadableLanguage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         OcrLanguage ocrLanguage = sender as OcrLanguage;
         if (e.PropertyName.Equals(nameof(ocrLanguage.IsDownloading)))
         {
            await Device.InvokeOnMainThreadAsync(() => PopulateDownloadableLanguages());
         }
      }

      internal bool IsInEditMode { get; set; } = false;
      private void EditButton_TitleExtraButtonTapped(object sender, EventArgs e)
      {
         IsInEditMode = !IsInEditMode;

         (sender as Label).Text = (IsInEditMode) ? "Done" : "Edit";

         RefreshActiveInactiveListViews();
      }

      private void Switch_Tapped(object sender, EventArgs e)
      {
         if (sender is Grid grid && grid.Children[1] is CustomSwitch customSwitch)
         {
            customSwitch.IsToggled = !customSwitch.IsToggled;
         }
      }

      private void DeleteLanguageButton_Tapped(object sender, EventArgs e)
      {
         OcrLanguage ocrLanguage = (sender as SvgImage).BindingContext as OcrLanguage;
         if (ocrLanguage == null) return;

         OcrLanguageManager.DeleteLanguage(ocrLanguage.Language);

         ocrLanguage.IsDownloadable = true;

         // Manually remove from active/inactive languages, then manually add to downloadable languages
         if (ActiveLanguages.Contains(ocrLanguage))
            ActiveLanguages.Remove(ocrLanguage);
         else if (InactiveLanguages.Contains(ocrLanguage))
            InactiveLanguages.Remove(ocrLanguage);

         if (!DownloadableLanguages.Contains(ocrLanguage))
         {
            // Take the language from OcrLanguageManager.DownloadableLanguages instead of the local downloadable languages list since that one 
            // has the updated class members like file size and what not.
            var langs = OcrLanguageManager.DownloadableLanguages.Where(l => l.Language.Equals(ocrLanguage.Language));
            if (langs != null && langs.Count() > 0)
               ocrLanguage = langs.ElementAt(0);

            DownloadableLanguages.Add(ocrLanguage);
         }

         ReloadCachedLanguagesInfo();
      }

      private void DownloadLanguageButton_Tapped(object sender, EventArgs e)
      {
         Label downloadButton = sender as Label;
         downloadButton.IsEnabled = false;
         OcrLanguage ocrLanguage = (sender as Label).BindingContext as OcrLanguage;
         if (ocrLanguage == null) return;

         ocrLanguage.Tag = sender;
         OcrLanguageManager.DownloadLanguage(ocrLanguage.Language, null, new OcrLanguageManager.CompletionDelegate(DownloadLanguageCompleted));
      }

      private void CancelLanguageDownloadButton_Tapped(object sender, EventArgs e)
      {
         OcrLanguage ocrLanguage = (sender as Label).BindingContext as OcrLanguage;
         if (ocrLanguage == null) return;

         OcrLanguageManager.CancelDownloadForLanguage(ocrLanguage.Language);
      }

      private async void DownloadLanguageCompleted(OcrLanguage language, Exception error)
      {
         if (language != null && language.Tag != null)
            await Device.InvokeOnMainThreadAsync(() => (language.Tag as Label).IsEnabled = true);

         if (error == null)
         {
            // Download completed successfully, then remove the downloaded language from the downloadable languages list and add it to the inactive languages list.
            await Device.InvokeOnMainThreadAsync(async() =>
            {
               if (language != null && !language.CancellationTokenSource.IsCancellationRequested)
               {
                  DownloadableLanguages = OcrLanguageManager.DownloadableLanguages;

                  var langs = InactiveLanguages.Where(l => l.Language.Equals(language.Language));
                  if (langs == null || langs.Count() == 0)
                     InactiveLanguages.Add(language);

                  language.IsDownloadable = false;

                  await Device.InvokeOnMainThreadAsync(() => ReloadCachedLanguagesInfo());
               }
            });
         }
         else
         {
            // Error occurred while downloading this language
            await Device.InvokeOnMainThreadAsync(async() =>
            {
               ReloadCachedLanguagesInfo();
               await DisplayAlert("Error Downloading File", $"Encountered an error while trying to download the {language.LocalizedName} language: {error.Message}", "OK");
            });
         }
      }
   }

   public class ListViewDataTemplateSelector : DataTemplateSelector
   {
      public DataTemplate DefaultItemTemplate { get; set; }
      public DataTemplate EditLanguagesItemTemplate { get; set; }
      public DataTemplate DefaultDownloadTemplate { get; set; }
      public DataTemplate DownloadingTemplate { get; set; }

      protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
      {
         DataTemplate retTemplate = DefaultItemTemplate;

         OcrLanguage langItem = item as OcrLanguage;
         if (langItem != null)
         {
            CustomListView listView = (container as CustomListView);
            OcrLanguagesPage parentPage = listView.GetParentPage() as OcrLanguagesPage;

            if (langItem.IsDownloadable)
            {
               retTemplate = langItem.IsDownloading ? DownloadingTemplate : DefaultDownloadTemplate;
            }
            else if (parentPage.IsInEditMode)
            {
               // If active languages list view and it contains only one language then don't show the edit template
               retTemplate = (!string.IsNullOrWhiteSpace(listView.StyleId) && listView.StyleId.Equals("ActiveLanguages") && listView.ItemsSource != null && (listView.ItemsSource as ObservableCollection<OcrLanguage>).Count == 1) ? DefaultItemTemplate : EditLanguagesItemTemplate;
            }
         }

         return retTemplate;
      }
   }

   public static class ViewExtensions
   {
      public static Xamarin.Forms.Page GetParentPage(this VisualElement element)
      {
         if (element != null)
         {
            var parent = element.Parent;
            while (parent != null)
            {
               if (parent is Xamarin.Forms.Page)
               {
                  return parent as Xamarin.Forms.Page;
               }
               parent = parent.Parent;
            }
         }
         return null;
      }
   }
}