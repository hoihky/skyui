using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SkyUI.DataGrid;

/// <summary>
/// Data grid over <see cref="IVirtualGridDataSource"/>: columns, sort/export hooks, optional in-place edits.
/// When <see cref="EnableRowVirtualization"/> is true (default), recycles a fixed pool of row visuals; when false,
/// materializes one row per logical record (use only for modest row counts).
/// </summary>
public class SkyVirtualDataGrid : TemplatedControl
{
    /// <summary>Template part: single <see cref="ScrollViewer"/> for horizontal and vertical scroll.</summary>
    public const string PartScroll = "PART_Scroll";
    public const string PartScrollRoot = "PART_ScrollRoot";
    public const string PartHeaderGrid = "PART_HeaderGrid";
    public const string PartVirtualExtent = "PART_VirtualExtent";
    public const string PartRows = "PART_Rows";
    public const string PartColumnDragIndicator = "PART_ColumnDragIndicator";

    public static readonly StyledProperty<IVirtualGridDataSource?> DataSourceProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, IVirtualGridDataSource?>(nameof(DataSource));

    public static readonly StyledProperty<double> RowHeightProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, double>(nameof(RowHeight), 28);

    public static readonly StyledProperty<bool> AllowColumnReorderProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, bool>(nameof(AllowColumnReorder), true);

    public static readonly StyledProperty<bool> AllowRowReorderProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, bool>(nameof(AllowRowReorder));

    public static readonly StyledProperty<long?> SelectedRowIndexProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, long?>(nameof(SelectedRowIndex));

    public static readonly StyledProperty<bool> EnableRowVirtualizationProperty =
        AvaloniaProperty.Register<SkyVirtualDataGrid, bool>(nameof(EnableRowVirtualization), defaultValue: true);

    private ScrollViewer? _scroll;
    private SkyVirtualDataGridScrollRoot? _scrollRoot;
    private ScrollContentPresenter? _scrollPresenter;
    private Grid? _headerGrid;
    private SkyVirtualDataGridScrollHost? _virtualExtent;
    private Border? _columnDragIndicator;
    private ItemsControl? _rowsItems;
    private readonly ObservableCollection<SkyVirtualRowModel> _rowModels = new();
    private readonly SkyVirtualDataGridUpdateCoordinator _coordinator;
    private readonly HashSet<SkyDataGridColumn> _columnHooks = new();
    private IVirtualGridDataSource? _dataSubscription;
    private int _poolSize;
    private bool _rowFormatPending;
    private bool _rowVisualPending;
    private int? _headerPointerColumn;
    private Point _headerPointerOrigin;
    private bool _headerDragged;
    private double _lastStableViewport;
    private Border? _headerDragSource;
    private long _lastScrollFirst = long.MinValue;
    private double _lastHeaderExtentHeight = double.NaN;
    private double _lastScrollOffsetY = double.NaN;
    private double _lastBodyViewportHeight = double.NaN;

    public SkyVirtualDataGrid()
    {
        Columns = new ObservableCollection<SkyDataGridColumn>();
        Columns.CollectionChanged += OnColumnsCollectionChanged;
        _coordinator = new SkyVirtualDataGridUpdateCoordinator(InvalidateVisibleRowsCore);
    }

    static SkyVirtualDataGrid()
    {
        DataSourceProperty.Changed.AddClassHandler<SkyVirtualDataGrid>((o, _) => o.OnDataSourceChanged());
        RowHeightProperty.Changed.AddClassHandler<SkyVirtualDataGrid>((o, _) => o.OnRowHeightChanged());
        SelectedRowIndexProperty.Changed.AddClassHandler<SkyVirtualDataGrid>((o, _) => o.OnSelectedRowIndexChanged());
        EnableRowVirtualizationProperty.Changed.AddClassHandler<SkyVirtualDataGrid>((o, _) => o.OnEnableRowVirtualizationChanged());
    }

    public ObservableCollection<SkyDataGridColumn> Columns { get; }

    public IVirtualGridDataSource? DataSource
    {
        get => GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public double RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public bool AllowColumnReorder
    {
        get => GetValue(AllowColumnReorderProperty);
        set => SetValue(AllowColumnReorderProperty, value);
    }

    public bool AllowRowReorder
    {
        get => GetValue(AllowRowReorderProperty);
        set => SetValue(AllowRowReorderProperty, value);
    }

    public long? SelectedRowIndex
    {
        get => GetValue(SelectedRowIndexProperty);
        set => SetValue(SelectedRowIndexProperty, value);
    }

    public bool EnableRowVirtualization
    {
        get => GetValue(EnableRowVirtualizationProperty);
        set => SetValue(EnableRowVirtualizationProperty, value);
    }

    public event EventHandler<SkyDataGridSortingEventArgs>? Sorting;

    public event EventHandler<SkyDataGridRowFormattingEventArgs>? RowFormatting;

    public event EventHandler<SkyDataGridColumnReorderEventArgs>? ColumnReorderRequested;

    public event EventHandler<SkyDataGridRowReorderEventArgs>? RowReorderRequested;

    public event EventHandler<SkyDataGridCellEditEventArgs>? CellEditCommitted;

    public void Post(Action action) => Post(action, DispatcherPriority.Normal);

    public void Post(Action action, DispatcherPriority priority) =>
        Dispatcher.UIThread.Post(action, priority);

    public void InvalidateStructure()
    {
        _lastScrollFirst = long.MinValue;
        ResizeRowPool(0);
        _coordinator.RequestRefresh();
    }

    public void InvalidateVisibleRows() => InvalidateVisibleRowsCore();

    public Task ExportToCsvAsync(Stream destination, long startIndex, long maxRows, CancellationToken ct = default)
    {
        var ds = DataSource ?? throw new InvalidOperationException("DataSource is required.");
        var exporter = new SkyDataGridCsvExporter();
        return exporter.ExportAsync(ds, Columns.ToList(), destination, startIndex, maxRows, ct);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachScrollHandlers();
        if (_headerGrid != null)
            _headerGrid.LayoutUpdated -= OnHeaderGridLayoutUpdated;
        _scroll = e.NameScope.Find<ScrollViewer>(PartScroll);
        _scrollRoot = e.NameScope.Find<SkyVirtualDataGridScrollRoot>(PartScrollRoot)
            ?? (_scroll?.Content as SkyVirtualDataGridScrollRoot);
        _scrollPresenter = _scroll?.Presenter as ScrollContentPresenter;
        _headerGrid = e.NameScope.Find<Grid>(PartHeaderGrid);
        if (_headerGrid != null)
            _headerGrid.LayoutUpdated += OnHeaderGridLayoutUpdated;
        _lastHeaderExtentHeight = double.NaN;
        _virtualExtent = e.NameScope.Find<SkyVirtualDataGridScrollHost>(PartVirtualExtent);
        _virtualExtent?.RegisterTemplateChildren();
        _columnDragIndicator = e.NameScope.Find<Border>(PartColumnDragIndicator);
        _rowsItems = e.NameScope.Find<ItemsControl>(PartRows);
        if (_rowsItems != null)
        {
            _rowsItems.ItemsSource = _rowModels;
            _rowsItems.ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Vertical });
        }

        foreach (var c in Columns)
            EnsureColumnHook(c);

        AttachScrollHandlers();
        RebuildHeader();
        ResizeRowPool(0);
        SyncRowItemTemplate();
        if (_scrollRoot != null)
            AttachScrollRoot(_scrollRoot);
        else
            UpdateVirtualExtent();
        InvalidateVisibleRowsCore();
        if (EnableRowVirtualization)
        {
            Dispatcher.UIThread.Post(
                () =>
                {
                    if (DataSource == null)
                        return;
                    _lastBodyViewportHeight = double.NaN;
                    ResizeRowPool(0);
                    InvalidateVisibleRowsCore();
                },
                DispatcherPriority.Loaded);
        }
    }

    /// <summary>Viewport size for <see cref="ILogicalScrollable"/> (must not be conflated with logical extent).</summary>
    internal Size GetScrollViewportSize()
    {
        var w = 1.0;
        var h = 32.0;
        if (_scroll != null)
        {
            var b = _scroll.Bounds;
            if (b.Width >= 1 && double.IsFinite(b.Width))
                w = b.Width;
            if (b.Height >= 32 && double.IsFinite(b.Height))
                h = Math.Max(h, b.Height);

            var v = _scroll.Viewport;
            if (v.Width >= 1 && double.IsFinite(v.Width))
                w = Math.Max(w, v.Width);
            if (v.Height >= 32 && double.IsFinite(v.Height))
                h = Math.Max(h, v.Height);
        }

        var grid = Bounds;
        if (grid.Width >= 1)
            w = Math.Max(w, grid.Width);
        if (grid.Height >= 32)
            h = Math.Max(h, grid.Height);

        return new Size(Math.Max(1, w), Math.Max(32, h));
    }

    internal void AttachScrollRoot(SkyVirtualDataGridScrollRoot root)
    {
        if (_scrollRoot != null && !ReferenceEquals(_scrollRoot, root))
            return;
        _scrollRoot = root;
        root.Attach(this);
        UpdateVirtualExtent();
        root.NotifyScrollMetricsChanged();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DetachScrollHandlers();
        foreach (var c in Columns.ToList())
            DropColumnHook(c);
        if (_dataSubscription != null)
            _dataSubscription.StructureChanged -= OnDataStructureChanged;
        if (_headerGrid != null)
            _headerGrid.LayoutUpdated -= OnHeaderGridLayoutUpdated;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (EnableRowVirtualization)
            ResizeRowPool();
        InvalidateVisibleRowsCore();
    }

    private void OnDataSourceChanged()
    {
        if (_dataSubscription != null)
            _dataSubscription.StructureChanged -= OnDataStructureChanged;
        _dataSubscription = DataSource;
        if (_dataSubscription != null)
            _dataSubscription.StructureChanged += OnDataStructureChanged;
        _lastScrollFirst = long.MinValue;
        ResizeRowPool(0);
        UpdateVirtualExtent();
        _scrollRoot?.NotifyScrollMetricsChanged();
        InvalidateVisibleRowsCore();
    }

    private void OnDataStructureChanged(object? sender, EventArgs e) => _coordinator.RequestRefresh();

    private void OnRowHeightChanged()
    {
        UpdateVirtualExtent();
        InvalidateVisibleRowsCore();
    }

    private void OnSelectedRowIndexChanged() => InvalidateVisibleRowsCore();

    private void OnEnableRowVirtualizationChanged()
    {
        _lastScrollFirst = long.MinValue;
        ResizeRowPool(0);
        UpdateVirtualExtent();
        InvalidateVisibleRowsCore();
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (SkyDataGridColumn c in e.NewItems)
                EnsureColumnHook(c);
        }

        if (e.OldItems != null)
        {
            foreach (SkyDataGridColumn c in e.OldItems)
                DropColumnHook(c);
        }

        RebuildHeader();
        UpdateVirtualExtent();
        SyncRowItemTemplate();
        InvalidateVisibleRowsCore();
    }

    private void EnsureColumnHook(SkyDataGridColumn c)
    {
        if (_columnHooks.Add(c))
            c.PropertyChanged += OnColumnPropertyChanged;
    }

    private void DropColumnHook(SkyDataGridColumn c)
    {
        if (_columnHooks.Remove(c))
            c.PropertyChanged -= OnColumnPropertyChanged;
    }

    private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SkyDataGridColumn.Width) or nameof(SkyDataGridColumn.Header) or nameof(SkyDataGridColumn.SortDirection))
            RebuildHeader();
        if (e.PropertyName is nameof(SkyDataGridColumn.Width) or nameof(SkyDataGridColumn.BindingPath) or nameof(SkyDataGridColumn.CellTemplate))
            SyncRowItemTemplate();
        InvalidateVisibleRowsCore();
    }

    private void AttachScrollHandlers()
    {
        if (_scroll != null)
        {
            // Virtual row windowing reads Offset during drag; deferred scrolling only commits on thumb release.
            _scroll.SetCurrentValue(ScrollViewer.IsDeferredScrollingEnabledProperty, false);
            _scroll.ScrollChanged += OnScrollChanged;
            _scroll.PropertyChanged += OnScrollPropertyChanged;
            _scroll.LayoutUpdated += OnScrollLayoutUpdated;
            _scroll.SizeChanged += OnScrollSizeChanged;
        }

        if (_scrollPresenter != null)
            _scrollPresenter.LayoutUpdated += OnScrollLayoutUpdated;
    }

    private void DetachScrollHandlers()
    {
        if (_scroll != null)
        {
            _scroll.ScrollChanged -= OnScrollChanged;
            _scroll.PropertyChanged -= OnScrollPropertyChanged;
            _scroll.LayoutUpdated -= OnScrollLayoutUpdated;
            _scroll.SizeChanged -= OnScrollSizeChanged;
        }

        if (_scrollPresenter != null)
            _scrollPresenter.LayoutUpdated -= OnScrollLayoutUpdated;
    }

    private void OnScrollSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!EnableRowVirtualization)
            return;
        if (e.NewSize.Height < 8)
            return;
        _lastBodyViewportHeight = double.NaN;
        ResizeRowPool(0);
        InvalidateVisibleRowsCore();
    }

    private void OnScrollLayoutUpdated(object? sender, EventArgs e)
    {
        if (!EnableRowVirtualization)
            return;

        var y = GetScrollOffsetY();
        var vh = GetBodyViewportHeight();
        var offsetChanged = !double.IsFinite(_lastScrollOffsetY) || Math.Abs(y - _lastScrollOffsetY) >= 0.5;
        var viewportChanged = !double.IsFinite(_lastBodyViewportHeight) || Math.Abs(vh - _lastBodyViewportHeight) >= 0.5;
        if (!offsetChanged && !viewportChanged)
            return;

        _lastScrollOffsetY = y;
        _lastBodyViewportHeight = vh;
        if (viewportChanged)
            ResizeRowPool(0);
        InvalidateVisibleRowsCore();
    }

    private void OnScrollPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollViewer.OffsetProperty && ReferenceEquals(sender, _scroll))
            InvalidateVisibleRowsCore();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => InvalidateVisibleRowsCore();

    private double GetStableViewportHeight()
    {
        var raw = _scroll?.Viewport.Height ?? Bounds.Height;
        if (raw >= 32 && double.IsFinite(raw))
        {
            _lastStableViewport = raw;
            return raw;
        }

        if (_lastStableViewport >= 32)
            return _lastStableViewport;
        var bh = Bounds.Height;
        if (bh >= 32 && double.IsFinite(bh))
            return bh;
        return 400;
    }

    /// <summary>Vertical scroll offset used to map the recycled row pool onto logical indices.</summary>
    private double GetScrollOffsetY()
    {
        var fromPresenter = _scrollPresenter?.Offset.Y;
        if (fromPresenter is >= 0 and var py && double.IsFinite(py))
            return py;
        var fromViewer = _scroll?.Offset.Y ?? 0;
        return double.IsFinite(fromViewer) ? Math.Max(0, fromViewer) : 0;
    }

    /// <summary>Height of the scrollable body region (header is outside <see cref="PartScroll"/>).</summary>
    internal double GetBodyViewportHeight()
    {
        var rh = Math.Max(8, RowHeight);
        var best = 0.0;

        if (_scroll != null)
        {
            var bh = _scroll.Bounds.Height;
            if (bh >= rh && double.IsFinite(bh))
                best = bh;

            // With ILogicalScrollable, Viewport.Height can be far smaller than the visible area — prefer Bounds.
            var vv = _scroll.Viewport.Height;
            if (vv >= rh && double.IsFinite(vv))
                best = Math.Max(best, vv);
        }

        var hostH = _virtualExtent?.Bounds.Height ?? 0;
        if (hostH >= rh && double.IsFinite(hostH))
            best = Math.Max(best, hostH);

        if (best >= rh)
            return best;

        return Math.Max(rh, GetStableViewportHeight());
    }

    private int ComputeVirtualRowPoolSize(double bodyViewport, double rh, long rowCount)
    {
        if (rowCount <= 0)
            return 1;
        // Recycled row visuals only — does not limit scroll range (scroll extent comes from RowCount×RowHeight spacer).
        const int maxPool = 256;
        const int overscanRows = 6;
        var visibleRows = (int)Math.Ceiling(bodyViewport / rh) + overscanRows;
        var pool = Math.Max(1, visibleRows);
        pool = Math.Min(maxPool, pool);
        return (int)Math.Min(pool, rowCount);
    }

    private void InvalidateVisibleRowsCore()
    {
        var ds = DataSource;
        if (_rowsItems == null || _virtualExtent == null || ds == null)
            return;

        var rh = Math.Max(8, RowHeight);
        var count = ds.RowCount;

        if (!EnableRowVirtualization)
        {
            _rowsItems.Margin = default;
            var fullW = Columns.Sum(static c => c.Width);
            _rowsItems.Width = fullW;
            _rowsItems.Height = _rowModels.Count * rh;

            var selection = SelectedRowIndex;
            for (var i = 0; i < _rowModels.Count; i++)
            {
                if (i >= count)
                    _rowModels[i].Clear();
                else
                {
                    _rowModels[i].Update(i, ds.GetRow(i));
                    _rowModels[i].SetSelected(selection.HasValue && selection.Value == i);
                }
            }

            _lastScrollFirst = 0;
            ScheduleRowFormatting();
            ScheduleRowSelectionVisualSync();
            return;
        }

        var bodyViewport = GetBodyViewportHeight();
        var desiredPool = ComputeVirtualRowPoolSize(bodyViewport, rh, count);
        if (desiredPool != _poolSize)
            ResizeRowPool(desiredPool);

        var offsetY = GetScrollOffsetY();
        _lastScrollOffsetY = offsetY;
        // Header is outside PART_Scroll; offset Y is already the body scroll position.
        var bodyOffsetY = Math.Max(0, offsetY);
        var first = (long)(bodyOffsetY / rh);
        if (first < 0)
            first = 0;
        var maxFirst = Math.Max(0, count - _rowModels.Count);
        if (first > maxFirst)
            first = maxFirst;

        // Logical scroll: body host is viewport-sized — anchor rows at top with sub-row offset.
        // Physical scroll: tall spacer — position the pool with margin = first×RowHeight.
        var usesLogicalScroll = _scrollRoot?.IsLogicalScrollEnabled == true;
        if (usesLogicalScroll)
        {
            var partial = bodyOffsetY - first * rh;
            if (partial < 0 || !double.IsFinite(partial))
                partial = 0;
            if (partial >= rh)
                partial = 0;
            _rowsItems.Margin = new Thickness(0, -partial, 0, 0);
        }
        else
        {
            _rowsItems.Margin = new Thickness(0, first * rh, 0, 0);
        }

        var totalW = Columns.Sum(static c => c.Width);
        _rowsItems.Width = totalW;
        _rowsItems.Height = _rowModels.Count * rh;

        var sel = SelectedRowIndex;
        for (var i = 0; i < _rowModels.Count; i++)
        {
            var idx = first + i;
            if (idx >= count)
                _rowModels[i].Clear();
            else
            {
                _rowModels[i].Update(idx, ds.GetRow(idx));
                _rowModels[i].SetSelected(sel.HasValue && sel.Value == idx);
            }
        }

        if (first != _lastScrollFirst)
        {
            _lastScrollFirst = first;
            _rowsItems.InvalidateArrange();
        }

        ScheduleRowFormatting();
        ScheduleRowSelectionVisualSync();
    }

    private void ScheduleRowSelectionVisualSync()
    {
        if (_rowVisualPending)
            return;
        _rowVisualPending = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _rowVisualPending = false;
                if (_rowsItems is null)
                    return;
                for (var i = 0; i < _rowModels.Count; i++)
                {
                    var m = _rowModels[i];
                    if (_rowsItems.ContainerFromIndex(i) is not ContentPresenter { Child: Control row })
                        continue;
                    SyncRowSelectionClass(row, m);
                }
            },
            DispatcherPriority.Render);
    }

    private void ScheduleRowFormatting()
    {
        if (RowFormatting is null || _rowFormatPending)
            return;
        _rowFormatPending = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _rowFormatPending = false;
                if (_rowsItems is null)
                    return;
                for (var i = 0; i < _rowModels.Count; i++)
                {
                    var m = _rowModels[i];
                    if (m.RowIndex < 0)
                        continue;
                    if (_rowsItems.ContainerFromIndex(i) is not ContentPresenter { Child: Control row })
                        continue;
                    ApplyRowFormatting(row, m);
                }
            },
            DispatcherPriority.Render);
    }

    private static void SyncRowSelectionClass(Control rowRoot, SkyVirtualRowModel model)
    {
        if (model.IsSelected)
            rowRoot.Classes.Add("sky-grid-row-selected");
        else
            rowRoot.Classes.Remove("sky-grid-row-selected");
    }

    private void ApplyRowFormatting(Control rowRoot, SkyVirtualRowModel model)
    {
        if (RowFormatting is null)
            return;
        var cells = new List<Control>();
        if (rowRoot is Grid g)
        {
            foreach (var child in g.Children)
            {
                if (child is Control c)
                    cells.Add(c);
            }
        }

        var args = new SkyDataGridRowFormattingEventArgs(model.RowIndex, model.Item, rowRoot, cells);
        RowFormatting.Invoke(this, args);
        foreach (var cls in args.RowClasses)
            rowRoot.Classes.Add(cls);
        for (var i = 0; i < args.CellClasses.Count && i < cells.Count; i++)
        {
            var list = args.CellClasses[i];
            if (list is null)
                continue;
            foreach (var cls in list)
                cells[i].Classes.Add(cls);
        }
    }

    private void ResizeRowPool() => ResizeRowPool(_poolSize == 0 ? 0 : _poolSize);

    private void ResizeRowPool(int target)
    {
        var ds = DataSource;
        var rh = Math.Max(8, RowHeight);
        int computed;
        if (!EnableRowVirtualization && ds != null)
            computed = ClipRowCountToInt(ds.RowCount);
        else if (target > 0)
            computed = target;
        else if (ds != null)
            computed = ComputeVirtualRowPoolSize(GetBodyViewportHeight(), rh, ds.RowCount);
        else
            computed = 1;

        while (_rowModels.Count < computed)
            _rowModels.Add(new SkyVirtualRowModel());
        while (_rowModels.Count > computed)
            _rowModels.RemoveAt(_rowModels.Count - 1);

        _poolSize = computed;
        UpdateVirtualExtent();
    }

    private void UpdateVirtualExtent()
    {
        var ds = DataSource;
        var rh = Math.Max(8, RowHeight);
        var bodyH = ds == null ? 0 : (double)ds.RowCount * rh;
        var w = Columns.Sum(static c => c.Width);
        var usePhysicalSpacer = !EnableRowVirtualization || ds == null;

        if (_virtualExtent != null)
        {
            _virtualExtent.ApplyBodyExtent(bodyH, w, usePhysicalSpacer);
            _virtualExtent.InvalidateMeasure();
        }

        if (_headerGrid != null)
            _headerGrid.MinWidth = w;

        if (_scrollRoot != null)
        {
            // Header is not in the scroll viewer; logical extent is the virtual body only.
            _scrollRoot.SetLogicalExtent(new Size(Math.Max(1, w), bodyH));
            _scrollRoot.NotifyScrollMetricsChanged();
            _scrollRoot.InvalidateMeasure();
        }

        _scroll?.InvalidateMeasure();
    }

    private void OnHeaderGridLayoutUpdated(object? sender, EventArgs e)
    {
        var h = GetHeaderPixelHeight();
        if (double.IsFinite(_lastHeaderExtentHeight) && Math.Abs(h - _lastHeaderExtentHeight) < 0.5)
            return;
        _lastHeaderExtentHeight = h;
        UpdateVirtualExtent();
        InvalidateVisibleRowsCore();
    }

    private double GetHeaderPixelHeight()
    {
        if (_headerGrid == null)
            return 0;
        var h = _headerGrid.Bounds.Height;
        if (h >= 1 && double.IsFinite(h))
            return h;
        h = _headerGrid.DesiredSize.Height;
        if (h >= 1 && double.IsFinite(h))
            return h;
        return 32;
    }

    private void SyncRowItemTemplate()
    {
        if (_rowsItems == null)
            return;
        _rowsItems.ItemTemplate = new FuncDataTemplate<SkyVirtualRowModel?>(BuildRow, supportsRecycling: false);
    }

    private void RebuildHeader()
    {
        if (_headerGrid == null)
            return;
        _headerGrid.Children.Clear();
        _headerGrid.ColumnDefinitions.Clear();
        foreach (var col in Columns)
            _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(col.Width, GridUnitType.Pixel));

        for (var i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4),
                Background = new SolidColorBrush(Color.Parse("#2a2a2a")),
                Cursor = new Cursor(StandardCursorType.Hand),
                [Grid.ColumnProperty] = i,
                Tag = i,
            };
            var headerText = col.SortDirection switch
            {
                SkyDataGridSortDirection.Ascending => $"{col.Header} ▲",
                SkyDataGridSortDirection.Descending => $"{col.Header} ▼",
                _ => col.Header,
            };
            border.Child = new TextBlock { Text = headerText, VerticalAlignment = VerticalAlignment.Center };
            border.PointerPressed += HeaderBorderPointerPressed;
            border.PointerMoved += HeaderBorderPointerMoved;
            border.PointerReleased += HeaderBorderPointerReleased;
            _headerGrid.Children.Add(border);
        }
    }

    private void HeaderBorderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border { Tag: int colIndex } || _headerGrid is null)
            return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        _headerPointerColumn = colIndex;
        _headerPointerOrigin = e.GetPosition(_headerGrid);
        _headerDragged = false;
        _headerDragSource = sender as Border;
        if (_columnDragIndicator != null)
            _columnDragIndicator.IsVisible = false;
        e.Pointer.Capture(sender as Border);
    }

    private void HeaderBorderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_headerPointerColumn is null || _headerGrid is null || !AllowColumnReorder)
            return;
        var p = e.GetPosition(_headerGrid);
        var dx = p.X - _headerPointerOrigin.X;
        var dy = p.Y - _headerPointerOrigin.Y;
        if (dx * dx + dy * dy > 36)
            _headerDragged = true;

        if (_headerDragged)
        {
            if (_headerDragSource != null)
                _headerDragSource.Opacity = 0.55;
            if (_columnDragIndicator != null && _headerGrid.Bounds.Width > 0)
            {
                _columnDragIndicator.IsVisible = true;
                const double indW = 3;
                var x = Math.Clamp(p.X, 0, Math.Max(0, _headerGrid.Bounds.Width - indW));
                _columnDragIndicator.Margin = new Thickness(x, 0, 0, 0);
            }
        }
    }

    private void HeaderBorderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Border { Tag: int fromIndex })
            return;
        e.Pointer.Capture(null);
        try
        {
            if (_headerPointerColumn is null || _headerGrid is null)
                return;

            if (AllowColumnReorder && _headerDragged)
            {
                var x = e.GetPosition(_headerGrid).X;
                var toIndex = GetColumnIndexAtX(x);
                if (toIndex != fromIndex)
                {
                    var col = Columns[fromIndex];
                    var delta = toIndex - fromIndex;
                    MoveColumn(fromIndex, toIndex);
                    ColumnReorderRequested?.Invoke(this, new SkyDataGridColumnReorderEventArgs(col, delta));
                }

                return;
            }

            if (!_headerDragged)
                ApplyHeaderSort(fromIndex);
        }
        finally
        {
            if (_headerDragSource != null)
            {
                _headerDragSource.Opacity = 1;
                _headerDragSource = null;
            }

            if (_columnDragIndicator != null)
                _columnDragIndicator.IsVisible = false;
            _headerPointerColumn = null;
            _headerDragged = false;
        }
    }

    private int GetColumnIndexAtX(double x)
    {
        if (Columns.Count == 0)
            return 0;
        var acc = 0.0;
        for (var i = 0; i < Columns.Count; i++)
        {
            var w = Columns[i].Width;
            if (x < acc + w)
                return i;
            acc += w;
        }

        return Columns.Count - 1;
    }

    private void MoveColumn(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0)
            return;
        toIndex = Math.Clamp(toIndex, 0, Columns.Count - 1);
        if (fromIndex >= Columns.Count)
            return;
        var col = Columns[fromIndex];
        Columns.RemoveAt(fromIndex);
        var insert = toIndex > fromIndex ? toIndex - 1 : toIndex;
        insert = Math.Clamp(insert, 0, Columns.Count);
        Columns.Insert(insert, col);
    }

    private void ApplyHeaderSort(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= Columns.Count)
            return;
        var column = Columns[columnIndex];
        var next = column.SortDirection switch
        {
            SkyDataGridSortDirection.None => SkyDataGridSortDirection.Ascending,
            SkyDataGridSortDirection.Ascending => SkyDataGridSortDirection.Descending,
            _ => SkyDataGridSortDirection.None,
        };

        foreach (var c in Columns)
        {
            if (!ReferenceEquals(c, column))
                c.SortDirection = SkyDataGridSortDirection.None;
        }

        column.SortDirection = next;
        Sorting?.Invoke(this, new SkyDataGridSortingEventArgs(column, next));
        DataSource?.ApplySort(column, next);
        InvalidateVisibleRowsCore();
    }

    private Control BuildRow(SkyVirtualRowModel? model, INameScope _)
    {
        var grid = new Grid { MinHeight = RowHeight, Height = RowHeight };
        foreach (var col in Columns)
            grid.ColumnDefinitions.Add(new ColumnDefinition(col.Width, GridUnitType.Pixel));

        if (model is null)
            return grid;

        grid.Classes.Add("sky-grid-row");
        grid.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(grid).Properties.IsLeftButtonPressed)
                return;
            if (model.RowIndex < 0)
                return;
            SetCurrentValue(SelectedRowIndexProperty, model.RowIndex);
        };

        for (var i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];
            var cell = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 2),
                [Grid.ColumnProperty] = i,
            };

            if (col.CellTemplate is { } tpl)
            {
                var host = new ContentPresenter { ContentTemplate = tpl };
                host.Bind(ContentPresenter.ContentProperty, new Binding(nameof(SkyVirtualRowModel.Item)) { Source = model });
                cell.Child = host;
            }
            else if (!string.IsNullOrEmpty(col.BindingPath))
            {
                var tb = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
                tb.Bind(TextBlock.TextProperty, new Binding($"Item.{col.BindingPath}") { Source = model, Mode = BindingMode.OneWay });
                cell.Child = tb;
                if (!col.IsReadOnly)
                {
                    cell.Cursor = new Cursor(StandardCursorType.Ibeam);
                    cell.DoubleTapped += (_, _) => BeginCellEdit(cell, tb, model, col);
                }
            }
            else
            {
                cell.Child = new TextBlock { Text = "—", VerticalAlignment = VerticalAlignment.Center };
            }

            grid.Children.Add(cell);
        }

        if (AllowRowReorder)
        {
            grid.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(grid).Properties.IsRightButtonPressed)
                    RowReorderRequested?.Invoke(this, new SkyDataGridRowReorderEventArgs(model.RowIndex));
            };
        }

        return grid;
    }

    private void BeginCellEdit(Border host, TextBlock tb, SkyVirtualRowModel model, SkyDataGridColumn column)
    {
        var editor = new TextBox
        {
            Text = tb.Text,
            VerticalAlignment = VerticalAlignment.Stretch,
            FontSize = tb.FontSize,
        };
        host.Child = editor;
        editor.Focus();
        editor.LostFocus += (_, _) => CommitCellEdit(host, editor, tb, model, column);
        editor.KeyDown += (_, ke) =>
        {
            if (ke.Key == Key.Enter)
            {
                CommitCellEdit(host, editor, tb, model, column);
                ke.Handled = true;
            }
        };
    }

    private void CommitCellEdit(Border host, TextBox editor, TextBlock tb, SkyVirtualRowModel model, SkyDataGridColumn column)
    {
        var newText = editor.Text ?? "";
        tb.Text = newText;
        host.Child = tb;
        if (!string.IsNullOrEmpty(column.BindingPath) && model.Item != null)
            TrySetProperty(model.Item, column.BindingPath, newText);
        CellEditCommitted?.Invoke(this, new SkyDataGridCellEditEventArgs(model.RowIndex, column, newText));
    }

    private static int ClipRowCountToInt(long rowCount)
    {
        if (rowCount <= 0)
            return 0;
        return (int)Math.Min(rowCount, int.MaxValue - 8);
    }

    private static void TrySetProperty(object target, string path, string value)
    {
        var parts = path.Split('.');
        object? current = target;
        for (var i = 0; i < parts.Length; i++)
        {
            if (current is null)
                return;
            var prop = current.GetType().GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is null)
                return;
            if (i == parts.Length - 1)
            {
                try
                {
                    var converted = Convert.ChangeType(value, prop.PropertyType, System.Globalization.CultureInfo.CurrentCulture);
                    prop.SetValue(current, converted);
                }
                catch
                {
                    /* ignore */
                }
            }
            else
            {
                current = prop.GetValue(current);
            }
        }
    }
}
