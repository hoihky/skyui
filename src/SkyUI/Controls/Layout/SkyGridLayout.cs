using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Attached properties that apply responsive column layouts to <see cref="Grid"/>.</summary>
public static class SkyGridLayout
{
    public static readonly AttachedProperty<bool> IsResponsiveProperty =
        AvaloniaProperty.RegisterAttached<Grid, bool>("IsResponsive", typeof(SkyGridLayout));

    public static readonly AttachedProperty<SkyResponsiveColumnProfile?> ColumnProfileProperty =
        AvaloniaProperty.RegisterAttached<Grid, SkyResponsiveColumnProfile?>("ColumnProfile", typeof(SkyGridLayout));

    public static readonly AttachedProperty<bool> AutoPlaceChildrenProperty =
        AvaloniaProperty.RegisterAttached<Grid, bool>("AutoPlaceChildren", typeof(SkyGridLayout), defaultValue: true);

    static SkyGridLayout()
    {
        IsResponsiveProperty.Changed.AddClassHandler<Grid>(OnIsResponsiveChanged);
        ColumnProfileProperty.Changed.AddClassHandler<Grid>(OnProfileChanged);
    }

    public static bool GetIsResponsive(Grid grid) => grid.GetValue(IsResponsiveProperty);

    public static void SetIsResponsive(Grid grid, bool value) => grid.SetValue(IsResponsiveProperty, value);

    public static SkyResponsiveColumnProfile? GetColumnProfile(Grid grid) => grid.GetValue(ColumnProfileProperty);

    public static void SetColumnProfile(Grid grid, SkyResponsiveColumnProfile? value) =>
        grid.SetValue(ColumnProfileProperty, value);

    public static bool GetAutoPlaceChildren(Grid grid) => grid.GetValue(AutoPlaceChildrenProperty);

    public static void SetAutoPlaceChildren(Grid grid, bool value) => grid.SetValue(AutoPlaceChildrenProperty, value);

    private static void OnIsResponsiveChanged(Grid grid, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            grid.SizeChanged += OnGridSizeChanged;
            ApplyLayout(grid, grid.Bounds.Width);
        }
        else
        {
            grid.SizeChanged -= OnGridSizeChanged;
        }
    }

    private static void OnProfileChanged(Grid grid, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetIsResponsive(grid))
            ApplyLayout(grid, grid.Bounds.Width);
    }

    private static void OnGridSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is Grid grid)
            ApplyLayout(grid, e.NewSize.Width);
    }

    internal static void ApplyLayout(Grid grid, double width)
    {
        if (width <= 0)
            return;

        var columns = SkyGridBreakpoint.ResolveColumns(width, GetColumnProfile(grid));
        grid.ColumnDefinitions = CreateStarColumns(columns);

        if (!GetAutoPlaceChildren(grid))
            return;

        var rows = Math.Max(1, (grid.Children.Count + columns - 1) / columns);
        grid.RowDefinitions = CreateStarRows(rows);

        for (var i = 0; i < grid.Children.Count; i++)
        {
            Grid.SetRow(grid.Children[i], i / columns);
            Grid.SetColumn(grid.Children[i], i % columns);
        }
    }

    private static ColumnDefinitions CreateStarColumns(int count)
    {
        var defs = new ColumnDefinitions();
        for (var i = 0; i < count; i++)
            defs.Add(new ColumnDefinition(1, GridUnitType.Star));
        return defs;
    }

    private static RowDefinitions CreateStarRows(int count)
    {
        var defs = new RowDefinitions();
        for (var i = 0; i < count; i++)
            defs.Add(new RowDefinition(GridLength.Auto));
        return defs;
    }
}
