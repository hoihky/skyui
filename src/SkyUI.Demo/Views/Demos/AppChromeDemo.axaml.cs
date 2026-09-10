using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class AppChromeDemo : UserControl
{
    public AppChromeDemo()
    {
        InitializeComponent();
        DataContext = new AppChromeDemoViewModel();
    }

    private void OnDrawerClosed(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AppChromeDemoViewModel viewModel)
            viewModel.IsDrawerOpen = false;
    }
}
