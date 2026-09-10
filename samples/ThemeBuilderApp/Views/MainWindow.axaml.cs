using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.Samples.ThemeBuilderApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowDialogButton.Click += OnShowDialogClick;
        ShowSheetButton.Click += OnShowSheetClick;
        ShowSnackbarButton.Click += OnShowSnackbarClick;
        PreviewDialog.PrimaryAction += OnDialogPrimaryAction;
    }

    private void OnShowDialogClick(object? sender, RoutedEventArgs e) => PreviewDialog.Show();

    private void OnShowSheetClick(object? sender, RoutedEventArgs e) => PreviewSheet.Show();

    private void OnShowSnackbarClick(object? sender, RoutedEventArgs e) =>
        PreviewSnackbar.Enqueue("Theme applied successfully.", SkyFeedbackVariant.Success);

    private void OnDialogPrimaryAction(object? sender, RoutedEventArgs e) => PreviewDialog.Close();
}
