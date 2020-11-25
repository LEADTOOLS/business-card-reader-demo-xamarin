// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Leadtools.Demos.UI.Pages.Info
{
   [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class FeedbackPage : PopupPage
   {
      #region Config

      private static int UsesBeforePrompt { get; set; } = 10;
      private static TimeSpan DurationBeforePrompt { get; set; } = TimeSpan.FromDays(3);
      private static string FeedbackEndpoint { get; set; } = "https://www.leadtools.com/adtk/app_rating_handler.ashx";

      private const string PromptText_NotReally = "Not really";
      private const string PromptText_Yes = "Yes!";
      private const string PromptText_AskMeLater = "Ask me later";

      private static string Setting_CanceledFeedback { get; } = $"{nameof(FeedbackPage)} > {nameof(CanceledFeedback)}";
      private static string Setting_LastUse { get; } = $"{nameof(FeedbackPage)} > {nameof(LastUse)}";
      private static string Setting_NumUses { get; } = $"{nameof(FeedbackPage)} > {nameof(NumUses)}";
      private static string Setting_SuppressPrompt { get; } = $"{nameof(FeedbackPage)} > {nameof(SuppressPrompt)}";

      #endregion

      public FeedbackPage()
      {
         InitializeComponent();
         if (Device.RuntimePlatform == Device.iOS)
            HasSystemPadding = false;
      }

      #region Internal properties

      private static HttpClient HttpClient { get; } = new HttpClient();
      private static bool CanceledFeedback
      {
         get => Preferences.Get(Setting_CanceledFeedback, false);
         set => Preferences.Set(Setting_CanceledFeedback, value);
      }
      private static DateTime LastUse
      {
         get => Preferences.Get(Setting_LastUse, DateTime.MaxValue);
         set => Preferences.Set(Setting_LastUse, value);
      }
      private static int NumUses
      {
         get => Preferences.Get(Setting_NumUses, 0);
         set => Preferences.Set(Setting_NumUses, value);
      }
      private static bool SuppressPrompt
      {
         get => Preferences.Get(Setting_SuppressPrompt, false);
         set => Preferences.Set(Setting_SuppressPrompt, value);
      }

      #endregion

      #region Methods

      #region OnXXX

      protected override bool OnBackButtonPressed() => CanClose(base.OnBackButtonPressed());
      protected override bool OnBackgroundClicked() => CanClose(base.OnBackgroundClicked());

      #endregion

      #region Events

      private async void Cancel_Tapped(object sender, EventArgs e)
      {
         if (CanClose(true))
            await PopupNavigation.Instance.PopAsync();
      }

      private async void Send_Tapped(object sender, EventArgs e)
      {
         // Check for valid feedback endpoint URL string
         if (string.IsNullOrWhiteSpace(FeedbackEndpoint))
         {
            await DisplayAlert("Error", "Invalid feedback endpoint URL", "OK");
            return;
         }

         // Valid email?
         if (!string.IsNullOrWhiteSpace(Email.Text) && !IsValidEmail(Email.Text))
         {
            await DisplayAlert("Error", "The email provided was invalid", "OK");
            return;
         }

         // Valid feedback?
         if (string.IsNullOrWhiteSpace(Feedback.Text))
         {
            await DisplayAlert("Error", "Feedback is empty", "OK");
            return;
         }

         // Send web request
         SendFeedback(new FeedbackData(false, Email.Text, Feedback.Text));

         // Disable
         SuppressPrompt = true;

         // Hide dialog
         await PopupNavigation.Instance.PopAsync();
      }

      #endregion

      #region Helpers

      public static async Task AppWasUsed()
      {
         // Don't show prompt?
         if (SuppressPrompt)
            return;

         DateTime lastUse = LastUse;
         LastUse = DateTime.Now;
         if (++NumUses >= UsesBeforePrompt || LastUse - lastUse >= DurationBeforePrompt)
         {
            // Reset the number of uses
            NumUses = 0;

            // Show prompt
            await PromptForFeedback();
         }
      }

      private bool CanClose(bool @default)
      {
         if (!string.IsNullOrEmpty(Email.Text) || !string.IsNullOrEmpty(Feedback.Text))
         {
            PromptForClose();
            return false;
         }
         if (@default)
            // Dialog was cancelled and is going to close
            CheckCancelledTwice();
         return @default;
      }

      private void CheckCancelledTwice()
      {
         // Supress after second cancellation
         if (CanceledFeedback)
         {
            // Don't show prompt again
            SuppressPrompt = true;

            // Send web request
            SendFeedback(new FeedbackData(false, null, null));
         }
         else
            CanceledFeedback = true;
      }

      // Source: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
      private static bool IsValidEmail(string email)
      {
         if (string.IsNullOrWhiteSpace(email))
            return false;

         try
         {
            // Normalize the domain
            email = System.Text.RegularExpressions.Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
               // Use IdnMapping class to convert Unicode domain names.
               var idn = new IdnMapping();

               // Pull out and process domain name (throws ArgumentException on invalid)
               var domainName = idn.GetAscii(match.Groups[2].Value);

               return match.Groups[1].Value + domainName;
            }
         }
         catch (RegexMatchTimeoutException)
         {
            return false;
         }
         catch (ArgumentException)
         {
            return false;
         }

         try
         {
            return System.Text.RegularExpressions.Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
         }
         catch (RegexMatchTimeoutException)
         {
            return false;
         }
      }

      public static async Task<bool> OpenStoreReviewPage()
      {
         bool ret = false;
         try
         {
            Device.BeginInvokeOnMainThread(async () => await Browser.OpenAsync(DemoUtilities.AppShareLink));
            ret = true;
         }
         catch(Exception ex)
         {
            await Device.InvokeOnMainThreadAsync(async () => await DemoUtilities.MainPage.DisplayAlert("Error", $"Unable to display Feedback page: {ex.Message}", "OK"));
         }

         return ret;
      }

      private async void PromptForClose()
      {
         if (await DisplayAlert("Warning", "You have unsubmitted feedback", "Discard", "Cancel"))
         {
            CheckCancelledTwice();
            await PopupNavigation.Instance.PopAsync();
         }
      }

      private static async Task PromptForFeedback()
      {
         if (string.IsNullOrWhiteSpace(FeedbackEndpoint))
            return;

         string response = await Device.InvokeOnMainThreadAsync(async () => await DemoUtilities.MainPage.DisplayActionSheet($"Enjoying the LEADTOOLS {DemoUtilities.AppShareName}?", PromptText_AskMeLater, null, PromptText_Yes, PromptText_NotReally));
         switch (response)
         {
            case PromptText_NotReally:
               await Device.InvokeOnMainThreadAsync(async () => await PopupNavigation.Instance.PushAsync(new FeedbackPage()));
               break;
            case PromptText_Yes:
               // Send web request
               SendFeedback(new FeedbackData(true, null, null));

               // Show review page
               if (await OpenStoreReviewPage())
                  SuppressPrompt = true;
               break;
         }
      }

      public static async Task<bool> RateItNow()
      {
         if (string.IsNullOrWhiteSpace(FeedbackEndpoint))
            return false;

         if (await OpenStoreReviewPage())
         {
            // Don't show dialog again
            SuppressPrompt = true;

            // Send web request
            SendFeedback(new FeedbackData(true));
         }

         return true;
      }

      private static async void SendFeedback(FeedbackData data)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(FeedbackEndpoint))
               return;

            HttpResponseMessage response = await HttpClient.PostAsync(
               FeedbackEndpoint,
               new StringContent(
                  JsonConvert.SerializeObject(data, Formatting.Indented),
                  System.Text.Encoding.UTF8,
                  "text/json"));
            if(!response.IsSuccessStatusCode)
            {
               System.Diagnostics.Debug.WriteLine("SendFeedback failed with code {0}{1}{2}", response.StatusCode.ToString(), Environment.NewLine, await response.Content.ReadAsStringAsync());
            }
         }
         catch
         {
            // Optional: Report exception
         }
      }

      #endregion

      #endregion

      #region FeedbackData

      [Serializable]
      private class FeedbackData
      {
         public FeedbackData(bool enjoyingApp, string email, string feedback)
         {
            RateItNow = false;
            EnjoyingApp = enjoyingApp;
            Email = email;
            Feedback = feedback;
            QaEmail = "";

            AdID = DemoUtilities.AppAdID;
            MetaName = DemoUtilities.AppMetaName;
         }

         public FeedbackData(bool rateItNow) : this(false, null, null)
         {
            RateItNow = rateItNow;
         }

         #region Public properties

         public bool RateItNow { get; set; }
         public bool EnjoyingApp { get; set; }
         public string Email { get; set; }
         public string Feedback { get; set; }
         public string QaEmail { get; set; }
         public bool Echo = true;

         public string AdID { get; set; }
         public string MetaName { get; set; }

         #endregion
      }

      #endregion
   }
}
