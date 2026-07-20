using Avalonia.Threading;

namespace SkyUI.DataGrid;

/// <summary>Coalesces high-frequency invalidations onto a single UI-thread post (low latency under burst updates).</summary>
public sealed class SkyVirtualDataGridUpdateCoordinator
{
    private readonly Action _flush;
    private bool _pending;
    private readonly object _gate = new();

    public SkyVirtualDataGridUpdateCoordinator(Action flush) => _flush = flush;

    public void RequestRefresh()
    {
        lock (_gate)
        {
            if (_pending)
                return;
            _pending = true;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                lock (_gate)
                    _pending = false;
                _flush();
            },
            DispatcherPriority.Background);
    }
}
