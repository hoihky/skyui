using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class AppTemplatesDemo : UserControl
{
    public AppTemplatesDemo()
    {
        InitializeComponent();
        DataContext = new AppTemplatesDemoViewModel();

        ListDetailPage.Loaded += OnListDetailPageLoaded;
    }

    private void OnListDetailPageLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not SkyListDetailPage page)
            return;

        page.Loaded -= OnListDetailPageLoaded;
        page.AddHandler(SkyDrawer.ClosedEvent, OnDrawerClosed, RoutingStrategies.Bubble);
    }

    private void OnDrawerClosed(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AppTemplatesDemoViewModel viewModel)
            viewModel.IsDrawerOpen = false;
    }
}
