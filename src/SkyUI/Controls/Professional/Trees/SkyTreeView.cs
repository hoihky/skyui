using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public class SkyTreeView : TemplatedControl
{
    public const string ItemsHostPartName = "PART_ItemsHost";

    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<SkyTreeView, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<SkyTreeNodeItem?> SelectedItemProperty =
        AvaloniaProperty.Register<SkyTreeView, SkyTreeNodeItem?>(nameof(SelectedItem));

    public static readonly StyledProperty<double> IndentSizeProperty =
        AvaloniaProperty.Register<SkyTreeView, double>(nameof(IndentSize), 16);

    public static readonly StyledProperty<double> ExpanderSizeProperty =
        AvaloniaProperty.Register<SkyTreeView, double>(nameof(ExpanderSize), 16);

    private Panel? itemsHost;
    private INotifyCollectionChanged? subscribed;
    private SkyTreeNodeItem? selectedItem;

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public SkyTreeNodeItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public double IndentSize
    {
        get => GetValue(IndentSizeProperty);
        set => SetValue(IndentSizeProperty, value);
    }

    public double ExpanderSize
    {
        get => GetValue(ExpanderSizeProperty);
        set => SetValue(ExpanderSizeProperty, value);
    }

    static SkyTreeView()
    {
        ItemsProperty.Changed.AddClassHandler<SkyTreeView>((t, _) => t.Rebuild());
        SelectedItemProperty.Changed.AddClassHandler<SkyTreeView>((t, e) => t.selectedItem = e.NewValue as SkyTreeNodeItem);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        itemsHost = e.NameScope.Find<Panel>(ItemsHostPartName);
        Rebuild();
    }

    private void Rebuild()
    {
        if (itemsHost is null)
            return;
        Unsubscribe();
        itemsHost.Children.Clear();
        if (Items is null)
            return;
        foreach (var node in Items.OfType<SkyTreeNodeItem>())
            AppendNode(itemsHost, node, 0);
        if (Items is INotifyCollectionChanged notify)
        {
            subscribed = notify;
            subscribed.CollectionChanged += OnItemsChanged;
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Unsubscribe()
    {
        if (subscribed is null)
            return;
        subscribed.CollectionChanged -= OnItemsChanged;
        subscribed = null;
    }

    private void AppendNode(Panel host, SkyTreeNodeItem node, int depth)
    {
        host.Children.Add(CreateRow(node, depth));
        if (!node.IsExpanded)
            return;
        foreach (var child in node.Children)
            AppendNode(host, child, depth + 1);
    }

    private Control CreateRow(SkyTreeNodeItem node, int depth)
    {
        var hasChildren = node.Children.Count > 0;
        var indent = depth * IndentSize;
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(ExpanderSize)),
                new ColumnDefinition(GridLength.Star),
            },
            Height = ExpanderSize + 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var shell = new Border
        {
            Margin = new Thickness(indent, 0, 0, 0),
            CornerRadius = new CornerRadius(4),
            Background = node.IsSelected
                ? new SolidColorBrush(Color.FromArgb(40, 79, 195, 247))
                : Brushes.Transparent,
            Child = row,
        };
        var toggle = CreateExpander(node.IsExpanded, hasChildren);
        toggle.PointerPressed += (_, e) =>
        {
            if (!hasChildren)
                return;
            node.IsExpanded = !node.IsExpanded;
            Rebuild();
            e.Handled = true;
        };
        var label = new TextBlock
        {
            Text = node.Header,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 13,
            Classes = { node.IsSelected ? "sky-section-title" : "sky-body-secondary" },
        };
        Grid.SetColumn(toggle, 0);
        Grid.SetColumn(label, 1);
        row.Children.Add(toggle);
        row.Children.Add(label);
        shell.PointerPressed += (_, e) =>
        {
            if (e.Source is Visual visual && IsDescendantOf(visual, toggle))
                return;
            SelectNode(node);
            e.Handled = true;
        };
        return shell;
    }

    private static bool IsDescendantOf(Visual source, Visual ancestor)
    {
        for (var node = source; node is not null; node = node.Parent as Visual)
        {
            if (ReferenceEquals(node, ancestor))
                return true;
        }

        return false;
    }

    private Border CreateExpander(bool expanded, bool visible)
    {
        var path = new Avalonia.Controls.Shapes.Path
        {
            Width = 8,
            Height = 8,
            Stretch = Stretch.Uniform,
            Fill = new SolidColorBrush(Color.Parse("#9CA3AF")),
            Data = Geometry.Parse(expanded ? "M1.5,3 L4,5.5 L6.5,3 Z" : "M3,1.5 L3,6.5 L5.5,4 Z"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        return new Border
        {
            Width = ExpanderSize,
            Height = ExpanderSize,
            MinWidth = ExpanderSize,
            MinHeight = ExpanderSize,
            MaxWidth = ExpanderSize,
            MaxHeight = ExpanderSize,
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            IsVisible = visible,
            IsHitTestVisible = visible,
            Child = path,
        };
    }

    private void SelectNode(SkyTreeNodeItem node)
    {
        if (selectedItem is not null)
            selectedItem.IsSelected = false;
        node.IsSelected = true;
        selectedItem = node;
        SelectedItem = node;
        Rebuild();
    }
}
