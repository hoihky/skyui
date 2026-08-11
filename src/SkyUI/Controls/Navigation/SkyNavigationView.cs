using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>
/// App shell with adaptive navigation: expanded sidebar, compact sidebar, or bottom bar.
/// </summary>
[PseudoClasses("compact", "bottom")]
public class SkyNavigationView : TemplatedControl
{
    public const string RootPartName = "PART_Root";
    public const string SideNavListPartName = "PART_SideNavList";
    public const string BottomNavListPartName = "PART_BottomNavList";
    public const string SideNavHostPartName = "PART_SideNavHost";
    public const string BottomNavHostPartName = "PART_BottomNavHost";
    public const string ContentPartName = "PART_Content";

    public static readonly StyledProperty<SkyNavigationDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<SkyNavigationView, SkyNavigationDisplayMode>(
            nameof(DisplayMode),
            SkyNavigationDisplayMode.Auto);

    public static readonly StyledProperty<double> ExpandedSideNavWidthProperty =
        AvaloniaProperty.Register<SkyNavigationView, double>(nameof(ExpandedSideNavWidth), 240);

    public static readonly StyledProperty<double> CompactSideNavWidthProperty =
        AvaloniaProperty.Register<SkyNavigationView, double>(nameof(CompactSideNavWidth), 72);

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SkyNavigationView, object?>(nameof(Header));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SkyNavigationView, object?>(nameof(Content));

    public static readonly DirectProperty<SkyNavigationView, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyNavigationView, IList>(
            nameof(Items),
            o => o.Items);

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SkyNavigationView, int>(nameof(SelectedIndex), -1);

    public static readonly StyledProperty<SkyNavigationViewItem?> SelectedItemProperty =
        AvaloniaProperty.Register<SkyNavigationView, SkyNavigationViewItem?>(nameof(SelectedItem));

    public static readonly RoutedEvent<RoutedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SkyNavigationView, RoutedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    private readonly AvaloniaList<SkyNavigationViewItem> _items = new();
    private Control? _root;
    private ListBox? _sideNavList;
    private ListBox? _bottomNavList;
    private Control? _sideNavHost;
    private Control? _bottomNavHost;
    private bool _syncingSelection;

    static SkyNavigationView()
    {
        DisplayModeProperty.Changed.AddClassHandler<SkyNavigationView>((v, _) => v.ApplyDisplayMode());
        SelectedIndexProperty.Changed.AddClassHandler<SkyNavigationView>((v, e) => v.OnSelectedIndexChanged(e));
        SelectedItemProperty.Changed.AddClassHandler<SkyNavigationView>((v, e) => v.OnSelectedItemChanged(e));
    }

    public SkyNavigationView()
    {
        _items.CollectionChanged += OnItemsCollectionChanged;
    }

    [Content]
    public IList Items => _items;

    public SkyNavigationDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public double ExpandedSideNavWidth
    {
        get => GetValue(ExpandedSideNavWidthProperty);
        set => SetValue(ExpandedSideNavWidthProperty, value);
    }

    public double CompactSideNavWidth
    {
        get => GetValue(CompactSideNavWidthProperty);
        set => SetValue(CompactSideNavWidthProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public SkyNavigationViewItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachNavHandlers(_sideNavList);
        DetachNavHandlers(_bottomNavList);

        _root = e.NameScope.Find(RootPartName) as Control;
        _sideNavList = e.NameScope.Find(SideNavListPartName) as ListBox;
        _bottomNavList = e.NameScope.Find(BottomNavListPartName) as ListBox;
        _sideNavHost = e.NameScope.Find(SideNavHostPartName) as Control;
        _bottomNavHost = e.NameScope.Find(BottomNavHostPartName) as Control;

        AttachNavHandlers(_sideNavList);
        AttachNavHandlers(_bottomNavList);

        SyncNavItemsSource();
        ApplyDisplayMode();
        SyncSelectionToNavLists();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SyncNavItemsSource();
        ApplyDisplayMode();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (DisplayMode == SkyNavigationDisplayMode.Auto)
            ApplyDisplayMode();
        return size;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty && DisplayMode == SkyNavigationDisplayMode.Auto)
            ApplyDisplayMode();
    }

    private void AttachNavHandlers(ListBox? listBox)
    {
        if (listBox is null)
            return;

        listBox.SelectionChanged += OnNavSelectionChanged;
    }

    private void DetachNavHandlers(ListBox? listBox)
    {
        if (listBox is null)
            return;

        listBox.SelectionChanged -= OnNavSelectionChanged;
    }

    internal void RefreshItems() => SyncNavItemsSource();

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SyncNavItemsSource();

    private void SyncNavItemsSource()
    {
        var items = _items.ToList();
        if (_sideNavList is not null)
            _sideNavList.ItemsSource = items;
        if (_bottomNavList is not null)
            _bottomNavList.ItemsSource = items;

        if (SelectedIndex < 0 && items.Count > 0)
            SelectedIndex = 0;
    }

    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection || sender is not ListBox listBox)
            return;

        if (listBox.SelectedItem is SkyNavigationViewItem item)
            SelectedItem = item;
    }

    private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var index = e.GetNewValue<int>();
        var items = _items.ToList();
        if (index >= 0 && index < items.Count)
            SelectedItem = items[index];
        else if (index < 0)
            SelectedItem = null;
    }

    private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var item = e.NewValue as SkyNavigationViewItem;
        var items = _items.ToList();
        var index = item is null ? -1 : items.IndexOf(item);
        if (index != SelectedIndex)
            SetCurrentValue(SelectedIndexProperty, index);

        SetCurrentValue(ContentProperty, item?.Content);
        SyncSelectionToNavLists();
        RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
    }

    private void SyncSelectionToNavLists()
    {
        _syncingSelection = true;
        try
        {
            if (_sideNavList is not null)
                _sideNavList.SelectedItem = SelectedItem;
            if (_bottomNavList is not null)
                _bottomNavList.SelectedItem = SelectedItem;
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void ApplyDisplayMode()
    {
        var mode = DisplayMode == SkyNavigationDisplayMode.Auto
            ? SkyBreakpoint.ResolveNavigationDisplayMode(Bounds.Width)
            : DisplayMode;

        var useBottom = mode == SkyNavigationDisplayMode.Bottom;

        if (_sideNavHost is not null)
        {
            _sideNavHost.IsVisible = !useBottom;
            _sideNavHost.Width = mode == SkyNavigationDisplayMode.Compact
                ? CompactSideNavWidth
                : ExpandedSideNavWidth;
        }

        if (_bottomNavHost is not null)
            _bottomNavHost.IsVisible = useBottom;

        if (_root is Grid root)
        {
            if (useBottom)
            {
                root.ColumnDefinitions[0].Width = new GridLength(0);
                root.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                root.ColumnDefinitions[0].Width = GridLength.Auto;
                root.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            }
        }

        PseudoClasses.Set(":compact", mode == SkyNavigationDisplayMode.Compact);
        PseudoClasses.Set(":bottom", useBottom);
    }
}
