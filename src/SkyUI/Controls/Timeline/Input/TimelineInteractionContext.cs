using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using SkyUI.Controls.Timeline.Commands;
using SkyUI.Controls.Timeline.Layout;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.Controls.Timeline.Input;

/// <summary>Shared services and template parts for timeline gesture interactors and rendering.</summary>
public sealed class TimelineInteractionContext
{
    public required TimelineHost Host { get; init; }

    public VideoTimeline Control => Host.Control;

    public required TimelineProject Project { get; init; }

    public required ITimelineLayoutEngine Layout { get; init; }

    public required TimelineSelectionModel Selection { get; init; }

    public required TimelineUndoStack UndoStack { get; init; }

    public required TimelineRenderer Renderer { get; init; }

    public ScrollViewer? RulerScroll { get; set; }

    public Canvas? RulerCanvas { get; set; }

    public ScrollViewer? VerticalTrackScroll { get; set; }

    public ScrollViewer? MainScroll { get; set; }

    public Canvas? MainCanvas { get; set; }

    public StackPanel? HeaderStack { get; set; }

    public double PlayheadTime
    {
        get => Control.PlayheadTime;
        set => Control.PlayheadTime = value;
    }

    public double Duration => Control.Duration;

    public double PixelsPerSecond => Control.PixelsPerSecond;

    public ObservableCollection<TimelineTrackItem> Tracks => Project.Tracks;

    public ObservableCollection<TimelineClipItem> Clips => Project.Clips;

    public ObservableCollection<TimelineMarkerItem> Markers => Project.Markers;

    public IList<TimelineClipItem> Clipboard { get; } = new List<TimelineClipItem>();

    public bool HasTimeRangeSelection { get; set; }

    public TimelineTimeRange TimeRangeSelection { get; set; }

    public Action FullRebuild { get; init; } = static () => { };

    public Action UpdateOverlays { get; init; } = static () => { };

    public Action RaiseUndoRedoStateChanged { get; init; } = static () => { };

    public Action RaiseSelectionChanged { get; init; } = static () => { };

    public Action<TimelineClipItem, string, bool> RaiseClipEdit { get; init; } = static (_, _, _) => { };

    public Action<TimelineClipItem> RaiseClipSelectionChanged { get; init; } = static _ => { };

    public Action<TimelineTimeRange> RaiseTimeRangeChanged { get; init; } = static _ => { };

    public Action<TimelineTrackItem, int, int> RaiseTrackReordered { get; init; } = static (_, _, _) => { };

    public Action<TimelineTrackItem> RaiseTrackSelected { get; init; } = static _ => { };

    public Action<TimelineMarkerItem> RaiseMarkerAdded { get; init; } = static _ => { };

    public Action<IReadOnlyList<TimelineClipItem>> RaiseClipsRemoved { get; init; } = static _ => { };

    public Action<IReadOnlyList<TimelineClipItem>> RaiseClipsPasted { get; init; } = static _ => { };

    public int TrackRowIndex(string trackId)
    {
        for (var i = 0; i < Tracks.Count; i++)
        {
            if (Tracks[i].Id == trackId)
                return i;
        }

        return -1;
    }

    public double TimeFromMainPointer(Point canvasPoint) =>
        Layout.PointerTimeToSeconds(canvasPoint.X, MainScroll?.Offset.X ?? 0);

    public void SetSelectedTrackId(string? trackId) => Host.SetSelectedTrackId(trackId);
}
