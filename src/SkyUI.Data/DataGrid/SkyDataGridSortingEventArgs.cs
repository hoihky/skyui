namespace SkyUI.DataGrid;

public sealed class SkyDataGridSortingEventArgs : EventArgs
{
    public SkyDataGridSortingEventArgs(SkyDataGridColumn column, SkyDataGridSortDirection newDirection)
    {
        Column = column;
        NewDirection = newDirection;
    }

    public SkyDataGridColumn Column { get; }

    public SkyDataGridSortDirection NewDirection { get; }
}
