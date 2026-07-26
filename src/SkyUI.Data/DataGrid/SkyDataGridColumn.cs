using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Templates;

namespace SkyUI.DataGrid;

/// <summary>Column definition for <see cref="SkyVirtualDataGrid"/> (MVVM-friendly INPC).</summary>
public sealed class SkyDataGridColumn : INotifyPropertyChanged
{
    private string _header = "";
    private double _width = 120;
    private string? _bindingPath;
    private bool _isReadOnly = true;
    private SkyDataGridSortDirection _sortDirection = SkyDataGridSortDirection.None;
    private IDataTemplate? _cellTemplate;

    public string Header
    {
        get => _header;
        set
        {
            if (_header == value)
                return;
            _header = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Pixel width (fixed layout for predictable virtualization).</summary>
    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) < 0.5)
                return;
            _width = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Path on the row item for simple text cells (ignored when <see cref="CellTemplate"/> is set).</summary>
    public string? BindingPath
    {
        get => _bindingPath;
        set
        {
            if (_bindingPath == value)
                return;
            _bindingPath = value;
            OnPropertyChanged();
        }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly == value)
                return;
            _isReadOnly = value;
            OnPropertyChanged();
        }
    }

    public SkyDataGridSortDirection SortDirection
    {
        get => _sortDirection;
        set
        {
            if (_sortDirection == value)
                return;
            _sortDirection = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Optional per-cell template; data context is the row item (same as <see cref="SkyVirtualRowModel.Item"/>).</summary>
    public IDataTemplate? CellTemplate
    {
        get => _cellTemplate;
        set
        {
            if (ReferenceEquals(_cellTemplate, value))
                return;
            _cellTemplate = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
