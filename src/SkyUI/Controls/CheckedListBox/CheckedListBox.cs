using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>
/// Hierarchical checklist: pluggable <see cref="ICheckedListItemAdapter"/>, optional <see cref="ItemComparer"/> sorting,
/// <see cref="CheckedListBoxSelectionMode"/>, tri-state cascade via <see cref="CheckedListCheckCoordinator"/>, and
/// UI virtualization via <see cref="VirtualizingStackPanel"/> for row visuals. Flattened visible rows are held in
/// <see cref="Rows"/> (expanded subtree only); extremely large expanded trees still allocate one row model per visible node.
/// </summary>
public class CheckedListBox : TemplatedControl
{
    public const string PartScrollViewer = "PART_ScrollViewer";
    public const string PartItemsHost = "PART_ItemsHost";
    public const string PartReorderOverlay = "PART_ReorderOverlay";

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<CheckedListBox, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<CheckedListBox, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<CheckedListBoxSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<CheckedListBox, CheckedListBoxSelectionMode>(nameof(SelectionMode));

    public static readonly StyledProperty<ICheckedListItemAdapter?> ItemAdapterProperty =
        AvaloniaProperty.Register<CheckedListBox, ICheckedListItemAdapter?>(nameof(ItemAdapter));

    public static readonly StyledProperty<double> IndentProperty =
        AvaloniaProperty.Register<CheckedListBox, double>(nameof(Indent), 18);

    public static readonly StyledProperty<bool> CascadeToChildrenProperty =
        AvaloniaProperty.Register<CheckedListBox, bool>(nameof(CascadeToChildren), true);

    public static readonly StyledProperty<bool> UseThreeStateForParentsProperty =
        AvaloniaProperty.Register<CheckedListBox, bool>(nameof(UseThreeStateForParents), true);

    public static readonly StyledProperty<bool> ShowCheckBoxesProperty =
        AvaloniaProperty.Register<CheckedListBox, bool>(nameof(ShowCheckBoxes), true);

    public static readonly StyledProperty<bool> AllowReorderProperty =
        AvaloniaProperty.Register<CheckedListBox, bool>(nameof(AllowReorder));

    public static readonly StyledProperty<bool> AllowInlineEditProperty =
        AvaloniaProperty.Register<CheckedListBox, bool>(nameof(AllowInlineEdit));

    public static readonly StyledProperty<ICheckedListEditableAdapter?> EditableAdapterProperty =
        AvaloniaProperty.Register<CheckedListBox, ICheckedListEditableAdapter?>(nameof(EditableAdapter));

    public static readonly StyledProperty<ICheckedListRowActionProvider?> RowActionProviderProperty =
        AvaloniaProperty.Register<CheckedListBox, ICheckedListRowActionProvider?>(nameof(RowActionProvider));

    private readonly ObservableCollection<CheckedListRowModel> _rows = new();
    private readonly HashSet<object> loadingItems = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyCollectionChanged> subscribedCollections = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, object?> _parents = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, PropertyChangedEventHandler> _itemHandlers = new(ReferenceEqualityComparer.Instance);
    private INotifyCollectionChanged? _rootNotify;
    private NotifyCollectionChangedEventHandler? _rootCollectionHandler;
    private ScrollViewer? _scroll;
    private ItemsControl? _itemsHost;
    private Panel? reorderOverlay;
    private bool _rebuildScheduled;
    private IComparer<object?>? _itemComparer;
    private CheckedListDragReorderHandler? dragReorderHandler;

    public CheckedListBox()
    {
        ItemAdapter = new DefaultCheckedListItemAdapter();
        dragReorderHandler = new CheckedListDragReorderHandler(this);
    }

    static CheckedListBox()
    {
        ItemsSourceProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.OnItemsSourceChanged());
        ItemAdapterProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.RebuildAll());
        ItemTemplateProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        IndentProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        ShowCheckBoxesProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        AllowInlineEditProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        RowActionProviderProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
    }

    internal CheckedListDragReorderHandler DragReorderHandler => dragReorderHandler!;

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public CheckedListBoxSelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public ICheckedListItemAdapter? ItemAdapter
    {
        get => GetValue(ItemAdapterProperty);
        set => SetValue(ItemAdapterProperty, value);
    }

    public double Indent
    {
        get => GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    public bool CascadeToChildren
    {
        get => GetValue(CascadeToChildrenProperty);
        set => SetValue(CascadeToChildrenProperty, value);
    }

    public bool UseThreeStateForParents
    {
        get => GetValue(UseThreeStateForParentsProperty);
        set => SetValue(UseThreeStateForParentsProperty, value);
    }

    /// <summary>When false, hides checkbox column (tree-only selection).</summary>
    public bool ShowCheckBoxes
    {
        get => GetValue(ShowCheckBoxesProperty);
        set => SetValue(ShowCheckBoxesProperty, value);
    }

    public bool AllowReorder
    {
        get => GetValue(AllowReorderProperty);
        set => SetValue(AllowReorderProperty, value);
    }

    public bool AllowInlineEdit
    {
        get => GetValue(AllowInlineEditProperty);
        set => SetValue(AllowInlineEditProperty, value);
    }

    public ICheckedListEditableAdapter? EditableAdapter
    {
        get => GetValue(EditableAdapterProperty);
        set => SetValue(EditableAdapterProperty, value);
    }

    public ICheckedListRowActionProvider? RowActionProvider
    {
        get => GetValue(RowActionProviderProperty);
        set => SetValue(RowActionProviderProperty, value);
    }

    /// <summary>Optional comparer applied to each sibling group when flattening (OCP: inject ordering).</summary>
    public IComparer<object?>? ItemComparer
    {
        get => _itemComparer;
        set
        {
            if (ReferenceEquals(_itemComparer, value))
                return;
            _itemComparer = value;
            RebuildAll();
        }
    }

    public IReadOnlyList<CheckedListRowModel> Rows => _rows;

    public event EventHandler<CheckedListBoxCheckedChangedEventArgs>? CheckedChanged;

    public event EventHandler<CheckedListBoxSelectionChangedEventArgs>? SelectionChanged;

    public event EventHandler<CheckedListReorderEventArgs>? ReorderRequested;

    public event EventHandler<CheckedListEditStartingEventArgs>? EditStarting;

    public event EventHandler<CheckedListEditCommittedEventArgs>? EditCommitted;

    public event EventHandler<CheckedListEditCancelledEventArgs>? EditCancelled;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scroll = e.NameScope.Find<ScrollViewer>(PartScrollViewer);
        _itemsHost = e.NameScope.Find<ItemsControl>(PartItemsHost);
        reorderOverlay = e.NameScope.Find<Panel>(PartReorderOverlay);
        if (_itemsHost != null)
        {
            _itemsHost.ItemsSource = _rows;
            _itemsHost.ItemsPanel = new FuncTemplate<Panel?>(() => new VirtualizingStackPanel());
            ApplyItemTemplate();
        }

        RebuildAll();
    }

    private void OnItemsSourceChanged()
    {
        if (_rootNotify != null && _rootCollectionHandler != null)
            _rootNotify.CollectionChanged -= _rootCollectionHandler;
        _rootNotify = ItemsSource as INotifyCollectionChanged;
        _rootCollectionHandler = OnRootCollectionChanged;
        if (_rootNotify != null)
            _rootNotify.CollectionChanged += _rootCollectionHandler;
        RebuildAll();
    }

    private void OnRootCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RequestStructureRebuild();

    private void ApplyItemTemplate()
    {
        if (_itemsHost == null)
            return;
        _itemsHost.ItemTemplate = new FuncDataTemplate<CheckedListRowModel?>((m, _) => CheckedListRowBuilder.Build(this, m), supportsRecycling: false);
    }

    internal void HandleRowPointerPressed(CheckedListRowModel row, PointerReleasedEventArgs e) =>
        SelectRow(row, e.KeyModifiers);

    internal void HandleRowPointerPressed(CheckedListRowModel row, PointerPressedEventArgs e) =>
        SelectRow(row, e.KeyModifiers);

    private void SelectRow(CheckedListRowModel row, KeyModifiers keyModifiers)
    {
        if (SelectionMode == CheckedListBoxSelectionMode.None)
            return;

        if (SelectionMode == CheckedListBoxSelectionMode.Single)
        {
            foreach (var r in _rows)
                r.IsSelected = ReferenceEquals(r, row);
        }
        else if (SelectionMode == CheckedListBoxSelectionMode.Multiple)
        {
            if ((keyModifiers & KeyModifiers.Control) != 0)
                row.IsSelected = !row.IsSelected;
            else
            {
                foreach (var r in _rows)
                    r.IsSelected = ReferenceEquals(r, row);
            }
        }

        SelectionChanged?.Invoke(this, new CheckedListBoxSelectionChangedEventArgs(row.Item, row.IsSelected));
    }

    private void OnRowCheckCommitted(CheckedListRowModel row, bool? value)
    {
        var adapter = ItemAdapter ?? new DefaultCheckedListItemAdapter();
        CheckedListCheckCoordinator.ApplyAfterItemChanged(
            row.Item,
            value,
            adapter,
            _parents,
            CascadeToChildren,
            UseThreeStateForParents);
        RefreshRowCheckBindings();
        CheckedChanged?.Invoke(this, new CheckedListBoxCheckedChangedEventArgs(row.Item, value));
    }

    private void RefreshRowCheckBindings()
    {
        foreach (var r in _rows)
            r.NotifyCheckFromAdapter();
    }

    internal void RequestStructureRebuild()
    {
        if (_rebuildScheduled)
            return;
        _rebuildScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _rebuildScheduled = false;
            RebuildAll();
        }, DispatcherPriority.Background);
    }

    public void RebuildAll()
    {
        UnsubscribeItemTree();
        _rows.Clear();
        var adapter = ItemAdapter ?? new DefaultCheckedListItemAdapter();
        CheckedListFlatIndex.RebuildParents(ItemsSource, adapter, _parents);
        CheckedListFlatIndex.AppendVisibleRows(
            _rows,
            ItemsSource,
            adapter,
            _itemComparer,
            RequestStructureRebuild,
            OnRowCheckCommitted,
            static (_, _) => { },
            OnRowExpandRequested,
            item => item is not null && loadingItems.Contains(item));
        SubscribeItemTree(ItemsSource, adapter);
    }

    private void OnRowExpandRequested(CheckedListRowModel row) =>
        _ = EnsureChildrenLoadedAsync(row.Item);

    private async Task EnsureChildrenLoadedAsync(object? item)
    {
        if (item is null || ItemAdapter is not AsyncCheckedListItemAdapter asyncAdapter)
            return;

        var source = asyncAdapter.AsyncSource;
        if (!source.HasChildren(item) || source.AreChildrenLoaded(item) || loadingItems.Contains(item))
            return;

        loadingItems.Add(item);
        RequestStructureRebuild();

        try
        {
            var children = await source.LoadChildrenAsync(item).ConfigureAwait(true);
            source.ApplyLoadedChildren(item, children);
        }
        finally
        {
            loadingItems.Remove(item);
            RequestStructureRebuild();
        }
    }

    internal void RaiseReorderRequested(CheckedListReorderEventArgs args)
    {
        object? parent = null;
        if (args.SourceItem is not null)
            _parents.TryGetValue(args.SourceItem, out parent);

        ReorderRequested?.Invoke(this, new CheckedListReorderEventArgs(
            args.SourceItem,
            args.TargetItem,
            args.Position,
            parent));

        RebuildAll();
    }

    public void BeginEditForItem(object? item)
    {
        foreach (var row in _rows)
        {
            if (ReferenceEquals(row.Item, item))
            {
                BeginInlineEdit(row);
                break;
            }
        }
    }

    internal Panel GetReorderIndicatorHost() =>
        reorderOverlay ?? throw new InvalidOperationException("CheckedListBox template is not applied.");

    internal bool HaveSameParent(object? left, object? right)
    {
        if (left is null || right is null)
            return false;

        if (!_parents.TryGetValue(left, out var leftParent))
            return false;

        if (!_parents.TryGetValue(right, out var rightParent))
            return false;

        return ReferenceEquals(leftParent, rightParent);
    }

    internal void RequestRowVisualRefresh() => ApplyItemTemplate();

    internal IBrush FindRowBrush(string key, IBrush fallback) => FindBrush(key, fallback);

    internal void BeginInlineEdit(CheckedListRowModel row)
    {
        if (!AllowInlineEdit || EditableAdapter is null || row.Item is null)
            return;

        if (!EditableAdapter.CanEdit(row.Item))
            return;

        var text = EditableAdapter.GetEditText(row.Item);
        var starting = new CheckedListEditStartingEventArgs(row.Item, text);
        EditStarting?.Invoke(this, starting);
        if (starting.Cancel)
            return;

        foreach (var other in _rows)
            other.IsEditing = false;

        row.EditText = starting.Text;
        row.IsEditing = true;
    }

    internal void CommitInlineEdit(CheckedListRowModel row, string text)
    {
        if (!row.IsEditing || EditableAdapter is null || row.Item is null)
            return;

        if (!EditableAdapter.TryCommitEdit(row.Item, text, out _))
        {
            CancelInlineEdit(row);
            return;
        }

        row.IsEditing = false;
        EditCommitted?.Invoke(this, new CheckedListEditCommittedEventArgs(row.Item, text));
    }

    internal void CancelInlineEdit(CheckedListRowModel row)
    {
        if (!row.IsEditing || row.Item is null)
            return;

        var original = EditableAdapter?.GetEditText(row.Item) ?? row.EditText;
        row.IsEditing = false;
        EditCancelled?.Invoke(this, new CheckedListEditCancelledEventArgs(row.Item, original));
    }

    internal void ShowRowActions(CheckedListRowModel row, Control anchor)
    {
        var actions = RowActionProvider?.GetActions(row.Item);
        if (actions is null || actions.Count == 0)
            return;

        var flyout = new SkyMenuFlyout();
        foreach (var action in actions)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = action.Label,
                Command = action.Command,
                CommandParameter = action.CommandParameter ?? row.Item,
            });
        }

        flyout.ShowAt(anchor);
    }

    private void SubscribeItemTree(IEnumerable? roots, ICheckedListItemAdapter adapter)
    {
        SubscribeCollection(roots);
        if (roots == null)
            return;

        foreach (var r in roots)
        {
            if (r is null)
                continue;
            SubscribeNode(r, adapter);
        }
    }

    private void SubscribeNode(object node, ICheckedListItemAdapter adapter)
    {
        if (node is INotifyPropertyChanged n && !_itemHandlers.ContainsKey(node))
        {
            void Handler(object? s, global::System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName is nameof(ICheckedListBoxItem.IsExpanded) or nameof(ICheckedListBoxItem.Children))
                    RequestStructureRebuild();
                else if (e.PropertyName is nameof(ICheckedListBoxItem.IsChecked))
                    RefreshRowCheckBindings();
            }

            n.PropertyChanged += Handler;
            _itemHandlers[node] = Handler;
        }

        SubscribeCollection(ResolveChildCollection(node));
        foreach (var c in adapter.GetChildren(node))
        {
            if (c is null)
                continue;
            SubscribeNode(c, adapter);
        }
    }

    private static IEnumerable? ResolveChildCollection(object node) =>
        node is ICheckedListBoxItem item ? item.Children : null;

    private void SubscribeCollection(IEnumerable? items)
    {
        if (items is not INotifyCollectionChanged collection)
            return;

        if (!subscribedCollections.Add(collection))
            return;

        collection.CollectionChanged += OnSubscribedCollectionChanged;
    }

    private void OnSubscribedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
            RebuildAll();
        else
            RequestStructureRebuild();
    }

    private void UnsubscribeItemTree()
    {
        foreach (var kv in _itemHandlers)
        {
            if (kv.Key is INotifyPropertyChanged n)
                n.PropertyChanged -= kv.Value;
        }

        _itemHandlers.Clear();

        foreach (var collection in subscribedCollections)
            collection.CollectionChanged -= OnSubscribedCollectionChanged;

        subscribedCollections.Clear();
    }

    private IBrush FindBrush(string key, IBrush fallback) =>
        TryGetResource(key, ActualThemeVariant, out var o) && o is IBrush b ? b : fallback;
}

public sealed class CheckedListBoxCheckedChangedEventArgs : EventArgs
{
    public CheckedListBoxCheckedChangedEventArgs(object? item, bool? newValue)
    {
        Item = item;
        NewValue = newValue;
    }

    public object? Item { get; }

    public bool? NewValue { get; }
}

public sealed class CheckedListBoxSelectionChangedEventArgs : EventArgs
{
    public CheckedListBoxSelectionChangedEventArgs(object? item, bool isSelected)
    {
        Item = item;
        IsSelected = isSelected;
    }

    public object? Item { get; }

    public bool IsSelected { get; }
}
