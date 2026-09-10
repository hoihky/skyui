using SkyUI.DataGrid;

namespace SkyUI.UnitTests;

public class SkyDataGridCellFormatterTests
{
    [Fact]
    public void FormatCell_resolves_binding_path()
    {
        var row = new DemoRow { Index = 42, Label = "Test" };
        var column = new SkyDataGridColumn { Header = "Index", BindingPath = "Index" };

        Assert.Equal("42", SkyDataGridCellFormatter.FormatCell(row, column));
    }

    [Fact]
    public void ResolveValue_supports_nested_paths()
    {
        var row = new ParentRow { Child = new DemoRow { Label = "Nested" } };
        Assert.Equal("Nested", SkyDataGridCellFormatter.ResolveValue(row, "Child.Label"));
    }

    private sealed class DemoRow
    {
        public long Index { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    private sealed class ParentRow
    {
        public DemoRow Child { get; set; } = new();
    }
}
