using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class DataPresentationDemo : UserControl
{
    public DataPresentationDemo()
    {
        InitializeComponent();
        DataContext = new DataPresentationDemoViewModel();

        GridPager.Target = GridList;
    }

    private void OnPagerPageChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DataPresentationDemoViewModel viewModel)
            viewModel.RegisterPageChange();
    }
}
