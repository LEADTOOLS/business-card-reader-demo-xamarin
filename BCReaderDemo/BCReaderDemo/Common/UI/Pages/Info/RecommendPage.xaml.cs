// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Leadtools.Demos.UI.Elements;
using Leadtools.Demos.UI.Page;
using Leadtools.Demos.Utils;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Pages.Info
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class RecommendPage : CustomPage
   {
      public RecommendPage()
      {
         InitializeComponent();
         AfterInitializeComponent();
         InitOptions(new RecommendOptionGrid[]
         {
            new RecommendOptionGrid("FACEBOOK", Facebook_Tapped, Color.FromRgb(64, 93, 154), "Icons/fb-ico.svg"),
            new RecommendOptionGrid("TWITTER", Twitter_Tapped, Color.FromRgb(41, 169, 225), "Icons/twitter-ico.svg"),
            new RecommendOptionGrid("SMS", SMS_Tapped, Color.FromRgb(84, 234, 223), "Icons/sms-ico.svg"),
            new RecommendOptionGrid("EMAIL", Email_Tapped, Color.FromRgb(255, 109, 104), "Icons/email-ico.svg"),
         });
      }

      #region Methods

      #region Events

      private async void Option_Tapped(object sender, EventArgs e)
      {
         if (sender is RecommendOptionGrid option)
            await option.Callback?.Invoke();
      }

      #endregion

      #region Helpers

      private async Task Facebook_Tapped() => await Browser.OpenAsync($"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString($"{DemoUtilities.AppShareLink}?{DemoUtilities.QueryString("facebook", false)}")}");
      private async Task Twitter_Tapped() => await Browser.OpenAsync($"https://www.twitter.com/intent/tweet?text={Uri.EscapeDataString($"Hey! I've been using the #free @LEADTOOLS {DemoUtilities.AppShareName} for {DemoUtilities.AppShareDescription}. I highly recommend you get it here:")}&hashtags={Uri.EscapeDataString(string.Join(",", DemoUtilities.AppShareHashtags))}&url={Uri.EscapeDataString($"{DemoUtilities.AppShareLink}?{DemoUtilities.QueryString("twitter", false)}")}");
      private async Task SMS_Tapped()
      {
         try
         {
            string command = string.Format("sms:{0}body={1}", (Device.RuntimePlatform == Device.iOS) ? "&" : "?", $"Hey! I've been using the free LEADTOOLS {DemoUtilities.AppShareName} for {DemoUtilities.AppShareDescription}. I highly recommend you get it here: {DemoUtilities.AppShareLink}");
            await Launcher.OpenAsync(new Uri(new Uri(command).AbsoluteUri));
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to compose sms: {ex.Message}", "OK");
         }
      }
      private async Task Email_Tapped()
      {
         try
         {
            string subject = string.Format("{0}{1}", !DemoUtilities.AppShareName.StartsWith("LEADTOOLS", StringComparison.OrdinalIgnoreCase) ? "LEADTOOLS " : string.Empty, DemoUtilities.AppShareName);
            string body = $"I've been using the LEADTOOLS {DemoUtilities.AppShareName} and I highly recommend it for {DemoUtilities.AppShareDescription}. You can check it out here: {DemoUtilities.AppShareLink}?{DemoUtilities.QueryString("email", false)}";
            string command = string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, subject, body);
            await Launcher.OpenAsync(new Uri(new Uri(command).AbsoluteUri));
         }
         catch (Exception ex)
         {
            await DisplayAlert("Error", $"Unable to compose email: {ex.Message}", "OK");
         }
      }

      private void InitOptions(RecommendOptionGrid[] options)
      {
         TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Tapped += Option_Tapped;

         int col = 0;
         foreach (RecommendOptionGrid option in options)
         {
            // Add the column definitions
            if (col != 0)
               OptionsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });
            OptionsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Auto) });

            // Configure/add the option
            option.GestureRecognizers.Add(tapGestureRecognizer);
            OptionsGrid.Children.Add(option, col, 0);
            col += 2;
         }
      }

      protected override void OnSizeAllocated(double width, double height)
      {
         base.OnSizeAllocated(width, height);

         // Reduce the recommend QR image size in landscape mode
         if(ContainerScrollViewer != null)
         {
            double rowsSpacing = Utils.GlobalMarginExtension.UnitSize * 0.75 * 2;
            double availableScreenHeight = ContainerScrollViewer.Height - rowsSpacing;
            if (this.Width > this.Height && ContainerScrollViewer.Height != -1 && QRCode.HeightRequest > availableScreenHeight)
               QRCode.HeightRequest = availableScreenHeight;
         }
      }

      #endregion

      #endregion

      #region RecommendOptionGrid

      private class RecommendOptionGrid : Grid
      {
         public RecommendOptionGrid(string labelText, Func<Task> callback, Color textColor, string imageResourceName)
         {
            LabelText = labelText;
            Callback = callback;
            TextColor = textColor;
            ImageResourceName = imageResourceName;

            InitializeComponent();
         }

         #region Internal properties

         public string ImageResourceName { get; }
         public string LabelText { get; }
         public Color TextColor { get; }

         #endregion

         #region Public properties

         public Func<Task> Callback { get; }

         #endregion

         #region Methods

         #region Helpers

         private void InitializeComponent()
         {
            // Setup the properties
            Margin = new Thickness(0);
            Padding = new Thickness(0);
            ColumnSpacing = 0;
            RowSpacing = 0;
            ColumnDefinitions = new ColumnDefinitionCollection()
            {
               new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Auto) }
            };
            RowDefinitions = new RowDefinitionCollection()
            {
               new RowDefinition() { Height = new GridLength(2.0, GridUnitType.Star) },
               new RowDefinition() { Height = new GridLength(1.25, GridUnitType.Star) }
            };

            // Add the inner components
            Children.Add(new SvgImage()
            {
               IsEnabled = false, // Pass-through touch events
               HorizontalOptions = LayoutOptions.Center,
               VerticalOptions = LayoutOptions.Start,
               WidthRequest = GlobalMarginExtension.UnitSize,
               HeightRequest = GlobalMarginExtension.UnitSize,
               ResourceName = ImageResourceName
            }, 0, 1, 0, 2);
            Children.Add(new Label()
            {
               VerticalOptions = LayoutOptions.End,
               HorizontalTextAlignment = TextAlignment.Center,
               VerticalTextAlignment = TextAlignment.Center,
               FontFamily = DemoUtilities.FontFamily,
               FontSize = FontSizeExtension.MicroFontSize,
               Text = LabelText,
               TextColor = TextColor
            }, 0, 2);
         }

         #endregion

         #endregion
      }

      #endregion
   }
}