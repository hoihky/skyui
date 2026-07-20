using Avalonia;
using Avalonia.Controls;

namespace SkyUI.DataGrid;

/// <summary>
/// Body host: optional <see cref="PART_BodySpacer"/> for non-virtual physical scroll, or viewport-sized
/// measure when logical virtualization is active on the scroll root.
/// </summary>
public sealed class SkyVirtualDataGridScrollHost : Panel
{
    public const string PartBodySpacer = "PART_BodySpacer";

    private Border? _bodySpacer;
    private Control? _rowsHost;
    private bool _usePhysicalSpacer;

    public double VirtualExtentWidth { get; set; }

    public double VirtualExtentHeight { get; set; }

    internal void RegisterTemplateChildren()
    {
        _bodySpacer = null;
        _rowsHost = null;
        if (Children.Count >= 1 && Children[0] is Border spacer)
            _bodySpacer = spacer;
        if (Children.Count >= 2)
            _rowsHost = Children[1];
        else
        {
            foreach (var child in Children)
            {
                if (child is ItemsControl)
                    _rowsHost = child;
            }
        }
    }

    internal void ApplyBodyExtent(double bodyHeight, double width, bool usePhysicalSpacer)
    {
        VirtualExtentHeight = Math.Max(0, bodyHeight);
        VirtualExtentWidth = Math.Max(1, width);
        _usePhysicalSpacer = usePhysicalSpacer;

        if (_bodySpacer == null)
            return;

        if (usePhysicalSpacer)
        {
            _bodySpacer.IsVisible = true;
            _bodySpacer.Width = width;
            _bodySpacer.Height = bodyHeight;
        }
        else
        {
            _bodySpacer.IsVisible = false;
            _bodySpacer.Height = 0;
            _bodySpacer.Width = 0;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var ew = double.IsFinite(VirtualExtentWidth) ? Math.Max(1, VirtualExtentWidth) : 1;

        if (!_usePhysicalSpacer)
        {
            var vh = availableSize.Height;
            if (double.IsPositiveInfinity(vh) || vh <= 0 || double.IsNaN(vh))
                vh = 400;
            _rowsHost?.Measure(new Size(ew, double.PositiveInfinity));
            return new Size(ew, vh);
        }

        var eh = double.IsFinite(VirtualExtentHeight) ? Math.Max(0, VirtualExtentHeight) : 0;
        if (_bodySpacer != null)
        {
            _bodySpacer.Width = ew;
            _bodySpacer.Height = eh;
            _bodySpacer.Measure(new Size(ew, eh));
        }

        _rowsHost?.Measure(new Size(ew, double.PositiveInfinity));
        return new Size(ew, eh);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var ew = Math.Max(finalSize.Width, VirtualExtentWidth);

        if (!_usePhysicalSpacer)
        {
            var eh = finalSize.Height;
            _rowsHost?.Arrange(new Rect(0, 0, ew, eh));
            return new Size(ew, eh);
        }

        var bodyH = Math.Max(finalSize.Height, VirtualExtentHeight);
        if (_bodySpacer != null)
            _bodySpacer.Arrange(new Rect(0, 0, ew, bodyH));

        if (_rowsHost != null)
        {
            var m = _rowsHost.Margin;
            _rowsHost.Arrange(new Rect(m.Left, m.Top, _rowsHost.DesiredSize.Width, _rowsHost.DesiredSize.Height));
        }

        return new Size(ew, bodyH);
    }
}
