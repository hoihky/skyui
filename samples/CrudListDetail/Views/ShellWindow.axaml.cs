using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.DataGrid;
using SkyUI.Samples.CrudListDetail.ViewModels;

namespace SkyUI.Samples.CrudListDetail.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
        AddHandler(LoadedEvent, OnLoaded);
    }

    public SkySnackbarHost SnackbarHost => SnackbarHostControl;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (CustomerGrid.Columns.Count > 0)
            return;

        CustomerGrid.Columns.Add(new SkyDataGridColumn
        {
            Header = "Company",
            BindingPath = nameof(Models.CustomerRow.Company),
            Width = 180,
        });
        CustomerGrid.Columns.Add(new SkyDataGridColumn
        {
            Header = "Contact",
            BindingPath = nameof(Models.CustomerRow.ContactName),
            Width = 140,
        });
        CustomerGrid.Columns.Add(new SkyDataGridColumn
        {
            Header = "Email",
            BindingPath = nameof(Models.CustomerRow.Email),
            Width = 200,
        });
        CustomerGrid.Columns.Add(new SkyDataGridColumn
        {
            Header = "Status",
            BindingPath = nameof(Models.CustomerRow.Status),
            Width = 100,
        });
    }
}
