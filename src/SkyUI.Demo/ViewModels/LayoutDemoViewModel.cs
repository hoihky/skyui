using System.ComponentModel;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for layout controls and responsive grids.</summary>
public sealed class LayoutDemoViewModel : INotifyPropertyChanged
{
    private int _columnCount = 1;
    private string _status = "Resize the window to see responsive column changes.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            if (_columnCount == value)
                return;
            _columnCount = value;
            Notify(nameof(ColumnCount), nameof(ColumnCountLabel));
        }
    }

    public string ColumnCountLabel => $"Columns at current width: {ColumnCount}";

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;
            _status = value;
            Notify(nameof(Status));
        }
    }

    public void UpdateColumnCount(double width) =>
        ColumnCount = SkyGridBreakpoint.ResolveColumns(width);

    public void OnExpanderExpanded(bool expanded) =>
        Status = expanded ? "Expander opened." : "Expander collapsed.";

    private void Notify(params string[] names)
    {
        foreach (var name in names)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
