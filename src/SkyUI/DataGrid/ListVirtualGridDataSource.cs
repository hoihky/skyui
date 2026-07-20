using System.Collections;
using System.Collections.Specialized;

namespace SkyUI.DataGrid;

/// <summary>Bridges an in-memory <see cref="IList"/> to <see cref="IVirtualGridDataSource"/> for moderate sizes (still virtualized in the grid UI).</summary>
public sealed class ListVirtualGridDataSource : IVirtualGridDataSource
{
    private IList _list = Array.Empty<object>();

    public IList List
    {
        get => _list;
        set
        {
            if (_notify != null && _handler != null)
                _notify.CollectionChanged -= _handler;
            _list = value ?? Array.Empty<object>();
            _notify = _list as INotifyCollectionChanged;
            if (_notify != null)
            {
                _handler = (_, _) => StructureChanged?.Invoke(this, EventArgs.Empty);
                _notify.CollectionChanged += _handler;
            }

            StructureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private INotifyCollectionChanged? _notify;
    private NotifyCollectionChangedEventHandler? _handler;

    public long RowCount => _list.Count;

    public object? GetRow(long index) =>
        index < 0 || index >= _list.Count ? null : _list[(int)index];

    public event EventHandler? StructureChanged;
}
