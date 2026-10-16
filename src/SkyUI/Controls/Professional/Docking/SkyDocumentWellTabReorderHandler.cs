using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SkyUI.Controls.Professional;

internal sealed class SkyDocumentWellTabReorderHandler
{
    private const double DragThreshold = 6;

    private readonly SkyDocumentWell owner;
    private ISkyDocument? dragDocument;
    private int dragIndex = -1;
    private Point pressPoint;
    private bool dragActive;

    public SkyDocumentWellTabReorderHandler(SkyDocumentWell owner) => this.owner = owner;

    public void AttachTab(Control tabRoot, ISkyDocument document, int index)
    {
        tabRoot.Tag = new TabDragContext(document, index);
        tabRoot.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        tabRoot.AddHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!owner.AllowTabReorder || sender is not Control control)
            return;
        if (control.Tag is not TabDragContext context)
            return;
        if (e.Source is Control source && source.Classes.Contains("sky-document-tab-close"))
            return;

        dragDocument = context.Document;
        dragIndex = context.Index;
        pressPoint = e.GetPosition(owner);
        dragActive = false;
        e.Pointer.Capture(control);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (dragDocument is null || sender is not Control control)
            return;

        var point = e.GetPosition(owner);
        if (!dragActive)
        {
            if (Math.Abs(point.X - pressPoint.X) < DragThreshold && Math.Abs(point.Y - pressPoint.Y) < DragThreshold)
                return;
            dragActive = true;
        }

        if (!IsPointInsideOwner(point))
        {
            var manager = owner.FindAncestorOfType<SkyDockManager>();
            if (manager is not null && manager.AllowDockDrag)
            {
                manager.BeginDocumentShellDrag(dragDocument);
                manager.UpdateDocumentShellDrag(e.GetPosition(manager));
                e.Pointer.Capture(manager);
                Reset();
                e.Handled = true;
                return;
            }
        }

        var targetIndex = owner.HitTestTabIndex(point);
        if (targetIndex >= 0 && targetIndex != dragIndex)
        {
            owner.MoveDocument(dragIndex, targetIndex);
            dragIndex = targetIndex;
        }
    }

    private bool IsPointInsideOwner(Point pointInOwner)
    {
        var width = owner.Bounds.Width;
        var height = owner.Bounds.Height;
        if (width <= 0 || height <= 0)
            return true;
        return pointInOwner.X >= 0 && pointInOwner.Y >= 0 && pointInOwner.X <= width && pointInOwner.Y <= height;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (dragDocument is null)
            return;

        if (!dragActive && sender is Control control && control.Tag is TabDragContext context)
            owner.ActivateDocument(context.Document);

        Reset();
        e.Pointer.Capture(null);
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Reset();

    private void Reset()
    {
        dragDocument = null;
        dragIndex = -1;
        dragActive = false;
    }

    private sealed record TabDragContext(ISkyDocument Document, int Index);
}
