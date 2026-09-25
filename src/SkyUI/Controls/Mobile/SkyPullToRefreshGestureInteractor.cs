using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Pointer math for pull-to-refresh gestures on a <see cref="ScrollViewer"/>.</summary>
internal sealed class SkyPullToRefreshGestureInteractor : IDisposable
{
    private const double TopEpsilon = 0.5;

    private readonly ScrollViewer scrollViewer;
    private readonly Func<bool> canRefresh;
    private readonly Action<double> onPullOffsetChanged;
    private readonly Func<bool> onRefreshTriggered;

    private bool tracking;
    private bool pullActive;
    private double startY;
    private double pullOffset;

    public SkyPullToRefreshGestureInteractor(
        ScrollViewer scrollViewer,
        Func<bool> canRefresh,
        Action<double> onPullOffsetChanged,
        Func<bool> onRefreshTriggered)
    {
        this.scrollViewer = scrollViewer;
        this.canRefresh = canRefresh;
        this.onPullOffsetChanged = onPullOffsetChanged;
        this.onRefreshTriggered = onRefreshTriggered;
    }

    public double PullOffset => pullOffset;

    private const RoutingStrategies PointerRoutes = RoutingStrategies.Tunnel | RoutingStrategies.Bubble;

    public void Attach()
    {
        scrollViewer.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, PointerRoutes, handledEventsToo: true);
        scrollViewer.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, PointerRoutes, handledEventsToo: true);
        scrollViewer.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, PointerRoutes, handledEventsToo: true);
        scrollViewer.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, PointerRoutes, handledEventsToo: true);
    }

    public void Detach()
    {
        scrollViewer.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        scrollViewer.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        scrollViewer.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        scrollViewer.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        ResetPull(animate: false);
    }

    public void Dispose() => Detach();

    public void ResetPull(bool animate) => SetPullOffset(0, animate);

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!canRefresh() || tracking || !IsAtTop())
            return;

        if (e.GetCurrentPoint(scrollViewer).Properties.IsRightButtonPressed)
            return;

        tracking = true;
        pullActive = false;
        startY = e.GetPosition(scrollViewer).Y;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!tracking)
            return;

        var delta = e.GetPosition(scrollViewer).Y - startY;
        if (!pullActive)
        {
            if (!IsAtTop() || delta <= 0)
            {
                tracking = false;
                return;
            }

            if (delta < 4)
                return;

            pullActive = true;
            e.Pointer.Capture(scrollViewer);
        }

        SetPullOffset(Math.Max(0, delta), animate: false);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!tracking)
            return;

        var startedRefresh = false;
        try
        {
            if (pullActive && canRefresh())
                startedRefresh = onRefreshTriggered();
        }
        finally
        {
            tracking = false;
            pullActive = false;
            e.Pointer.Capture(null);
            if (!startedRefresh)
                SetPullOffset(0, animate: true);
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!tracking)
            return;

        tracking = false;
        pullActive = false;
        SetPullOffset(0, animate: true);
    }

    private bool IsAtTop() => scrollViewer.Offset.Y <= TopEpsilon;

    private void SetPullOffset(double value, bool animate)
    {
        pullOffset = value;
        onPullOffsetChanged(pullOffset);
    }
}
