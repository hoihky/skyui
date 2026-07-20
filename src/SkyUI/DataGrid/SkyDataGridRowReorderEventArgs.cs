namespace SkyUI.DataGrid;

public sealed class SkyDataGridRowReorderEventArgs : EventArgs
{
    public SkyDataGridRowReorderEventArgs(long rowIndex) => RowIndex = rowIndex;

    public long RowIndex { get; }
}
