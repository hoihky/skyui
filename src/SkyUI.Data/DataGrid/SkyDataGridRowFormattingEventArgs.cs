using Avalonia.Controls;

namespace SkyUI.DataGrid;

/// <summary>Conditional row / cell formatting hook (set <see cref="RowClasses"/> / <see cref="CellClasses"/>).</summary>
public sealed class SkyDataGridRowFormattingEventArgs : EventArgs
{
    public SkyDataGridRowFormattingEventArgs(long rowIndex, object? item, Control rowRoot, IReadOnlyList<Control> cells)
    {
        RowIndex = rowIndex;
        Item = item;
        RowRoot = rowRoot;
        Cells = cells;
    }

    public long RowIndex { get; }

    public object? Item { get; }

    public Control RowRoot { get; }

    public IReadOnlyList<Control> Cells { get; }

    public IList<string> RowClasses { get; } = new List<string>();

    public IList<IList<string>> CellClasses { get; } = new List<IList<string>>();
}
