using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class LayoutDemo : UserControl
{
    private LayoutDemoViewModel? _viewModel;

    public LayoutDemo()
    {
        InitializeComponent();
        DataContext = _viewModel = new LayoutDemoViewModel();
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e) =>
        _viewModel?.UpdateColumnCount(e.NewSize.Width);

    private void OnExpanderExpanded(object? sender, RoutedEventArgs e) =>
        _viewModel?.OnExpanderExpanded(true);

    private void OnExpanderCollapsed(object? sender, RoutedEventArgs e) =>
        _viewModel?.OnExpanderExpanded(false);
}
