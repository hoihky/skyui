namespace SkyUI.DataGrid;

public sealed class SkyDataGridCellEditEventArgs : EventArgs
{
    public SkyDataGridCellEditEventArgs(long rowIndex, SkyDataGridColumn column, string newText)
    {
        RowIndex = rowIndex;
        Column = column;
        NewText = newText;
    }

    public long RowIndex { get; }

    public SkyDataGridColumn Column { get; }

    public string NewText { get; }
}
