using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using PathShape = Avalonia.Controls.Shapes.Path;

namespace SkyUI.Controls.Professional;

public class SkyPropertyGrid : TemplatedControl
{
    public const string CategoriesHostPartName = "PART_CategoriesHost";

    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<SkyPropertyGrid, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<IPropertyGridEditorFactory?> EditorFactoryProperty =
        AvaloniaProperty.Register<SkyPropertyGrid, IPropertyGridEditorFactory?>(
            nameof(EditorFactory),
            defaultValue: new DefaultPropertyGridEditorFactory());

    public static readonly StyledProperty<bool> ShowGridLinesProperty =
        AvaloniaProperty.Register<SkyPropertyGrid, bool>(nameof(ShowGridLines), true);

    public static readonly StyledProperty<bool> CategoriesExpandedByDefaultProperty =
        AvaloniaProperty.Register<SkyPropertyGrid, bool>(nameof(CategoriesExpandedByDefault), true);

    private readonly Dictionary<string, bool> categoryExpansion = new(StringComparer.Ordinal);
    private Panel? categoriesHost;
    private INotifyCollectionChanged? subscribed;

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public IPropertyGridEditorFactory? EditorFactory
    {
        get => GetValue(EditorFactoryProperty);
        set => SetValue(EditorFactoryProperty, value);
    }

    public bool ShowGridLines
    {
        get => GetValue(ShowGridLinesProperty);
        set => SetValue(ShowGridLinesProperty, value);
    }

    public bool CategoriesExpandedByDefault
    {
        get => GetValue(CategoriesExpandedByDefaultProperty);
        set => SetValue(CategoriesExpandedByDefaultProperty, value);
    }

    static SkyPropertyGrid()
    {
        ItemsProperty.Changed.AddClassHandler<SkyPropertyGrid>((g, _) => g.Rebuild());
        ShowGridLinesProperty.Changed.AddClassHandler<SkyPropertyGrid>((g, _) => g.Rebuild());
    }

    public void SetCategoryExpanded(string category, bool expanded)
    {
        categoryExpansion[category] = expanded;
        Rebuild();
    }

    public bool IsCategoryExpanded(string category) =>
        categoryExpansion.TryGetValue(category, out var expanded)
            ? expanded
            : CategoriesExpandedByDefault;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        categoriesHost = e.NameScope.Find<Panel>(CategoriesHostPartName);
        Rebuild();
    }

    private void Rebuild()
    {
        if (categoriesHost is null)
            return;

        UnsubscribeItems();
        categoriesHost.Children.Clear();
        var factory = EditorFactory ?? new DefaultPropertyGridEditorFactory();
        var rows = Items?.OfType<PropertyGridItem>().ToList() ?? [];
        var rowIndex = 0;
        foreach (var group in rows.GroupBy(r => r.Category).OrderBy(g => g.Key))
        {
            categoriesHost.Children.Add(CreateCategorySection(factory, group.Key, group.ToList(), ref rowIndex));
        }

        if (Items is INotifyCollectionChanged notify)
        {
            subscribed = notify;
            subscribed.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void UnsubscribeItems()
    {
        if (subscribed is null)
            return;
        subscribed.CollectionChanged -= OnItemsCollectionChanged;
        subscribed = null;
    }

    private Control CreateCategorySection(
        IPropertyGridEditorFactory factory,
        string category,
        List<PropertyGridItem> items,
        ref int rowIndex)
    {
        var expanded = IsCategoryExpanded(category);
        var gridLine = GetGridLineBrush();
        var section = new StackPanel { Spacing = 0 };

        var header = CreateCategoryHeader(category, expanded, gridLine);
        header.PointerPressed += (_, e) =>
        {
            if (e.Source is not Visual visual || !IsCategoryHeaderHit(visual, header))
                return;
            categoryExpansion[category] = !IsCategoryExpanded(category);
            Rebuild();
            e.Handled = true;
        };
        section.Children.Add(header);

        if (expanded && items.Count > 0)
        {
            var body = new StackPanel { Spacing = 0 };
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                body.Children.Add(CreateRow(factory, item, rowIndex % 2 == 1, gridLine, i > 0));
                rowIndex++;
            }

            section.Children.Add(body);
        }
        else if (items.Count > 0)
        {
            rowIndex += items.Count;
        }

        if (!ShowGridLines)
            return new Border { Margin = new Thickness(0, 8, 0, 0), Child = section };

        return new Border
        {
            Margin = new Thickness(0, 8, 0, 0),
            BorderBrush = gridLine,
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = section,
        };
    }

    private static bool IsCategoryHeaderHit(Visual source, Control header)
    {
        for (var node = source; node is not null; node = node.Parent as Visual)
        {
            if (ReferenceEquals(node, header))
                return true;
        }

        return false;
    }

    private static IBrush GetGridLineBrush() =>
        new SolidColorBrush(Color.Parse("#5A5A5A"));

    private IBrush GetCategoryHeaderBackgroundBrush()
    {
        if (IsLightCardSurface())
        {
            if (TryGetResource("SkySurfaceElevatedBrush", ActualThemeVariant, out var elevated) && elevated is IBrush light)
                return light;
            return new SolidColorBrush(Color.Parse("#F8F6FF"));
        }

        if (TryGetResource("SkyHoverTintBrush", ActualThemeVariant, out var hover) && hover is IBrush tint)
            return tint;
        return new SolidColorBrush(Color.FromArgb(28, 255, 255, 255));
    }

    private bool IsLightCardSurface()
    {
        if (!TryGetResource("SkyCardBrush", ActualThemeVariant, out var resource))
            return false;
        if (resource is ISolidColorBrush solid)
            return GetRelativeLuminance(solid.Color) > 0.55;
        if (resource is SolidColorBrush brush)
            return GetRelativeLuminance(brush.Color) > 0.55;
        return false;
    }

    private static double GetRelativeLuminance(Color color)
    {
        static double Channel(byte c) => c / 255d;
        var r = Channel(color.R);
        var g = Channel(color.G);
        var b = Channel(color.B);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static Control CreateCategoryChevron(bool expanded)
    {
        return new PathShape
        {
            Width = 8,
            Height = 8,
            Stretch = Stretch.Uniform,
            Fill = new SolidColorBrush(Color.Parse("#9CA3AF")),
            Data = Geometry.Parse(expanded ? "M1.5,3 L4,5.5 L6.5,3 Z" : "M3,1.5 L3,6.5 L5.5,4 Z"),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    private Control CreateCategoryHeader(string category, bool expanded, IBrush gridLine)
    {
        var chevronHost = new Border
        {
            Width = 14,
            Height = 14,
            Background = Brushes.Transparent,
            Child = CreateCategoryChevron(expanded),
        };
        var title = new TextBlock
        {
            Text = category,
            Classes = { "sky-section-title" },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0),
        };
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            MinHeight = 36,
        };
        Grid.SetColumn(chevronHost, 0);
        Grid.SetColumn(title, 1);
        grid.Children.Add(chevronHost);
        grid.Children.Add(title);
        var line = ShowGridLines ? gridLine : Brushes.Transparent;
        return new Border
        {
            Padding = new Thickness(10, 8),
            Background = GetCategoryHeaderBackgroundBrush(),
            BorderBrush = line,
            BorderThickness = new Thickness(0, 0, 0, ShowGridLines ? 1 : 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = grid,
        };
    }

    private Border CreateRow(
        IPropertyGridEditorFactory factory,
        PropertyGridItem item,
        bool alternate,
        IBrush gridLine,
        bool showTopBorder)
    {
        if (!ShowGridLines)
            gridLine = Brushes.Transparent;
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(150)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
            MinHeight = 36,
        };
        var name = new TextBlock
        {
            Text = item.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 8, 0),
            Classes = { "sky-body-secondary" },
        };
        var nameCell = new Border
        {
            BorderBrush = gridLine,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = name,
        };
        var editor = factory.CreateEditor(item);
        editor.VerticalAlignment = VerticalAlignment.Center;
        editor.Margin = new Thickness(8, 4, 12, 4);
        Grid.SetColumn(nameCell, 0);
        Grid.SetColumn(editor, 1);
        grid.Children.Add(nameCell);
        grid.Children.Add(editor);
        return new Border
        {
            Background = alternate
                ? new SolidColorBrush(Color.FromArgb(18, 255, 255, 255))
                : Brushes.Transparent,
            BorderBrush = gridLine,
            BorderThickness = new Thickness(0, ShowGridLines && showTopBorder ? 1 : 0, 0, 0),
            Child = grid,
        };
    }
}
