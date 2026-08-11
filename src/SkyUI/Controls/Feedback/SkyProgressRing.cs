using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace SkyUI.Controls;

/// <summary>Circular progress indicator (determinate or indeterminate).</summary>
public class SkyProgressRing : TemplatedControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SkyProgressRing, double>(nameof(Value), 0d);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<SkyProgressRing, double>(nameof(Minimum), 0d);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<SkyProgressRing, double>(nameof(Maximum), 100d);

    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<SkyProgressRing, bool>(nameof(IsIndeterminate));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<SkyProgressRing, double>(nameof(StrokeThickness), 3d);

    private DispatcherTimer? _spinTimer;
    private double _spinAngle;

    static SkyProgressRing()
    {
        WidthProperty.OverrideDefaultValue<SkyProgressRing>(40);
        HeightProperty.OverrideDefaultValue<SkyProgressRing>(40);
        MinWidthProperty.OverrideDefaultValue<SkyProgressRing>(24);
        MinHeightProperty.OverrideDefaultValue<SkyProgressRing>(24);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

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

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
            UpdateSpinTimer();
        else if (change.Property is StyledProperty<double> &&
                 change.Property.Name is nameof(Value) or nameof(Minimum) or nameof(Maximum))
            InvalidateVisual();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateSpinTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopSpinTimer();
        base.OnDetachedFromVisualTree(e);
    }

    public sealed override void Render(DrawingContext context)
    {
        var size = Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
            return;

        var stroke = Math.Max(1, StrokeThickness);
        var radius = Math.Max(1, Math.Min(size.Width, size.Height) / 2 - stroke);
        var center = new Point(size.Width / 2, size.Height / 2);
        var rect = new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2);

        var trackBrush = TryGetResource("SkySurfaceElevatedBrush", ActualThemeVariant, out var trackObj) && trackObj is IBrush track
            ? track
            : new SolidColorBrush(Color.Parse("#1f1f1f"));
        var indicatorBrush = Foreground ?? (TryGetResource("SkyAccentBrush", ActualThemeVariant, out var accentObj) && accentObj is IBrush accent
            ? accent
            : Brushes.White);

        context.DrawEllipse(null, new Pen(trackBrush, stroke), rect);

        var pen = new Pen(indicatorBrush, stroke) { LineCap = PenLineCap.Round };
        if (IsIndeterminate)
        {
            DrawArc(context, pen, rect, _spinAngle, 90);
            return;
        }

        var range = Maximum - Minimum;
        var normalized = range <= 0 ? 0 : Math.Clamp((Value - Minimum) / range, 0, 1);
        if (normalized <= 0)
            return;

        DrawArc(context, pen, rect, -90, normalized * 360);
    }

    private static void DrawArc(DrawingContext context, Pen pen, Rect rect, double startAngle, double sweepAngle)
    {
        if (Math.Abs(sweepAngle) < 0.01)
            return;

        var center = rect.Center;
        var radius = rect.Width / 2;
        var start = Polar(center, radius, startAngle);
        var end = Polar(center, radius, startAngle + sweepAngle);

        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(start, false);
            g.ArcTo(end, new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise, true);
            g.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
    }

    private static Point Polar(Point center, double radius, double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }

    private void UpdateSpinTimer()
    {
        if (IsIndeterminate && VisualRoot is not null)
            StartSpinTimer();
        else
            StopSpinTimer();
    }

    private void StartSpinTimer()
    {
        if (_spinTimer is not null)
            return;

        _spinTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        _spinTimer.Tick += (_, _) =>
        {
            _spinAngle = (_spinAngle + 12) % 360;
            InvalidateVisual();
        };
        _spinTimer.Start();
    }

    private void StopSpinTimer()
    {
        if (_spinTimer is null)
            return;

        _spinTimer.Stop();
        _spinTimer = null;
    }
}
