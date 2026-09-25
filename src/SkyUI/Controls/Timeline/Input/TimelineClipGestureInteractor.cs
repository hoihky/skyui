using Avalonia.Controls;
using Avalonia.Input;
using SkyUI.Controls.Timeline.Commands;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineClipGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;
    private TimelineClipItem? dragClip;
    private List<TimelineClipItem>? dragMovingClips;
    private IReadOnlyList<TimelineMoveClipsCommand.ClipMoveSnapshot>? dragMoveBefore;
    private double dragAnchorOrigStart;
    private double dragClipPointerTimeOffset;
    private int dragClipStartRow;
    private TimelineClipItem? trimClip;
    private bool trimLeftEdge;
    private double trimAnchorTime;
    private double trimOrigStart;
    private double trimOrigDuration;

    public int Priority => 80;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnClipPressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx == null || sender is not Control { Tag: string id })
            return;
        var clip = ctx.Clips.FirstOrDefault(c => c.Id == id);
        if (clip == null)
            return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            ToggleSelect(ctx, clip);
        else if (!ctx.Selection.IsClipSelected(clip.Id))
            SelectSingle(ctx, clip);

        dragClip = clip;
        dragMovingClips = ResolveDragClipSet(ctx, clip);
        dragMoveBefore = TimelineMoveClipsCommand.Capture(dragMovingClips);
        dragAnchorOrigStart = clip.StartTime;
        var t = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas!));
        dragClipPointerTimeOffset = t - clip.StartTime;
        dragClipStartRow = ctx.TrackRowIndex(clip.TrackId);
        ctx.Renderer.SetPinnedClips(dragMovingClips.Select(c => c.Id));
        ctx.PlayheadTime = clip.StartTime;
        e.Pointer.Capture((IInputElement)sender!);
        e.Handled = true;
    }

    public void OnClipMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (trimClip != null || dragClip == null || dragMovingClips == null || ctx?.MainCanvas == null)
            return;

        var anchorStart = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas)) - dragClipPointerTimeOffset;
        anchorStart = ctx.Layout.SnapClipStart(
            anchorStart,
            dragClip.Duration,
            ctx.PlayheadTime,
            ctx.Markers,
            ctx.Clips,
            dragClip.Id);
        anchorStart = ctx.Layout.ClampClipStart(anchorStart, dragClip.Duration);
        var timeDelta = anchorStart - dragAnchorOrigStart;

        var rowDelta = 0;
        var p = e.GetPosition(ctx.MainCanvas);
        var row = (int)Math.Floor(p.Y / Rendering.TimelineRenderMetrics.TrackHeight);
        row = Math.Clamp(row, 0, Math.Max(0, ctx.Tracks.Count - 1));
        rowDelta = row - dragClipStartRow;

        foreach (var moving in dragMovingClips)
        {
            if (dragMoveBefore is null)
                continue;
            var origin = dragMoveBefore.First(s => s.ClipId == moving.Id);
            moving.StartTime = ctx.Layout.ClampClipStart(origin.StartTime + timeDelta, moving.Duration);
            var originRow = ctx.TrackRowIndex(origin.TrackId);
            if (originRow >= 0)
            {
                var newRow = Math.Clamp(originRow + rowDelta, 0, Math.Max(0, ctx.Tracks.Count - 1));
                if (newRow < ctx.Tracks.Count)
                    moving.TrackId = ctx.Tracks[newRow].Id;
            }

            ctx.Renderer.SyncClipVisual(ctx, moving);
        }

        e.Handled = true;
    }

    public void OnClipReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        if (dragClip == null || ctx == null)
            return;

        var movedClips = dragMovingClips?.ToList();
        if (dragMovingClips is not null && dragMoveBefore is not null)
        {
            var after = TimelineMoveClipsCommand.Capture(dragMovingClips);
            if (!MoveSnapshotsEqual(dragMoveBefore, after))
            {
                ctx.RaiseClipEdit(dragClip, "move", true);
                ctx.UndoStack.Execute(new TimelineMoveClipsCommand(dragMovingClips, dragMoveBefore, after));
                ctx.RaiseClipEdit(dragClip, "move", false);
            }
        }

        ctx.Renderer.ClearPinnedClips();
        if (movedClips is not null)
        {
            var anchorRow = ctx.TrackRowIndex(dragClip.TrackId);
            if (anchorRow >= 0)
                ctx.Host.ScrollTrackRowIntoView(anchorRow);
            foreach (var moving in movedClips)
                ctx.Renderer.SyncClipVisual(ctx, moving);
        }

        e.Pointer.Capture(null);
        dragClip = null;
        dragMovingClips = null;
        dragMoveBefore = null;
    }

    public void OnTrimPressed(TimelineClipItem clip, bool leftEdge, IInputElement captureTo, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx == null)
            return;
        SelectSingle(ctx, clip);
        trimClip = clip;
        trimLeftEdge = leftEdge;
        trimOrigStart = clip.StartTime;
        trimOrigDuration = clip.Duration;
        trimAnchorTime = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas!));
        e.Pointer.Capture(captureTo);
        e.Handled = true;
    }

    public void OnTrimMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (trimClip == null || ctx?.MainCanvas == null || sender is not Control border)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, border))
            return;

        var t = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas));
        var delta = t - trimAnchorTime;
        if (trimLeftEdge)
        {
            var end = trimOrigStart + trimOrigDuration;
            var newStart = Math.Clamp(
                trimOrigStart + delta,
                0,
                end - TimelineCoordinateSystem.MinClipDurationSeconds);
            trimClip.StartTime = newStart;
            trimClip.Duration = end - newStart;
        }
        else
        {
            var newEnd = Math.Clamp(
                trimOrigStart + trimOrigDuration + delta,
                trimOrigStart + TimelineCoordinateSystem.MinClipDurationSeconds,
                ctx.Duration);
            trimClip.Duration = newEnd - trimOrigStart;
        }

        ctx.Renderer.LayoutClip(ctx, trimClip);
        e.Handled = true;
    }

    public void OnTrimReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        if (trimClip is null || ctx == null)
        {
            e.Pointer.Capture(null);
            return;
        }

        var clip = trimClip;
        var finalStart = clip.StartTime;
        var finalDuration = clip.Duration;
        if (trimLeftEdge)
        {
            finalStart = ctx.Layout.SnapTime(finalStart, ctx.PlayheadTime, ctx.Markers, ctx.Clips, clip.Id);
            finalStart = Math.Clamp(
                finalStart,
                0,
                trimOrigStart + trimOrigDuration - TimelineCoordinateSystem.MinClipDurationSeconds);
            finalDuration = trimOrigStart + trimOrigDuration - finalStart;
        }
        else
        {
            finalStart = trimOrigStart;
            var finalEnd = ctx.Layout.SnapTime(
                finalStart + finalDuration,
                ctx.PlayheadTime,
                ctx.Markers,
                ctx.Clips,
                clip.Id);
            finalEnd = Math.Clamp(
                finalEnd,
                trimOrigStart + TimelineCoordinateSystem.MinClipDurationSeconds,
                ctx.Duration);
            finalDuration = finalEnd - trimOrigStart;
        }

        clip.StartTime = finalStart;
        clip.Duration = finalDuration;
        ctx.Renderer.LayoutClip(ctx, clip);

        if (Math.Abs(finalStart - trimOrigStart) > 1e-9
            || Math.Abs(finalDuration - trimOrigDuration) > 1e-9)
        {
            ctx.RaiseClipEdit(clip, "trim", true);
            ctx.UndoStack.Execute(new TimelineTrimClipCommand(
                clip, trimOrigStart, trimOrigDuration, finalStart, finalDuration));
            ctx.RaiseClipEdit(clip, "trim", false);
        }

        trimClip = null;
        e.Pointer.Capture(null);
    }

    private static void SelectSingle(TimelineInteractionContext ctx, TimelineClipItem clip)
    {
        ctx.Selection.SelectSingleClip(clip.Id);
        ctx.RaiseClipSelectionChanged(clip);
    }

    private static void ToggleSelect(TimelineInteractionContext ctx, TimelineClipItem clip)
    {
        ctx.Selection.ToggleClip(clip.Id);
        ctx.RaiseClipSelectionChanged(clip);
    }

    private static List<TimelineClipItem> ResolveDragClipSet(TimelineInteractionContext ctx, TimelineClipItem anchor)
    {
        if (ctx.Selection.IsClipSelected(anchor.Id) && ctx.Selection.SelectedClipIds.Count > 0)
            return ctx.Selection.ResolveClips(ctx.Clips).ToList();
        return new List<TimelineClipItem> { anchor };
    }

    private static bool MoveSnapshotsEqual(
        IReadOnlyList<TimelineMoveClipsCommand.ClipMoveSnapshot> before,
        IReadOnlyList<TimelineMoveClipsCommand.ClipMoveSnapshot> after)
    {
        if (before.Count != after.Count)
            return false;
        var beforeById = before.ToDictionary(s => s.ClipId);
        foreach (var snapshot in after)
        {
            if (!beforeById.TryGetValue(snapshot.ClipId, out var prior))
                return false;
            if (Math.Abs(prior.StartTime - snapshot.StartTime) > 1e-9 || prior.TrackId != snapshot.TrackId)
                return false;
        }

        return true;
    }
}
