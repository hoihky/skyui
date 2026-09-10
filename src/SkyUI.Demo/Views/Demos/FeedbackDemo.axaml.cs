using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.Demo.Views.Demos;

public partial class FeedbackDemo : UserControl
{
    public FeedbackDemo()
    {
        InitializeComponent();
        SkyMessageBox.Attach(DialogHost);
        DialogHost.PrimaryAction += (_, _) => DialogHost.Close();
    }

    private void OnShowSnackbarClick(object? sender, RoutedEventArgs e) =>
        SnackbarHost.Enqueue("Changes saved", SkyFeedbackVariant.Success);

    private void OnShowDialogClick(object? sender, RoutedEventArgs e) =>
        DialogHost.Show();

    private async void OnMessageBoxInfoClick(object? sender, RoutedEventArgs e)
    {
        await SkyMessageBox.ShowInfoAsync("Your profile was updated successfully.");
        MessageBoxResultText.Text = "Info dismissed.";
    }

    private async void OnMessageBoxWarningClick(object? sender, RoutedEventArgs e)
    {
        await SkyMessageBox.ShowWarningAsync("This action cannot be undone on archived records.");
        MessageBoxResultText.Text = "Warning dismissed.";
    }

    private async void OnMessageBoxErrorClick(object? sender, RoutedEventArgs e)
    {
        await SkyMessageBox.ShowErrorAsync("The server returned an unexpected response.");
        MessageBoxResultText.Text = "Error dismissed.";
    }

    private async void OnMessageBoxConfirmClick(object? sender, RoutedEventArgs e)
    {
        var result = await SkyMessageBox.ConfirmAsync("Delete 3 selected items?");
        MessageBoxResultText.Text = $"Confirm result: {result}";
    }
}
