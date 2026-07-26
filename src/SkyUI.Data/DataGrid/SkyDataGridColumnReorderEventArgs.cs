namespace SkyUI.DataGrid;

public sealed class SkyDataGridColumnReorderEventArgs : EventArgs
{
    public SkyDataGridColumnReorderEventArgs(SkyDataGridColumn column, int delta)
    {
        Column = column;
        Delta = delta;
    }

    public SkyDataGridColumn Column { get; }

    public int Delta { get; }
}
