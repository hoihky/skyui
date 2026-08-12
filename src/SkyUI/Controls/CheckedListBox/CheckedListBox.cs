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

    private readonly ObservableCollection<CheckedListRowModel> _rows = new();
    private readonly Dictionary<object, object?> _parents = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, PropertyChangedEventHandler> _itemHandlers = new(ReferenceEqualityComparer.Instance);
    private INotifyCollectionChanged? _rootNotify;
    private NotifyCollectionChangedEventHandler? _rootCollectionHandler;
    private ScrollViewer? _scroll;
    private ItemsControl? _itemsHost;
    private bool _rebuildScheduled;
    private IComparer<object?>? _itemComparer;

    public CheckedListBox()
    {
        ItemAdapter = new DefaultCheckedListItemAdapter();
    }

    static CheckedListBox()
    {
        ItemsSourceProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.OnItemsSourceChanged());
        ItemAdapterProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.RebuildAll());
        ItemTemplateProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        IndentProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
        ShowCheckBoxesProperty.Changed.AddClassHandler<CheckedListBox>((o, _) => o.ApplyItemTemplate());
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scroll = e.NameScope.Find<ScrollViewer>(PartScrollViewer);
        _itemsHost = e.NameScope.Find<ItemsControl>(PartItemsHost);
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
        _itemsHost.ItemTemplate = new FuncDataTemplate<CheckedListRowModel?>((m, _) => BuildRow(this, m), supportsRecycling: false);
    }

    private static Control BuildRow(CheckedListBox owner, CheckedListRowModel? m)
    {
        if (m is null)
            return new Border();

        var border = new Border
        {
            Padding = new Thickness(4, 2),
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(4),
        };
        border.PointerPressed += (_, e) => owner.HandleRowPointerPressed(m, e);

        void SyncSelection()
        {
            border.Background = m.IsSelected
                ? owner.FindBrush(SkyTokenKeys.Brush.SelectedTint, new SolidColorBrush(Color.Parse("#331ED760")))
                : Brushes.Transparent;
        }

        m.PropertyChanged += (_, a) =>
        {
            if (a.PropertyName == nameof(CheckedListRowModel.IsSelected))
                SyncSelection();
        };
        SyncSelection();

        var grid = new Grid
        {
            ColumnDefinitions = owner.ShowCheckBoxes
                ? new ColumnDefinitions("Auto,Auto,Auto,*")
                : new ColumnDefinitions("Auto,Auto,*"),
            MinHeight = 32,
        };

        var indent = new Border { Width = m.Depth * owner.Indent, Background = Brushes.Transparent };
        Grid.SetColumn(indent, 0);
        grid.Children.Add(indent);

        var expand = new Button
        {
            Width = 26,
            Height = 26,
            Margin = new Thickness(0, 0, 4, 0),
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Content = "›",
            FontSize = 14,
            IsVisible = m.HasChildren,
            Command = m.ToggleExpandCommand,
        };
        expand.Classes.Add("sky");
        expand.Classes.Add("sky-subtle");
        expand.Classes.Add("sky-tree-expander");
        Grid.SetColumn(expand, 1);
        grid.Children.Add(expand);

        void SyncExpand()
        {
            expand.RenderTransform = new RotateTransform(m.IsExpanded ? 90 : 0);
        }

        m.PropertyChanged += (_, a) =>
        {
            if (a.PropertyName == nameof(CheckedListRowModel.IsExpanded))
                SyncExpand();
        };
        SyncExpand();

        var contentColumn = 2;
        if (owner.ShowCheckBoxes)
        {
            var cb = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            cb.Classes.Add("sky");
            cb.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(CheckedListRowModel.IsChecked))
            {
                Source = m,
                Mode = BindingMode.TwoWay,
            });
            Grid.SetColumn(cb, 2);
            grid.Children.Add(cb);
            contentColumn = 3;
        }

        var presenter = new ContentPresenter
        {
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        presenter.Bind(ContentPresenter.ContentProperty, new Binding(nameof(CheckedListRowModel.Item)) { Source = m });
        presenter.Bind(ContentPresenter.ContentTemplateProperty, new Binding(nameof(ItemTemplate)) { Source = owner });
        Grid.SetColumn(presenter, contentColumn);
        grid.Children.Add(presenter);

        border.Child = grid;
        return border;
    }

    private void HandleRowPointerPressed(CheckedListRowModel row, PointerPressedEventArgs e)
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
            if ((e.KeyModifiers & KeyModifiers.Control) != 0)
                row.IsSelected = !row.IsSelected;
            else
            {
                foreach (var r in _rows)
                    r.IsSelected = ReferenceEquals(r, row);
            }
        }

        SelectionChanged?.Invoke(this, new CheckedListBoxSelectionChangedEventArgs(row.Item, row.IsSelected));
        e.Handled = true;
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
            (row, e) => HandleRowPointerPressed(row, e));
        SubscribeItemTree(ItemsSource, adapter);
    }

    private void SubscribeItemTree(IEnumerable? roots, ICheckedListItemAdapter adapter)
    {
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

        foreach (var c in adapter.GetChildren(node))
        {
            if (c is null)
                continue;
            SubscribeNode(c, adapter);
        }
    }

    private void UnsubscribeItemTree()
    {
        foreach (var kv in _itemHandlers)
        {
            if (kv.Key is INotifyPropertyChanged n)
                n.PropertyChanged -= kv.Value;
        }

        _itemHandlers.Clear();
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
