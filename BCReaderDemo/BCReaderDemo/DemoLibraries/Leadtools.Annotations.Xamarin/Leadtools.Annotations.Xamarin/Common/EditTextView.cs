using System.Threading.Tasks;
using Xamarin.Forms;

namespace Leadtools.Annotations.Xamarin
{
    public class EditTextView
    {
      public static Task<string> ShowPasswordDialog(ContentView parent, string title)
      {
         return ShowDialog(parent, title, string.Empty, true);
      }

      public static Task<string> ShowDialog(ContentView parent, string title, string text)
      {
         return ShowDialog(parent, title, text, false);
      }

      private static Task<string> ShowDialog(ContentView parent, string title, string text, bool isPassword)
      {
         var taskCompletionSource = new TaskCompletionSource<string>();

         var titleLabel = new Label { Text = title, HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
         InputView textEntry = null;

         if(isPassword)
            textEntry = new Entry { IsPassword = true, Text = text };
         else
            textEntry = new Editor { Text = text, HeightRequest = 150 };

         var OKButton = new Button { Text = "OK" };
         OKButton.Clicked += (sender, args) =>
         {
            string result = null;
            if (textEntry is Entry)
               result = ((Entry)textEntry).Text;
            else
               result = ((Editor)textEntry).Text.Replace("\r", "\n");

            parent.Content = null;
            (parent.Parent as Frame).IsVisible = false;

            taskCompletionSource.SetResult(result);
         };

         var cancelButton = new Button { Text = "Cancel" };
         cancelButton.Clicked += (sender, args) =>
         {
            parent.Content = null;
            (parent.Parent as Frame).IsVisible = false;

            taskCompletionSource.SetResult(isPassword ? null : text);
         };

         var stackLayout = new StackLayout
         {
            Orientation = StackOrientation.Horizontal,
            Children = { OKButton, cancelButton },
         };

         var layout = new StackLayout
         {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = isPassword ? LayoutOptions.Center : LayoutOptions.FillAndExpand,
            Orientation = StackOrientation.Vertical,
            Margin = new Thickness(20, 20),
            Padding = new Thickness(20, 20),
            Children = { titleLabel, textEntry, stackLayout },
         };

         layout.BackgroundColor = Color.DarkGray;

         parent.Content = layout;
         (parent.Parent as Frame).IsVisible = true;
         textEntry.Focus();

         return taskCompletionSource.Task;
      }
   }
}
