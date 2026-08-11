using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.Demo.Views.Demos;

public partial class FeedbackDemo : UserControl
{
    public FeedbackDemo()
    {
        InitializeComponent();
        DialogHost.PrimaryAction += (_, _) => DialogHost.Close();
    }

    private void OnShowSnackbarClick(object? sender, RoutedEventArgs e) =>
        SnackbarHost.Enqueue("Changes saved", SkyFeedbackVariant.Success);

    private void OnShowDialogClick(object? sender, RoutedEventArgs e) =>
        DialogHost.Show();
}
