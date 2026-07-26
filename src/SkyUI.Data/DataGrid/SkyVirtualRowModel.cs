using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.DataGrid;

/// <summary>Per-row view model for recycled row visuals (index + item from <see cref="IVirtualGridDataSource"/>).</summary>
public sealed class SkyVirtualRowModel : INotifyPropertyChanged
{
    private long _rowIndex = -1;
    private object? _item;
    private bool _isSelected;

    public long RowIndex
    {
        get => _rowIndex;
        private set
        {
            if (_rowIndex == value)
                return;
            _rowIndex = value;
            OnPropertyChanged();
        }
    }

    public object? Item
    {
        get => _item;
        private set
        {
            if (ReferenceEquals(_item, value))
                return;
            _item = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        private set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    internal void SetSelected(bool value) => IsSelected = value;

    public void Update(long rowIndex, object? item)
    {
        RowIndex = rowIndex;
        Item = item;
    }

    public void Clear()
    {
        SetSelected(false);
        Update(-1, null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
