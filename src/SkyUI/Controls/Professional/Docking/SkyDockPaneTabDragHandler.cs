using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls.Professional;

internal sealed class SkyDockPaneTabDragHandler
{
    private const double DragThreshold = 8;

    private readonly SkyDockPanel owner;
    private SkyDockPane? dragPane;
    private Point pressPoint;
    private bool dragActive;

    public SkyDockPaneTabDragHandler(SkyDockPanel owner) => this.owner = owner;

    public void Attach(Control tabRoot, SkyDockPane pane)
    {
        tabRoot.Tag = pane;
        tabRoot.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (owner.Host is null || !owner.Host.AllowDockDrag || sender is not Control control)
            return;
        if (control.Tag is not SkyDockPane pane)
            return;
        if (e.Source is Control source && source.Classes.Contains("sky-dock-tab-close"))
            return;

        dragPane = pane;
        pressPoint = e.GetPosition(owner.Host);
        dragActive = false;
        e.Pointer.Capture(control);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (dragPane is null || owner.Host is null)
            return;

        var point = e.GetPosition(owner.Host);
        if (!dragActive)
        {
            if (Math.Abs(point.X - pressPoint.X) < DragThreshold && Math.Abs(point.Y - pressPoint.Y) < DragThreshold)
                return;
            dragActive = true;
            owner.Host.BeginDockDrag(dragPane);
            e.Pointer.Capture(owner.Host);
        }

        owner.Host.UpdateDockDrag(point);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (dragPane is null)
            return;

        if (!dragActive && owner.Host is not null)
            owner.SelectPane(dragPane);

        Reset();
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (dragPane is null)
            return;
        if (dragActive && owner.Host is not null && ReferenceEquals(e.Pointer.Captured, owner.Host))
        {
            Reset();
            return;
        }

        if (dragActive && owner.Host is not null)
            owner.Host.CancelDockDrag();
        Reset();
    }

    private void Reset()
    {
        dragPane = null;
        dragActive = false;
    }
}
