using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class ListDemo : UserControl
{
    private ListDemoViewModel? _viewModel;

    public ListDemo()
    {
        InitializeComponent();
        DataContext = _viewModel = new ListDemoViewModel();
    }

    private void OnSelectionChanged(object? sender, CheckedListBoxSelectionChangedEventArgs e) =>
        _viewModel?.OnSelectionChanged(e.Item, e.IsSelected);

    private void OnCheckedChanged(object? sender, CheckedListBoxCheckedChangedEventArgs e) =>
        _viewModel?.OnCheckedChanged(e.Item, e.NewValue);
}
