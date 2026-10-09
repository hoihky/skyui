using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public class SkyImageViewer : TemplatedControl
{
    public const string ImagePartName = "PART_Image";
    public const string ScrollPartName = "PART_Scroll";

    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<SkyImageViewer, IImage?>(nameof(Source));

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<SkyImageViewer, double>(nameof(Zoom), 1, coerce: CoerceZoom);

    public static readonly StyledProperty<SkyImageFitMode> FitModeProperty =
        AvaloniaProperty.Register<SkyImageViewer, SkyImageFitMode>(nameof(FitMode));

    private Image? image;
    private ScrollViewer? scroll;
    private Point panOrigin;
    private bool panning;

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public SkyImageFitMode FitMode
    {
        get => GetValue(FitModeProperty);
        set => SetValue(FitModeProperty, value);
    }

    static SkyImageViewer()
    {
        SourceProperty.Changed.AddClassHandler<SkyImageViewer>((v, _) => v.ApplyFit());
        FitModeProperty.Changed.AddClassHandler<SkyImageViewer>((v, _) => v.ApplyFit());
        ZoomProperty.Changed.AddClassHandler<SkyImageViewer>((v, _) => v.ApplyZoom());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        image = e.NameScope.Find<Image>(ImagePartName);
        scroll = e.NameScope.Find<ScrollViewer>(ScrollPartName);
        if (scroll is not null)
        {
            scroll.PointerPressed += OnPanPressed;
            scroll.PointerMoved += OnPanMoved;
            scroll.PointerReleased += OnPanReleased;
            scroll.PointerWheelChanged += OnWheel;
        }

        ApplyFit();
    }

    public void ZoomIn() => Zoom *= 1.15;

    public void ZoomOut() => Zoom /= 1.15;

    public void ResetZoom() => Zoom = 1;

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        Zoom *= e.Delta.Y > 0 ? 1.1 : 0.9;
        e.Handled = true;
    }

    private void OnPanPressed(object? sender, PointerPressedEventArgs e)
    {
        if (scroll is null || e.GetCurrentPoint(scroll).Properties.IsLeftButtonPressed != true)
            return;
        panning = true;
        panOrigin = e.GetPosition(scroll);
        e.Pointer.Capture(scroll);
    }

    private void OnPanMoved(object? sender, PointerEventArgs e)
    {
        if (!panning || scroll is null)
            return;
        var now = e.GetPosition(scroll);
        var delta = now - panOrigin;
        scroll.Offset = new Vector(scroll.Offset.X - delta.X, scroll.Offset.Y - delta.Y);
        panOrigin = now;
    }

    private void OnPanReleased(object? sender, PointerReleasedEventArgs e)
    {
        panning = false;
        e.Pointer.Capture(null);
    }

    private void ApplyFit()
    {
        if (image is null)
            return;
        image.Stretch = FitMode switch
        {
            SkyImageFitMode.Fill => Stretch.UniformToFill,
            SkyImageFitMode.Fit => Stretch.Uniform,
            _ => Stretch.None,
        };
    }

    private void ApplyZoom()
    {
        if (image is null)
            return;
        image.RenderTransform = new ScaleTransform(Zoom, Zoom);
    }

    private static double CoerceZoom(AvaloniaObject sender, double value) =>
        Math.Clamp(value, 0.1, 8);
}
