using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Thumbnails;

namespace SkyUI.Controls.Timeline.Rendering;

/// <summary>Builds and updates ruler, headers, virtualized lanes, clip visuals, and overlays.</summary>
public sealed class TimelineRenderer
{
    private readonly Dictionary<string, Control> clipBorders = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Border> clipBodyById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TimelineClipChromeLayout> clipChromeLayouts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Border> headerChromeByTrackId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Border> laneChromeByTrackId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Polygon> keyframeShapesById = new(StringComparer.Ordinal);

    private Rectangle? selectionRect;
    private Line? playheadLine;
    private int visibleFirstRow;
    private int visibleLastRow = -1;
    private readonly TimelineTrackReorderDragVisual trackReorderDragVisual = new();
    private readonly TimelineTrackHeaderChromeBuilder trackHeaderChromeBuilder = new();
    private readonly TimelineClipLabelFormatter clipLabelFormatter = new();
    private readonly TimelineAccentColorParser accentColorParser = new();
    private readonly HashSet<string> pinnedClipIds = new(StringComparer.Ordinal);

    private EventHandler<PointerPressedEventArgs>? onLanePressed;
    private EventHandler<TappedEventArgs>? onDoubleTapped;
    private Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed;
    private EventHandler<PointerEventArgs>? onTrimMoved;
    private EventHandler<PointerReleasedEventArgs>? onTrimReleased;
    private EventHandler<PointerPressedEventArgs>? onClipPressed;
    private EventHandler<PointerPressedEventArgs>? onKeyframePressed;
    private EventHandler<PointerEventArgs>? onKeyframeMoved;
    private EventHandler<PointerReleasedEventArgs>? onKeyframeReleased;
    private EventHandler<PointerEventArgs>? onClipMoved;
    private EventHandler<PointerReleasedEventArgs>? onClipReleased;

    public IReadOnlyDictionary<string, Border> ClipBodies => clipBodyById;

    public IReadOnlyDictionary<string, Border> LaneChrome => laneChromeByTrackId;

    public IReadOnlyDictionary<string, Border> HeaderChrome => headerChromeByTrackId;

    public void RebuildHeaders(TimelineInteractionContext ctx)
    {
        var stack = ctx.HeaderStack;
        if (stack == null)
            return;

        trackReorderDragVisual.End(ctx);
        headerChromeByTrackId.Clear();
        stack.Children.Clear();
        stack.MinHeight = MeasureTrackContentHeight(ctx);

        foreach (var track in ctx.Tracks)
        {
            var border = trackHeaderChromeBuilder.Build(ctx, track);
            ApplyTrackAccentChrome(track, border);
            stack.Children.Add(border);
            headerChromeByTrackId[track.Id] = border;
        }

        ApplyTrackSelectionChrome(ctx);
    }

    public void RebuildRuler(
        TimelineInteractionContext ctx,
        EventHandler<PointerPressedEventArgs>? onRulerPressed,
        EventHandler<PointerEventArgs>? onRulerMoved,
        EventHandler<PointerReleasedEventArgs>? onRulerReleased,
        EventHandler<PointerPressedEventArgs>? onMarkerPressed,
        EventHandler<PointerEventArgs>? onMarkerMoved,
        EventHandler<PointerReleasedEventArgs>? onMarkerReleased)
    {
        var canvas = ctx.RulerCanvas;
        var scroll = ctx.RulerScroll;
        if (canvas == null || scroll == null)
            return;

        canvas.Children.Clear();
        var width = Math.Max(ctx.Layout.ContentWidth(), scroll.Viewport.Width);
        canvas.Width = width;
        canvas.Height = TimelineRenderMetrics.RulerHeight;

        var tickBrush = ctx.Control.RulerTickBrush ?? new SolidColorBrush(Color.Parse("#555555"));
        var step = ctx.Layout.NiceTickStep(width);
        for (var t = 0.0; t <= ctx.Duration + 1e-6; t += step)
        {
            var x = ctx.Layout.TimeToPixel(t);
            canvas.Children.Add(new Line
            {
                StartPoint = new Point(x, TimelineRenderMetrics.RulerHeight - 10),
                EndPoint = new Point(x, TimelineRenderMetrics.RulerHeight),
                Stroke = tickBrush,
                StrokeThickness = 1,
                IsHitTestVisible = false,
            });
            var label = new TextBlock
            {
                Text = ctx.Layout.FormatTimeLabel(t),
                FontSize = 10,
                Foreground = ctx.Control.Foreground,
            };
            Canvas.SetLeft(label, x + 2);
            Canvas.SetTop(label, 2);
            canvas.Children.Add(label);
        }

        foreach (var marker in ctx.Markers)
        {
            var x = ctx.Layout.TimeToPixel(marker.Time);
            var shape = new Polygon
            {
                Points = new Points
                {
                    new Point(x - 5, TimelineRenderMetrics.RulerHeight - 2),
                    new Point(x + 5, TimelineRenderMetrics.RulerHeight - 2),
                    new Point(x, TimelineRenderMetrics.RulerHeight - 12),
                },
                Fill = ctx.Control.PlayheadBrush ?? Brushes.LimeGreen,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                IsHitTestVisible = true,
                Tag = marker.Id,
            };
            if (onMarkerPressed != null)
                shape.PointerPressed += onMarkerPressed;
            if (onMarkerMoved != null)
                shape.PointerMoved += onMarkerMoved;
            if (onMarkerReleased != null)
                shape.PointerReleased += onMarkerReleased;
            canvas.Children.Add(shape);
        }

        var hit = new Rectangle { Width = width, Height = TimelineRenderMetrics.RulerHeight, Fill = Brushes.Transparent };
        if (onRulerPressed != null)
            hit.PointerPressed += onRulerPressed;
        if (onRulerMoved != null)
            hit.PointerMoved += onRulerMoved;
        if (onRulerReleased != null)
            hit.PointerReleased += onRulerReleased;
        canvas.Children.Insert(0, hit);
    }

    public void RebuildMainCanvas(
        TimelineInteractionContext ctx,
        EventHandler<PointerPressedEventArgs>? onBackgroundPressed,
        EventHandler<PointerEventArgs>? onBackgroundMoved,
        EventHandler<PointerReleasedEventArgs>? onBackgroundReleased,
        EventHandler<TappedEventArgs>? onDoubleTapped,
        EventHandler<PointerPressedEventArgs>? onLanePressed,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed,
        EventHandler<PointerEventArgs>? onTrimMoved,
        EventHandler<PointerReleasedEventArgs>? onTrimReleased,
        EventHandler<PointerPressedEventArgs>? onClipPressed,
        EventHandler<PointerEventArgs>? onClipMoved,
        EventHandler<PointerReleasedEventArgs>? onClipReleased)
    {
        var canvas = ctx.MainCanvas;
        var scroll = ctx.MainScroll;
        if (canvas == null || scroll == null)
            return;

        canvas.Children.Clear();
        clipBorders.Clear();
        clipBodyById.Clear();
        clipChromeLayouts.Clear();
        laneChromeByTrackId.Clear();
        keyframeShapesById.Clear();
        StoreClipGestureHandlers(
            onLanePressed,
            onDoubleTapped,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);

        var contentW = Math.Max(ctx.Layout.ContentWidth(), scroll.Viewport.Width);
        var contentH = MeasureTrackContentHeight(ctx);
        canvas.Width = contentW;
        canvas.Height = contentH;

        var bg = new Rectangle
        {
            Width = contentW,
            Height = contentH,
            Fill = Brushes.Transparent,
            Tag = TimelineRenderMetrics.BackgroundHitTag,
        };
        if (onBackgroundPressed != null)
            bg.PointerPressed += onBackgroundPressed;
        if (onBackgroundMoved != null)
            bg.PointerMoved += onBackgroundMoved;
        if (onBackgroundReleased != null)
            bg.PointerReleased += onBackgroundReleased;
        if (onDoubleTapped != null)
            bg.DoubleTapped += onDoubleTapped;
        canvas.Children.Add(bg);

        UpdateVisibleLanes(
            ctx,
            contentW,
            onLanePressed,
            onDoubleTapped,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);

        selectionRect = new Rectangle
        {
            Fill = ctx.Control.SelectionBrush ?? new SolidColorBrush(Color.FromArgb(80, 0, 255, 136)),
            IsHitTestVisible = false,
            IsVisible = false,
        };
        canvas.Children.Add(selectionRect);

        playheadLine = new Line
        {
            Stroke = ctx.Control.PlayheadBrush ?? Brushes.LimeGreen,
            StrokeThickness = 2,
            IsHitTestVisible = false,
        };
        canvas.Children.Add(playheadLine);
        ApplyTrackSelectionChrome(ctx);
        SyncPropertyTrackKeyframes(ctx);
        UpdateOverlays(ctx);
    }

    public void ConfigureKeyframeGestures(
        EventHandler<PointerPressedEventArgs>? pressed,
        EventHandler<PointerEventArgs>? moved,
        EventHandler<PointerReleasedEventArgs>? released)
    {
        onKeyframePressed = pressed;
        onKeyframeMoved = moved;
        onKeyframeReleased = released;
    }

    public void SyncPropertyTrackKeyframes(TimelineInteractionContext ctx)
    {
        if (ctx.MainCanvas is null)
            return;

        var liveIds = new HashSet<string>(ctx.Project.Keyframes.Select(k => k.Id), StringComparer.Ordinal);
        foreach (var id in keyframeShapesById.Keys.Where(id => !liveIds.Contains(id)).ToList())
            RemoveKeyframeVisual(ctx, id);

        foreach (var keyframe in ctx.Project.Keyframes)
        {
            var row = ctx.TrackRowIndex(keyframe.TrackId);
            if (row < 0)
                continue;
            var track = ctx.Tracks[row];
            if (track.Kind != TimelineTrackKind.Property)
                continue;
            if (row < visibleFirstRow || row > visibleLastRow)
                continue;
            EnsureKeyframeVisual(ctx, keyframe);
            LayoutKeyframe(ctx, keyframe);
        }
    }

    public void LayoutKeyframe(TimelineInteractionContext ctx, TimelineKeyframeItem keyframe)
    {
        if (!keyframeShapesById.TryGetValue(keyframe.Id, out var shape))
            return;
        var row = ctx.TrackRowIndex(keyframe.TrackId);
        if (row < 0)
            return;
        var x = ctx.Layout.TimeToPixel(keyframe.Time);
        var y = row * TimelineRenderMetrics.TrackHeight + TimelineRenderMetrics.TrackHeight / 2;
        shape.Points = new Points
        {
            new Point(x, y - 5),
            new Point(x + 5, y),
            new Point(x, y + 5),
            new Point(x - 5, y),
        };
    }

    public void RefreshVirtualizedLanes(
        TimelineInteractionContext ctx,
        EventHandler<PointerPressedEventArgs>? onLanePressed,
        EventHandler<TappedEventArgs>? onDoubleTapped,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed,
        EventHandler<PointerEventArgs>? onTrimMoved,
        EventHandler<PointerReleasedEventArgs>? onTrimReleased,
        EventHandler<PointerPressedEventArgs>? onClipPressed,
        EventHandler<PointerEventArgs>? onClipMoved,
        EventHandler<PointerReleasedEventArgs>? onClipReleased)
    {
        if (ctx.MainCanvas == null || ctx.MainScroll == null)
            return;

        var scrollY = ctx.VerticalTrackScroll?.Offset.Y ?? 0;
        var viewportH = ctx.VerticalTrackScroll?.Viewport.Height ?? ctx.MainCanvas.Height;
        var (first, last) = TimelineLaneVirtualizer.GetVisibleRowRange(
            scrollY,
            viewportH,
            ctx.Tracks.Count,
            TimelineRenderMetrics.TrackHeight);
        if (first == visibleFirstRow && last == visibleLastRow)
            return;

        StoreClipGestureHandlers(
            onLanePressed,
            onDoubleTapped,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);

        RemoveVirtualizedContentOutsideRowRange(ctx, first, last);
        visibleFirstRow = first;
        visibleLastRow = last;

        var contentW = Math.Max(ctx.Layout.ContentWidth(), ctx.MainScroll.Viewport.Width);
        EnsureVisibleLanes(
            ctx,
            contentW,
            first,
            last,
            onLanePressed,
            onDoubleTapped,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);
        ApplyTrackSelectionChrome(ctx);
        SyncPropertyTrackKeyframes(ctx);
    }

    public void SetPinnedClips(IEnumerable<string> clipIds)
    {
        pinnedClipIds.Clear();
        foreach (var id in clipIds)
            pinnedClipIds.Add(id);
    }

    public void ClearPinnedClips() => pinnedClipIds.Clear();

    /// <summary>
    /// Ensures lane + clip visuals exist for the clip's current track row and keeps the clip above lane chrome.
    /// </summary>
    public void SyncClipVisual(TimelineInteractionContext ctx, TimelineClipItem clip)
    {
        if (ctx.MainCanvas == null || ctx.MainScroll == null)
            return;

        var row = ctx.TrackRowIndex(clip.TrackId);
        if (row < 0)
            return;

        var inRange = row >= visibleFirstRow && row <= visibleLastRow;
        if (!inRange)
        {
            if (pinnedClipIds.Contains(clip.Id))
                LayoutClip(ctx, clip);
            else
                RemoveClipVisual(ctx, clip.Id);
            return;
        }

        var contentW = Math.Max(ctx.Layout.ContentWidth(), ctx.MainScroll.Viewport.Width);
        EnsureLaneForRow(ctx, contentW, row);
        AddOrRefreshClipVisual(
            ctx,
            clip,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);
        BringClipAboveLanes(ctx, clip.Id);
    }

    public void AddOrRefreshClipVisual(
        TimelineInteractionContext ctx,
        TimelineClipItem clip,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed,
        EventHandler<PointerEventArgs>? onTrimMoved,
        EventHandler<PointerReleasedEventArgs>? onTrimReleased,
        EventHandler<PointerPressedEventArgs>? onClipPressed,
        EventHandler<PointerEventArgs>? onClipMoved,
        EventHandler<PointerReleasedEventArgs>? onClipReleased)
    {
        if (ctx.MainCanvas == null)
            return;
        var row = ctx.TrackRowIndex(clip.TrackId);
        if (row < visibleFirstRow || row > visibleLastRow)
            return;

        if (clipBorders.ContainsKey(clip.Id))
        {
            LayoutClip(ctx, clip);
            RefreshClipChrome(ctx, clip.Id);
            if (clipBorders.TryGetValue(clip.Id, out var existingRoot))
                ApplyClipTrackVisibility(ctx, clip, existingRoot);
            return;
        }

        var gripBrush = new SolidColorBrush(Color.FromArgb(130, 255, 255, 255));
        var left = new Border { Background = gripBrush, Cursor = new Cursor(StandardCursorType.SizeWestEast) };
        if (onTrimPressed != null)
            left.PointerPressed += (_, e) => onTrimPressed(clip, true, left, e);
        if (onTrimMoved != null)
            left.PointerMoved += onTrimMoved;
        if (onTrimReleased != null)
            left.PointerReleased += onTrimReleased;

        var chrome = new TimelineClipChromeLayout(
            new Image(),
            new TextBlock
            {
                Text = clipLabelFormatter.Format(clip),
                Foreground = ctx.Control.Foreground,
                FontSize = 11,
            });
        var body = new Border
        {
            CornerRadius = new CornerRadius(4),
            Background = ctx.Selection.IsClipSelected(clip.Id)
                ? ctx.Control.ClipSelectedBrush ?? ctx.Control.ClipBrush
                : ctx.Control.ClipBrush,
            BorderBrush = LaneSeparator(ctx),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4, 2, 4, 2),
            Child = chrome.Root,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
            Tag = clip.Id,
        };
        clipChromeLayouts[clip.Id] = chrome;
        if (onClipPressed != null)
            body.PointerPressed += onClipPressed;
        if (onClipMoved != null)
            body.PointerMoved += onClipMoved;
        if (onClipReleased != null)
            body.PointerReleased += onClipReleased;

        var right = new Border { Background = gripBrush, Cursor = new Cursor(StandardCursorType.SizeWestEast) };
        if (onTrimPressed != null)
            right.PointerPressed += (_, e) => onTrimPressed(clip, false, right, e);
        if (onTrimMoved != null)
            right.PointerMoved += onTrimMoved;
        if (onTrimReleased != null)
            right.PointerReleased += onTrimReleased;

        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TimelineRenderMetrics.TrimHandleWidth) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TimelineRenderMetrics.TrimHandleWidth) });
        Grid.SetColumn(left, 0);
        Grid.SetColumn(body, 1);
        Grid.SetColumn(right, 2);
        root.Children.Add(left);
        root.Children.Add(body);
        root.Children.Add(right);

        ApplyClipTrackVisibility(ctx, clip, root);
        ctx.MainCanvas.Children.Add(root);
        clipBorders[clip.Id] = root;
        clipBodyById[clip.Id] = body;
        LayoutClip(ctx, clip);
    }

    public void UpdateClipLabel(TimelineInteractionContext ctx, TimelineClipItem clip)
    {
        if (clipChromeLayouts.TryGetValue(clip.Id, out var chrome))
        {
            chrome.Label.Text = clipLabelFormatter.Format(clip);
            return;
        }

        if (!clipBodyById.TryGetValue(clip.Id, out var body) || body.Child is not TextBlock label)
            return;
        label.Text = clipLabelFormatter.Format(clip);
    }

    public void ApplyClipThumbnail(TimelineInteractionContext ctx, string clipId, IImage? image)
    {
        if (!clipChromeLayouts.TryGetValue(clipId, out var chrome))
            return;
        chrome.Thumbnail.Source = image;
        chrome.Thumbnail.IsVisible = image is not null;
    }

    public void EnumerateHeaderAffordances(Action<Border> visit)
    {
        foreach (var header in headerChromeByTrackId.Values)
            VisitAffordanceBorders(header, visit);
    }

    private static void VisitAffordanceBorders(Control root, Action<Border> visit)
    {
        if (root is Border { Tag: string tag } border
            && TimelineTrackHeaderTags.TryParse(tag, out var kind, out _)
            && kind is "vis" or "lock")
            visit(border);
        if (root is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control control)
                    VisitAffordanceBorders(control, visit);
            }
        }
    }

    public void LayoutClip(TimelineInteractionContext ctx, TimelineClipItem clip)
    {
        if (!clipBorders.TryGetValue(clip.Id, out var root))
            return;
        var row = ctx.TrackRowIndex(clip.TrackId);
        if (row < 0)
            return;
        var left = ctx.Layout.TimeToPixel(clip.StartTime);
        var width = Math.Max(10, ctx.Layout.TimeToPixel(clip.Duration));
        Canvas.SetLeft(root, left);
        Canvas.SetTop(root, row * TimelineRenderMetrics.TrackHeight + 5);
        root.Width = width;
        root.Height = TimelineRenderMetrics.TrackHeight - 10;
    }

    public void RefreshClipChrome(TimelineInteractionContext ctx)
    {
        foreach (var kv in clipBodyById)
        {
            kv.Value.Background = ctx.Selection.IsClipSelected(kv.Key)
                ? ctx.Control.ClipSelectedBrush ?? ctx.Control.ClipBrush
                : ctx.Control.ClipBrush;
        }
    }

    public void RefreshClipChrome(TimelineInteractionContext ctx, string clipId)
    {
        if (!clipBodyById.TryGetValue(clipId, out var body))
            return;
        body.Background = ctx.Selection.IsClipSelected(clipId)
            ? ctx.Control.ClipSelectedBrush ?? ctx.Control.ClipBrush
            : ctx.Control.ClipBrush;
    }

    public void ApplyTrackSelectionChrome(TimelineInteractionContext ctx)
    {
        if (headerChromeByTrackId.Count == 0 && laneChromeByTrackId.Count == 0)
            return;

        var hi = ctx.Control.TrackSelectionBrush ?? ctx.Control.PlayheadBrush ?? Brushes.LimeGreen;
        var sep = LaneSeparator(ctx);
        foreach (var kv in headerChromeByTrackId)
        {
            var selected = kv.Key == ctx.Selection.SelectedTrackId;
            kv.Value.BorderBrush = selected ? hi : Brushes.Transparent;
            kv.Value.BorderThickness = selected ? new Thickness(2) : new Thickness(0, 0, 0, 1);
        }

        foreach (var kv in laneChromeByTrackId)
        {
            var selected = kv.Key == ctx.Selection.SelectedTrackId;
            kv.Value.BorderBrush = selected ? hi : sep;
            kv.Value.BorderThickness = selected ? new Thickness(2) : new Thickness(0, 0, 0, 1);
        }
    }

    public void UpdateOverlays(TimelineInteractionContext ctx)
    {
        if (ctx.MainCanvas == null || playheadLine == null || selectionRect == null)
            return;

        var h = ctx.MainCanvas.Height;
        if (double.IsNaN(h) || h <= 0)
            h = MeasureTrackContentHeight(ctx);
        var x = ctx.Layout.TimeToPixel(ctx.PlayheadTime);
        playheadLine.StartPoint = new Point(x, 0);
        playheadLine.EndPoint = new Point(x, h);

        if (ctx.HasTimeRangeSelection)
        {
            selectionRect.IsVisible = true;
            var x0 = ctx.Layout.TimeToPixel(ctx.TimeRangeSelection.Min);
            var x1 = ctx.Layout.TimeToPixel(ctx.TimeRangeSelection.Max);
            Canvas.SetLeft(selectionRect, x0);
            Canvas.SetTop(selectionRect, 0);
            selectionRect.Width = Math.Max(1, x1 - x0);
            selectionRect.Height = h;
        }
        else
        {
            selectionRect.IsVisible = false;
        }
    }

    public void BeginTrackReorderDrag(
        TimelineInteractionContext ctx,
        Border header,
        string trackId,
        double pressYInHeaderStack)
    {
        if (reorderFromRow(ctx, trackId) < 0)
            return;
        trackReorderDragVisual.Begin(ctx, header, trackId, pressYInHeaderStack);
    }

    public void UpdateTrackReorderDrag(TimelineInteractionContext ctx, int fromIndex, double pointerYInHeaderStack) =>
        trackReorderDragVisual.Update(ctx, fromIndex, pointerYInHeaderStack);

    public void EndTrackReorderDrag(TimelineInteractionContext ctx) =>
        trackReorderDragVisual.End(ctx);

    private static int reorderFromRow(TimelineInteractionContext ctx, string trackId)
    {
        for (var i = 0; i < ctx.Tracks.Count; i++)
        {
            if (ctx.Tracks[i].Id == trackId)
                return i;
        }

        return -1;
    }

    public double MeasureTrackContentHeight(TimelineInteractionContext ctx)
    {
        var vh = 0.0;
        if (ctx.VerticalTrackScroll != null)
        {
            vh = ctx.VerticalTrackScroll.Viewport.Height;
            if (double.IsNaN(vh) || vh < 1)
                vh = ctx.VerticalTrackScroll.Bounds.Height;
        }

        return TimelineLaneVirtualizer.MeasureContentHeight(
            ctx.Tracks.Count,
            TimelineRenderMetrics.TrackHeight,
            vh);
    }

    private void UpdateVisibleLanes(
        TimelineInteractionContext ctx,
        double contentW,
        EventHandler<PointerPressedEventArgs>? onLanePressed,
        EventHandler<TappedEventArgs>? onDoubleTapped,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed,
        EventHandler<PointerEventArgs>? onTrimMoved,
        EventHandler<PointerReleasedEventArgs>? onTrimReleased,
        EventHandler<PointerPressedEventArgs>? onClipPressed,
        EventHandler<PointerEventArgs>? onClipMoved,
        EventHandler<PointerReleasedEventArgs>? onClipReleased)
    {
        var scrollY = ctx.VerticalTrackScroll?.Offset.Y ?? 0;
        var viewportH = ctx.VerticalTrackScroll?.Viewport.Height ?? ctx.MainCanvas!.Height;
        (visibleFirstRow, visibleLastRow) = TimelineLaneVirtualizer.GetVisibleRowRange(
            scrollY,
            viewportH,
            ctx.Tracks.Count,
            TimelineRenderMetrics.TrackHeight);

        EnsureVisibleLanes(
            ctx,
            contentW,
            visibleFirstRow,
            visibleLastRow,
            onLanePressed,
            onDoubleTapped,
            onTrimPressed,
            onTrimMoved,
            onTrimReleased,
            onClipPressed,
            onClipMoved,
            onClipReleased);
    }

    private void EnsureVisibleLanes(
        TimelineInteractionContext ctx,
        double contentW,
        int firstRow,
        int lastRow,
        EventHandler<PointerPressedEventArgs>? onLanePressed,
        EventHandler<TappedEventArgs>? onDoubleTapped,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? onTrimPressed,
        EventHandler<PointerEventArgs>? onTrimMoved,
        EventHandler<PointerReleasedEventArgs>? onTrimReleased,
        EventHandler<PointerPressedEventArgs>? onClipPressed,
        EventHandler<PointerEventArgs>? onClipMoved,
        EventHandler<PointerReleasedEventArgs>? onClipReleased)
    {
        for (var i = firstRow; i <= lastRow && i < ctx.Tracks.Count; i++)
        {
            var track = ctx.Tracks[i];
            EnsureLaneForRow(ctx, contentW, i, onLanePressed, onDoubleTapped);

            foreach (var clip in ctx.Clips.Where(c => c.TrackId == track.Id))
            {
                AddOrRefreshClipVisual(
                    ctx,
                    clip,
                    onTrimPressed,
                    onTrimMoved,
                    onTrimReleased,
                    onClipPressed,
                    onClipMoved,
                    onClipReleased);
            }
        }
    }

    private void EnsureKeyframeVisual(TimelineInteractionContext ctx, TimelineKeyframeItem keyframe)
    {
        if (ctx.MainCanvas is null || keyframeShapesById.ContainsKey(keyframe.Id))
            return;
        var shape = new Polygon
        {
            Fill = ctx.Control.ClipSelectedBrush ?? Brushes.Gold,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5,
            IsHitTestVisible = true,
            Tag = keyframe.Id,
        };
        if (onKeyframePressed != null)
            shape.PointerPressed += onKeyframePressed;
        if (onKeyframeMoved != null)
            shape.PointerMoved += onKeyframeMoved;
        if (onKeyframeReleased != null)
            shape.PointerReleased += onKeyframeReleased;
        ctx.MainCanvas.Children.Add(shape);
        keyframeShapesById[keyframe.Id] = shape;
        BringKeyframeAboveLanes(ctx, shape);
    }

    private void RemoveKeyframeVisual(TimelineInteractionContext ctx, string keyframeId)
    {
        if (!keyframeShapesById.TryGetValue(keyframeId, out var shape))
            return;
        ctx.MainCanvas?.Children.Remove(shape);
        keyframeShapesById.Remove(keyframeId);
    }

    private static void BringKeyframeAboveLanes(TimelineInteractionContext ctx, Polygon shape)
    {
        if (ctx.MainCanvas is null)
            return;
        ctx.MainCanvas.Children.Remove(shape);
        ctx.MainCanvas.Children.Add(shape);
    }

    private void StoreClipGestureHandlers(
        EventHandler<PointerPressedEventArgs>? lanePressed,
        EventHandler<TappedEventArgs>? doubleTapped,
        Action<TimelineClipItem, bool, IInputElement, PointerPressedEventArgs>? trimPressed,
        EventHandler<PointerEventArgs>? trimMoved,
        EventHandler<PointerReleasedEventArgs>? trimReleased,
        EventHandler<PointerPressedEventArgs>? clipPressed,
        EventHandler<PointerEventArgs>? clipMoved,
        EventHandler<PointerReleasedEventArgs>? clipReleased)
    {
        onLanePressed = lanePressed;
        onDoubleTapped = doubleTapped;
        onTrimPressed = trimPressed;
        onTrimMoved = trimMoved;
        onTrimReleased = trimReleased;
        onClipPressed = clipPressed;
        onClipMoved = clipMoved;
        onClipReleased = clipReleased;
    }

    private void EnsureLaneForRow(
        TimelineInteractionContext ctx,
        double contentW,
        int row,
        EventHandler<PointerPressedEventArgs>? lanePressed = null,
        EventHandler<TappedEventArgs>? doubleTapped = null)
    {
        if (ctx.MainCanvas == null || row < 0 || row >= ctx.Tracks.Count)
            return;

        lanePressed ??= onLanePressed;
        doubleTapped ??= onDoubleTapped;
        var track = ctx.Tracks[row];
        if (laneChromeByTrackId.ContainsKey(track.Id))
            return;

        var y = row * TimelineRenderMetrics.TrackHeight;
        var lane = new Border
        {
            Height = TimelineRenderMetrics.TrackHeight,
            Width = contentW,
            Background = ctx.Control.ClipLaneBrush,
            BorderBrush = LaneSeparator(ctx),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Tag = track.Id,
            Cursor = new Cursor(StandardCursorType.Arrow),
        };
        if (lanePressed != null)
            lane.PointerPressed += lanePressed;
        if (doubleTapped != null)
            lane.DoubleTapped += doubleTapped;
        Canvas.SetLeft(lane, 0);
        lane.Opacity = track.IsVisible ? 1 : 0.35;
        Canvas.SetTop(lane, y);
        ctx.MainCanvas.Children.Add(lane);
        laneChromeByTrackId[track.Id] = lane;
    }

    private void ApplyTrackAccentChrome(TimelineTrack track, Border header)
    {
        if (!accentColorParser.TryParseBrush(track.AccentColor, out var brush) || brush is null)
            return;
        header.BorderBrush = brush;
        header.BorderThickness = new Thickness(3, 0, 0, 1);
    }

    private void ApplyClipTrackVisibility(TimelineInteractionContext ctx, TimelineClipItem clip, Control root)
    {
        var track = ctx.Tracks.FirstOrDefault(t => t.Id == clip.TrackId);
        root.Opacity = track is { IsVisible: false } ? 0.35 : 1;
    }

    private void BringClipAboveLanes(TimelineInteractionContext ctx, string clipId)
    {
        if (ctx.MainCanvas is null || !clipBorders.TryGetValue(clipId, out var root))
            return;

        var insertIndex = ctx.MainCanvas.Children.Count;
        if (selectionRect != null)
        {
            var idx = ctx.MainCanvas.Children.IndexOf(selectionRect);
            if (idx >= 0)
                insertIndex = idx;
        }

        if (ReferenceEquals(ctx.MainCanvas.Children[insertIndex - 1], root))
            return;

        ctx.MainCanvas.Children.Remove(root);
        ctx.MainCanvas.Children.Insert(insertIndex, root);
    }

    private void RemoveClipVisual(TimelineInteractionContext ctx, string clipId)
    {
        if (ctx.MainCanvas is null)
            return;
        if (clipBorders.TryGetValue(clipId, out var root))
            ctx.MainCanvas.Children.Remove(root);
        clipBorders.Remove(clipId);
        clipBodyById.Remove(clipId);
        clipChromeLayouts.Remove(clipId);
    }

    private void RemoveVirtualizedContentOutsideRowRange(TimelineInteractionContext ctx, int firstRow, int lastRow)
    {
        if (ctx.MainCanvas == null)
            return;

        foreach (var trackId in laneChromeByTrackId.Keys.ToList())
        {
            var row = ctx.TrackRowIndex(trackId);
            if (row >= firstRow && row <= lastRow)
                continue;
            if (laneChromeByTrackId.TryGetValue(trackId, out var lane))
            {
                ctx.MainCanvas.Children.Remove(lane);
                laneChromeByTrackId.Remove(trackId);
            }
        }

        foreach (var clipId in clipBorders.Keys.ToList())
        {
            if (pinnedClipIds.Contains(clipId))
                continue;

            var clip = ctx.Clips.FirstOrDefault(c => c.Id == clipId);
            if (clip is null)
            {
                RemoveClipVisual(ctx, clipId);
                continue;
            }

            var row = ctx.TrackRowIndex(clip.TrackId);
            if (row >= firstRow && row <= lastRow)
                continue;
            RemoveClipVisual(ctx, clipId);
        }
    }

    private static IBrush LaneSeparator(TimelineInteractionContext ctx) =>
        ctx.Control.LaneSeparatorBrush ?? ctx.Control.RulerTickBrush ?? new SolidColorBrush(Color.Parse("#333333"));
}
