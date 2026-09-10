using SkyUI.DataGrid;
using SkyUI.Samples.CrudListDetail.Models;
using SkyUI.Samples.CrudListDetail.Services;

namespace SkyUI.Samples.CrudListDetail.Data;

/// <summary>Adapter between <see cref="ICustomerRepository"/> and <see cref="SkyVirtualDataGrid"/>.</summary>
public sealed class CustomerGridDataSource : IVirtualGridDataSource
{
    private readonly ICustomerRepository repository;
    private IReadOnlyList<CustomerRow> rows = Array.Empty<CustomerRow>();
    private string? searchQuery;

    public CustomerGridDataSource(ICustomerRepository repository)
    {
        this.repository = repository;
    }

    public long RowCount => rows.Count;

    public event EventHandler? StructureChanged;

    public string? SearchQuery
    {
        get => searchQuery;
        set
        {
            searchQuery = value;
            RefreshAsync().GetAwaiter().GetResult();
        }
    }

    public object? GetRow(long index) =>
        index >= 0 && index < rows.Count ? rows[(int)index] : null;

    public CustomerRow? GetRowModel(long index) =>
        index >= 0 && index < rows.Count ? rows[(int)index] : null;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var customers = await repository.ListAsync(searchQuery, cancellationToken).ConfigureAwait(false);
        rows = customers.Select(CustomerRow.FromCustomer).ToList();
        StructureChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ApplySort(SkyDataGridColumn? column, SkyDataGridSortDirection direction)
    {
        if (direction == SkyDataGridSortDirection.None || string.IsNullOrEmpty(column?.BindingPath))
        {
            RefreshAsync().GetAwaiter().GetResult();
            return;
        }

        var path = column.BindingPath;
        rows = direction == SkyDataGridSortDirection.Ascending
            ? rows.OrderBy(row => GetSortValue(row, path), StringComparer.OrdinalIgnoreCase).ToList()
            : rows.OrderByDescending(row => GetSortValue(row, path), StringComparer.OrdinalIgnoreCase).ToList();

        StructureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string GetSortValue(CustomerRow row, string path) => path switch
    {
        nameof(CustomerRow.Company) => row.Company,
        nameof(CustomerRow.ContactName) => row.ContactName,
        nameof(CustomerRow.Email) => row.Email,
        nameof(CustomerRow.Status) => row.Status,
        _ => string.Empty,
    };
}
