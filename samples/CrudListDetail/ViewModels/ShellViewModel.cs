using System.Windows.Input;
using SkyUI.Samples.CrudListDetail.Data;
using SkyUI.Samples.CrudListDetail.Services;
using SkyUI.Samples.Infrastructure.Mvvm;

namespace SkyUI.Samples.CrudListDetail.ViewModels;

/// <summary>Shell coordinator (facade) for list-detail CRUD workflow.</summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly CustomerGridDataSource gridDataSource;
    private readonly CustomerDetailViewModel detailViewModel;
    private string searchQuery = string.Empty;
    private long? selectedRowIndex;
    private string statusMessage = "Select a customer to edit.";

    public ShellViewModel(
        CustomerGridDataSource gridDataSource,
        CustomerDetailViewModel detailViewModel)
    {
        this.gridDataSource = gridDataSource;
        this.detailViewModel = detailViewModel;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        detailViewModel.CustomerChanged += async (_, _) => await RefreshAsync().ConfigureAwait(true);
        detailViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CustomerDetailViewModel.IsNew)
                || args.PropertyName == nameof(CustomerDetailViewModel.HasSelection))
            {
                RaisePropertyChanged(nameof(DetailTitle));
            }
        };
    }

    public CustomerGridDataSource GridDataSource => gridDataSource;

    public CustomerDetailViewModel Detail => detailViewModel;

    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (!SetProperty(ref searchQuery, value))
                return;

            gridDataSource.SearchQuery = value;
            StatusMessage = $"Showing {gridDataSource.RowCount:N0} customers";
        }
    }

    public long? SelectedRowIndex
    {
        get => selectedRowIndex;
        set
        {
            if (!SetProperty(ref selectedRowIndex, value))
                return;

            OnSelectionChanged();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public string DetailTitle => detailViewModel.IsNew
        ? "New customer"
        : detailViewModel.HasSelection
            ? "Customer details"
            : "No selection";

    public ICommand RefreshCommand { get; }

    public async void Initialize()
    {
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task RefreshAsync()
    {
        await gridDataSource.RefreshAsync().ConfigureAwait(true);
        StatusMessage = $"Showing {gridDataSource.RowCount:N0} customers";
        await OnSelectionChangedAsync().ConfigureAwait(true);
    }

    private void OnSelectionChanged() =>
        OnSelectionChangedAsync().GetAwaiter().GetResult();

    private async Task OnSelectionChangedAsync()
    {
        if (!selectedRowIndex.HasValue)
        {
            detailViewModel.Clear();
            StatusMessage = "Select a customer to edit.";
            return;
        }

        var row = gridDataSource.GetRowModel(selectedRowIndex.Value);
        if (row is null)
        {
            detailViewModel.Clear();
            return;
        }

        await detailViewModel.LoadCustomerAsync(row.Id).ConfigureAwait(true);
        StatusMessage = $"Editing {row.Company}";
    }
}
