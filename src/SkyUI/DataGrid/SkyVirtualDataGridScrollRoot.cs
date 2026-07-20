using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace SkyUI.DataGrid;

/// <summary>
/// ScrollViewer content: header + virtual body. With row virtualization, implements
/// <see cref="ILogicalScrollable"/> so extent is <c>header + RowCount×RowHeight</c> while the
/// control stays viewport-sized (physical tall stacks get capped ~pool×row height on macOS).
/// </summary>
public sealed class SkyVirtualDataGridScrollRoot : StackPanel, ILogicalScrollable
{
    private SkyVirtualDataGrid? _owner;
    private Vector _offset;
    private Size _logicalExtent;

    public SkyVirtualDataGridScrollRoot()
    {
        Orientation = Orientation.Vertical;
        CanHorizontallyScroll = true;
        CanVerticallyScroll = true;
    }

    internal void Attach(SkyVirtualDataGrid owner) => _owner = owner;

    internal void SetLogicalExtent(Size extent)
    {
        if (_logicalExtent == extent)
            return;
        _logicalExtent = extent;
        if (IsLogicalScrollEnabled)
            ScrollInvalidated?.Invoke(this, EventArgs.Empty);
    }

    internal void NotifyScrollMetricsChanged() => ScrollInvalidated?.Invoke(this, EventArgs.Empty);

    public bool IsLogicalScrollEnabled =>
        _owner?.EnableRowVirtualization == true && _owner.DataSource != null;

    public Size Extent => IsLogicalScrollEnabled ? _logicalExtent : Bounds.Size;

    public Size Viewport
    {
        get
        {
            if (!IsLogicalScrollEnabled)
                return Bounds.Size;

            var vp = _owner!.GetScrollViewportSize();
            // Never report a viewport as large as the logical extent (breaks ScrollBarMaximum).
            if (_logicalExtent.Height > 1 && vp.Height >= _logicalExtent.Height * 0.5)
            {
                var fallback = _owner.GetBodyViewportHeight();
                vp = new Size(Math.Max(1, vp.Width), Math.Max(32, fallback));
            }

            return vp;
        }
    }

    public Vector Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public bool CanHorizontallyScroll { get; set; }

    public bool CanVerticallyScroll { get; set; }

    public Size ScrollSize
    {
        get
        {
            var rh = Math.Max(8, _owner?.RowHeight ?? 28);
            return new Size(16, rh);
        }
    }

    public Size PageScrollSize => Viewport;

    public event EventHandler? ScrollInvalidated;

    public void RaiseScrollInvalidated(EventArgs e) => ScrollInvalidated?.Invoke(this, e);

    public bool BringIntoView(Control target, Rect targetRect) => false;

    public new Control? GetControlInDirection(NavigationDirection direction, Control? from) => null;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        this.FindAncestorOfType<SkyVirtualDataGrid>()?.AttachScrollRoot(this);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (!IsLogicalScrollEnabled)
            return MeasurePhysical(availableSize);

        var width = ResolveWidth(availableSize);
        var viewportH = ResolveViewportHeight(availableSize);
        var constraint = new Size(width, double.PositiveInfinity);

        double natural = 0;
        foreach (var child in Children)
        {
            child.Measure(constraint);
            natural += child.DesiredSize.Height;
        }

        // Logical scroll: physical size is the viewport, not RowCount×RowHeight.
        return new Size(width, Math.Min(viewportH, natural > 0 ? natural : viewportH));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!IsLogicalScrollEnabled)
            return ArrangePhysical(finalSize);

        var width = finalSize.Width;
        double y = 0;
        foreach (var child in Children)
        {
            var h = child.DesiredSize.Height;
            child.Arrange(new Rect(0, y, width, h));
            y += h;
        }

        return finalSize;
    }

    private Size MeasurePhysical(Size availableSize)
    {
        var width = double.IsPositiveInfinity(availableSize.Width) || availableSize.Width <= 0
            ? 0
            : availableSize.Width;

        var constraint = new Size(
            width > 0 ? width : double.PositiveInfinity,
            double.PositiveInfinity);

        double height = 0;
        double maxWidth = 0;
        foreach (var child in Children)
        {
            child.Measure(constraint);
            height += child.DesiredSize.Height;
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
        }

        if (width <= 0)
            width = maxWidth;

        return new Size(width, height);
    }

    private Size ArrangePhysical(Size finalSize)
    {
        var width = finalSize.Width;
        double height = 0;
        foreach (var child in Children)
        {
            var ch = child.DesiredSize.Height;
            child.Arrange(new Rect(0, height, width, ch));
            height += ch;
        }

        return new Size(width, height);
    }

    private double ResolveWidth(Size availableSize)
    {
        if (!double.IsPositiveInfinity(availableSize.Width) && availableSize.Width > 0)
            return availableSize.Width;
        if (_logicalExtent.Width > 0)
            return _logicalExtent.Width;
        return Math.Max(1, Bounds.Width);
    }

    private double ResolveViewportHeight(Size availableSize)
    {
        if (!double.IsPositiveInfinity(availableSize.Height) && availableSize.Height > 0)
            return availableSize.Height;
        return _owner?.GetScrollViewportSize().Height ?? 400;
    }
}
