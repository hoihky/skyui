using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public class SkyDualRangeSlider : TemplatedControl
{
    public const string TrackPartName = "PART_Track";
    public const string FillPartName = "PART_Fill";
    public const string StartThumbPartName = "PART_StartThumb";
    public const string EndThumbPartName = "PART_EndThumb";

    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<SkyDualRangeSlider>();

    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<SkyDualRangeSlider>();

    public static readonly StyledProperty<double> RangeStartProperty =
        AvaloniaProperty.Register<SkyDualRangeSlider, double>(nameof(RangeStart), 20);

    public static readonly StyledProperty<double> RangeEndProperty =
        AvaloniaProperty.Register<SkyDualRangeSlider, double>(nameof(RangeEnd), 80);

    private Grid? track;
    private Border? fill;
    private Border? startThumb;
    private Border? endThumb;
    private bool draggingStart;
    private bool syncing;

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    public double RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    static SkyDualRangeSlider()
    {
        MinimumProperty.OverrideDefaultValue<SkyDualRangeSlider>(0);
        MaximumProperty.OverrideDefaultValue<SkyDualRangeSlider>(100);
        RangeStartProperty.Changed.AddClassHandler<SkyDualRangeSlider>((s, _) => s.LayoutThumbs());
        RangeEndProperty.Changed.AddClassHandler<SkyDualRangeSlider>((s, _) => s.LayoutThumbs());
        MinimumProperty.Changed.AddClassHandler<SkyDualRangeSlider>((s, _) => s.LayoutThumbs());
        MaximumProperty.Changed.AddClassHandler<SkyDualRangeSlider>((s, _) => s.LayoutThumbs());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        track = e.NameScope.Find<Grid>(TrackPartName);
        fill = e.NameScope.Find<Border>(FillPartName);
        startThumb = e.NameScope.Find<Border>(StartThumbPartName);
        endThumb = e.NameScope.Find<Border>(EndThumbPartName);
        if (startThumb is not null)
        {
            startThumb.PointerPressed += (_, ev) => BeginDrag(true, ev);
            startThumb.PointerMoved += OnPointerMoved;
            startThumb.PointerReleased += EndDrag;
        }

        if (endThumb is not null)
        {
            endThumb.PointerPressed += (_, ev) => BeginDrag(false, ev);
            endThumb.PointerMoved += OnPointerMoved;
            endThumb.PointerReleased += EndDrag;
        }

        if (track is not null)
            track.PointerPressed += OnTrackPressed;
        LayoutThumbs();
    }

    private void BeginDrag(bool start, PointerPressedEventArgs e)
    {
        draggingStart = start;
        e.Pointer.Capture(start ? startThumb : endThumb);
        e.Handled = true;
    }

    private void EndDrag(object? sender, PointerReleasedEventArgs e)
    {
        draggingStart = false;
        e.Pointer.Capture(null);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (track is null || e.Pointer.Captured is not Border)
            return;
        SetValueFromPointer(e.GetPosition(track), draggingStart);
    }

    private void OnTrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (track is null)
            return;
        var value = ValueFromPointer(e.GetPosition(track));
        var distStart = Math.Abs(value - RangeStart);
        var distEnd = Math.Abs(value - RangeEnd);
        SetValueFromPointer(e.GetPosition(track), distStart <= distEnd);
    }

    private void SetValueFromPointer(Point point, bool moveStart)
    {
        var value = ValueFromPointer(point);
        if (moveStart)
        {
            value = Math.Min(value, RangeEnd);
            RangeStart = value;
        }
        else
        {
            value = Math.Max(value, RangeStart);
            RangeEnd = value;
        }
    }

    private double ValueFromPointer(Point point)
    {
        var width = Math.Max(1, track?.Bounds.Width ?? 1);
        var t = Math.Clamp(point.X / width, 0, 1);
        return Minimum + t * (Maximum - Minimum);
    }

    private void LayoutThumbs()
    {
        if (syncing || track is null || startThumb is null || endThumb is null || fill is null)
            return;
        syncing = true;
        var span = Math.Max(1e-6, Maximum - Minimum);
        var startRatio = (RangeStart - Minimum) / span;
        var endRatio = (RangeEnd - Minimum) / span;
        var width = Math.Max(1, track.Bounds.Width);
        if (width <= 1 && track.IsMeasureValid == false)
        {
            syncing = false;
            return;
        }

        var thumb = startThumb.Width;
        if (double.IsNaN(thumb) || thumb <= 0)
            thumb = 16;
        var startX = startRatio * width - thumb / 2;
        var endX = endRatio * width - thumb / 2;
        startThumb.Margin = new Thickness(Math.Max(0, startX), 0, 0, 0);
        endThumb.Margin = new Thickness(Math.Max(0, endX), 0, 0, 0);
        fill.Margin = new Thickness(startRatio * width, 0, (1 - endRatio) * width, 0);
        syncing = false;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        LayoutThumbs();
        return size;
    }
}
