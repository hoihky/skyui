using SkyUI.DataGrid;

namespace SkyUI.UnitTests;

public class SkyVirtualDataGridClipboardTests
{
    [Fact]
    public void BuildSelectionRows_includes_headers_and_selected_row()
    {
        var grid = new SkyVirtualDataGrid
        {
            DataSource = new DemoSource(),
            SelectedRowIndex = 1,
        };
        grid.Columns.Add(new SkyDataGridColumn { Header = "Index", BindingPath = "Index" });
        grid.Columns.Add(new SkyDataGridColumn { Header = "Label", BindingPath = "Label" });

        var rows = SkyVirtualDataGridClipboard.BuildSelectionRows(grid);

        Assert.Equal(2, rows.Count);
        Assert.Equal(["Index", "Label"], rows[0]);
        Assert.Equal(["1", "Row 1"], rows[1]);
    }

    [Fact]
    public void BuildSelectionRows_without_selection_returns_headers_only_when_requested()
    {
        var grid = new SkyVirtualDataGrid
        {
            DataSource = new DemoSource(),
            SelectedRowIndex = null,
        };
        grid.Columns.Add(new SkyDataGridColumn { Header = "Index", BindingPath = "Index" });

        var rows = SkyVirtualDataGridClipboard.BuildSelectionRows(grid, includeHeaders: true);
        Assert.Single(rows);
        Assert.Equal(["Index"], rows[0]);
    }

    private sealed class DemoSource : IVirtualGridDataSource
    {
        private readonly object?[] rows =
        [
            new DemoRow { Index = 0, Label = "Row 0" },
            new DemoRow { Index = 1, Label = "Row 1" },
        ];

        public long RowCount => rows.Length;

        public object? GetRow(long index) => index >= 0 && index < rows.Length ? rows[index] : null;

#pragma warning disable CS0067
        public event EventHandler? StructureChanged;
#pragma warning restore CS0067
    }

    private sealed class DemoRow
    {
        public long Index { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
