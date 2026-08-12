using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>
/// Uniform grid that updates <see cref="UniformGrid.Columns"/> from control width and
/// <see cref="SkyGridBreakpoint"/>.
/// </summary>
public class SkyResponsiveGrid : UniformGrid
{
    public static readonly StyledProperty<SkyResponsiveColumnProfile?> ColumnProfileProperty =
        AvaloniaProperty.Register<SkyResponsiveGrid, SkyResponsiveColumnProfile?>(nameof(ColumnProfile));

    public SkyResponsiveGrid()
    {
        Classes.Add("sky");
        Classes.Add("sky-responsive-grid");
        ColumnSpacing = 16;
        RowSpacing = 16;
    }

    public SkyResponsiveColumnProfile? ColumnProfile
    {
        get => GetValue(ColumnProfileProperty);
        set => SetValue(ColumnProfileProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateColumns(Bounds.Width);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
            UpdateColumns(Bounds.Width);
        else if (change.Property == ColumnProfileProperty)
            UpdateColumns(Bounds.Width);
    }

    private void UpdateColumns(double width)
    {
        if (width <= 0)
            return;

        var columns = SkyGridBreakpoint.ResolveColumns(width, ColumnProfile);
        if (Columns != columns)
            Columns = columns;
    }
}
