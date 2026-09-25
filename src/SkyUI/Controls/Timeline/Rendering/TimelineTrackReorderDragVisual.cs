using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SkyUI.Controls.Timeline.Input;

namespace SkyUI.Controls.Timeline.Rendering;

/// <summary>Drag-time visuals for track header reorder (ghost header, drop line, lane highlight).</summary>
internal sealed class TimelineTrackReorderDragVisual
{
    private Border? dropIndicator;
    private Border? draggedHeader;
    private Border? draggedLane;
    private TranslateTransform? dragTransform;
    private double pressYInHeaderStack;
    private IBrush? savedHeaderBackground;
    private IBrush? savedLaneBackground;
    private Cursor? savedHeaderCursor;

    public bool IsActive => draggedHeader is not null;

    public void Begin(TimelineInteractionContext ctx, Border header, string trackId, double headerStackPressY)
    {
        End(ctx);
        if (!ctx.Renderer.HeaderChrome.TryGetValue(trackId, out var chrome) || !ReferenceEquals(chrome, header))
            chrome = header;

        draggedHeader = chrome;
        pressYInHeaderStack = headerStackPressY;
        savedHeaderBackground = chrome.Background;
        savedHeaderCursor = chrome.Cursor;
        savedLaneBackground = null;

        dragTransform = new TranslateTransform(0, 0);
        chrome.RenderTransform = dragTransform;
        chrome.ZIndex = 10;
        chrome.Opacity = 0.92;
        chrome.Cursor = new Cursor(StandardCursorType.SizeAll);
        chrome.BorderBrush = ctx.Control.TrackSelectionBrush ?? ctx.Control.PlayheadBrush ?? Brushes.DodgerBlue;
        chrome.BorderThickness = new Thickness(2);
        chrome.Background = ctx.Control.ClipSelectedBrush ?? savedHeaderBackground;

        if (ctx.Renderer.LaneChrome.TryGetValue(trackId, out var lane))
        {
            draggedLane = lane;
            savedLaneBackground = lane.Background;
            lane.Opacity = 0.55;
            lane.BorderBrush = ctx.Control.TrackSelectionBrush ?? ctx.Control.PlayheadBrush ?? Brushes.DodgerBlue;
            lane.BorderThickness = new Thickness(2);
        }

        EnsureDropIndicator(ctx);
    }

    public void Update(TimelineInteractionContext ctx, int fromIndex, double pointerY)
    {
        if (draggedHeader is null || dragTransform is null)
            return;

        dragTransform.Y = pointerY - pressYInHeaderStack;

        var trackHeight = TimelineRenderMetrics.TrackHeight;
        var insertRow = (int)Math.Floor(pointerY / trackHeight);
        insertRow = Math.Clamp(insertRow, 0, Math.Max(0, ctx.Tracks.Count - 1));

        if (dropIndicator is not null)
            dropIndicator.Margin = new Thickness(0, insertRow * trackHeight, 0, 0);
    }

    public void End(TimelineInteractionContext ctx)
    {
        if (draggedHeader is not null)
        {
            draggedHeader.RenderTransform = null;
            draggedHeader.ZIndex = 0;
            draggedHeader.Opacity = 1;
            draggedHeader.Cursor = savedHeaderCursor;
            draggedHeader.Background = savedHeaderBackground;
            draggedHeader.BorderBrush = Brushes.Transparent;
            draggedHeader.BorderThickness = new Thickness(0, 0, 0, 1);
        }

        if (draggedLane is not null)
        {
            draggedLane.Opacity = 1;
            draggedLane.Background = savedLaneBackground;
        }

        draggedHeader = null;
        draggedLane = null;
        dragTransform = null;
        savedHeaderBackground = null;
        savedLaneBackground = null;
        savedHeaderCursor = null;

        if (dropIndicator is not null)
        {
            if (dropIndicator.Parent is Panel parent)
                parent.Children.Remove(dropIndicator);
            dropIndicator = null;
        }

        ctx.Renderer.ApplyTrackSelectionChrome(ctx);
    }

    private void EnsureDropIndicator(TimelineInteractionContext ctx)
    {
        if (ctx.HeaderStack?.Parent is not Grid headerColumnGrid)
            return;

        var accent = ctx.Control.TrackSelectionBrush ?? ctx.Control.PlayheadBrush ?? Brushes.DodgerBlue;
        dropIndicator = new Border
        {
            Height = 2,
            Background = accent,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            IsHitTestVisible = false,
            ZIndex = 20,
        };
        Grid.SetColumn(dropIndicator, 0);
        headerColumnGrid.Children.Add(dropIndicator);
    }

}
