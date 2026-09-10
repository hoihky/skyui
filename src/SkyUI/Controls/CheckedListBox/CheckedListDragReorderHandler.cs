using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

internal sealed class CheckedListDragReorderHandler
{
    private const double DragThreshold = 8;

    private readonly CheckedListBox owner;
    private Border? dragSourceBorder;
    private CheckedListRowModel? dragSource;
    private Point pressPoint;
    private bool dragActive;
    private Border? dragIndicator;

    public CheckedListDragReorderHandler(CheckedListBox owner)
    {
        this.owner = owner;
    }

    public void Attach(Border rowBorder, CheckedListRowModel row)
    {
        rowBorder.Tag = row;
        rowBorder.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!owner.AllowReorder || sender is not Border border || border.Tag is not CheckedListRowModel row)
            return;

        if (e.GetCurrentPoint(border).Properties.IsRightButtonPressed)
            return;

        if (CheckedListRowInput.IsInteractiveSource(e.Source, border))
            return;

        dragSource = row;
        dragSourceBorder = border;
        pressPoint = e.GetPosition(owner);
        dragActive = false;

        owner.AddHandler(InputElement.PointerMovedEvent, OnOwnerPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        owner.AddHandler(InputElement.PointerReleasedEvent, OnOwnerPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        e.Pointer.Capture(border);
    }

    private void OnOwnerPointerMoved(object? sender, PointerEventArgs e)
    {
        if (dragSource is null || dragSourceBorder is null)
            return;

        if (!ReferenceEquals(e.Pointer.Captured, dragSourceBorder))
            return;

        var delta = e.GetPosition(owner) - pressPoint;
        if (!dragActive && Math.Abs(delta.Y) >= DragThreshold)
            dragActive = true;

        if (!dragActive)
            return;

        var pointer = e.GetPosition(owner);
        var (targetRow, targetBorder) = HitTestRow(pointer);
        ShowIndicator(targetRow, targetBorder, pointer);
        e.Handled = true;
    }

    private void OnOwnerPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (dragSource is null || dragSourceBorder is null)
                return;

            if (!ReferenceEquals(e.Pointer.Captured, dragSourceBorder))
                return;

            if (dragActive)
            {
                var pointer = e.GetPosition(owner);
                var (targetRow, targetBorder) = HitTestRow(pointer);
                if (targetRow is not null
                    && !ReferenceEquals(targetRow.Item, dragSource.Item)
                    && owner.HaveSameParent(dragSource.Item, targetRow.Item))
                {
                    var position = ResolveDropPosition(targetBorder, pointer, owner);
                    owner.RaiseReorderRequested(new CheckedListReorderEventArgs(
                        dragSource.Item,
                        targetRow.Item,
                        position));
                }

                e.Handled = true;
            }
            else
            {
                owner.HandleRowPointerPressed(dragSource, e);
            }
        }
        finally
        {
            DetachOwnerHandlers();
            dragSource = null;
            dragSourceBorder = null;
            dragActive = false;
            HideIndicator();
            e.Pointer.Capture(null);
        }
    }

    private void DetachOwnerHandlers()
    {
        owner.RemoveHandler(InputElement.PointerMovedEvent, OnOwnerPointerMoved);
        owner.RemoveHandler(InputElement.PointerReleasedEvent, OnOwnerPointerReleased);
    }

    private (CheckedListRowModel? Row, Border? Border) HitTestRow(Point pointOnOwner)
    {
        var hit = owner.InputHitTest(pointOnOwner);
        if (hit is not Visual visual)
            return (null, null);

        var current = visual;
        while (current is not null)
        {
            if (current is Border { Tag: CheckedListRowModel model } border)
                return (model, border);

            current = current.GetVisualParent();
        }

        return (null, null);
    }

    private static CheckedListDropPosition ResolveDropPosition(Border? targetBorder, Point pointOnOwner, CheckedListBox owner)
    {
        if (targetBorder is null)
            return CheckedListDropPosition.After;

        var local = targetBorder.TranslatePoint(pointOnOwner, owner);
        if (local is not Point point || targetBorder.Bounds.Height <= 0)
            return CheckedListDropPosition.After;

        return point.Y < targetBorder.Bounds.Height / 2
            ? CheckedListDropPosition.Before
            : CheckedListDropPosition.After;
    }

    private void ShowIndicator(CheckedListRowModel? target, Border? targetBorder, Point pointOnOwner)
    {
        if (target is null || targetBorder is null)
        {
            HideIndicator();
            return;
        }

        var topLeft = targetBorder.TranslatePoint(new Point(0, 0), owner);
        if (topLeft is not Point origin)
        {
            HideIndicator();
            return;
        }

        dragIndicator ??= CreateIndicator();
        var host = owner.GetReorderIndicatorHost();
        if (dragIndicator.Parent != host)
        {
            host.Children.Add(dragIndicator);
        }

        var before = ResolveDropPosition(targetBorder, pointOnOwner, owner) == CheckedListDropPosition.Before;
        var top = before ? origin.Y : origin.Y + targetBorder.Bounds.Height - 2;

        dragIndicator.Margin = new Thickness(origin.X, top, 0, 0);
        dragIndicator.Width = Math.Max(0, targetBorder.Bounds.Width);
        dragIndicator.IsVisible = true;
    }

    private void HideIndicator()
    {
        if (dragIndicator is null)
            return;

        dragIndicator.IsVisible = false;
    }

    private static Border CreateIndicator() =>
        new()
        {
            Height = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Background = new SolidColorBrush(Color.Parse("#1ED760")),
            IsHitTestVisible = false,
            IsVisible = false,
        };
}
