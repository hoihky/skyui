using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public sealed class SkyDockResizeGrip : Border
{
    public static readonly StyledProperty<SkyDockResizeOrientation> OrientationProperty =
        AvaloniaProperty.Register<SkyDockResizeGrip, SkyDockResizeOrientation>(nameof(Orientation));

    public event EventHandler<double>? ResizeDelta;

    private bool isResizing;
    private Point startPoint;

    public SkyDockResizeOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static SkyDockResizeGrip()
    {
        OrientationProperty.Changed.AddClassHandler<SkyDockResizeGrip>((g, _) => g.UpdateResizeCursor());
    }

    public SkyDockResizeGrip()
    {
        Classes.Add("sky-dock-resize-grip");
        Background = Brushes.Transparent;
        UpdateResizeCursor();
    }

    private void UpdateResizeCursor() =>
        Cursor = Orientation == SkyDockResizeOrientation.Vertical
            ? new Cursor(StandardCursorType.SizeNorthSouth)
            : new Cursor(StandardCursorType.SizeWestEast);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        isResizing = true;
        startPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!isResizing)
            return;
        var point = e.GetPosition(this);
        var delta = Orientation == SkyDockResizeOrientation.Horizontal
            ? point.X - startPoint.X
            : point.Y - startPoint.Y;
        ResizeDelta?.Invoke(this, delta);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        EndResize(e.Pointer);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndResize(e.Pointer);
    }

    private void EndResize(IPointer pointer)
    {
        if (!isResizing)
            return;
        isResizing = false;
        pointer.Capture(null);
    }
}

public enum SkyDockResizeOrientation
{
    Horizontal,
    Vertical,
}
