using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SkyUI.Controls;
using SkyUI.Demo.Mobile.Views;

namespace SkyUI.Demo.Mobile.Views.Demos;

public partial class MobileFeedbackDemo : UserControl
{
    public MobileFeedbackDemo()
    {
        InitializeComponent();
        SnackbarButton.Click += OnSnackbarClick;
        MessageButton.Click += OnMessageClick;
    }

    private void OnSnackbarClick(object? sender, RoutedEventArgs e)
    {
        var shell = this.GetVisualAncestors().OfType<MobileAppView>().FirstOrDefault();
        shell?.SnackbarHostControl.Enqueue("Saved to device.");
    }

    private async void OnMessageClick(object? sender, RoutedEventArgs e)
    {
        await SkyMessageBox.ShowInfoAsync(
            "This dialog uses SkyDialogHost on mobile.",
            title: "SkyUI");
    }
}
