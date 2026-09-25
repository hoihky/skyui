using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.Demo.Views.Demos;

public partial class VideoTimelineDemo : UserControl
{
    public VideoTimelineDemo()
    {
        InitializeComponent();
        Log.ItemsSource = LogItems;
        BuildSample();
        Timeline.TimeRangeSelectionChanged += (_, e) =>
            LogLine($"Range: {e.Range.Min:F2}s – {e.Range.Max:F2}s (len {e.Range.Length:F2}s)");
        Timeline.TrackOrderChanged += (_, e) =>
            LogLine($"Track reordered: {e.Track.Name} {e.OldIndex} → {e.NewIndex}");
        Timeline.TrackSelected += (_, e) => LogLine($"Track selected: {e.Track.Name}");
        Timeline.TrackRemoved += (_, e) => LogLine($"Track removed: {e.Track.Name}");
        Timeline.MarkerAdded += (_, e) => LogLine($"Marker: {e.Marker.Label} @ {e.Marker.Time:F2}s");
        Timeline.ClipSelectionChanged += (_, e) => LogLine($"Clip selected: {e.Clip.Label}");
        Timeline.ClipsPasted += (_, e) => LogLine($"Pasted {e.Clips.Count} clip(s)");
        Timeline.ClipsRemoved += (_, e) => LogLine($"Removed {e.Clips.Count} clip(s)");
        Timeline.ClipChanged += (_, e) => LogLine($"Clip {e.EditKind}: {e.Clip.Label}");
        Timeline.SelectionChanged += (_, e) =>
            LogLine($"Selection: {e.SelectedClips.Count} clip(s), track={e.SelectedTrack?.Name ?? "(none)"}");
        Timeline.UndoRedoStateChanged += (_, _) =>
            LogLine($"Undo={Timeline.CanUndo}, Redo={Timeline.CanRedo}");
    }

    private void BuildSample()
    {
        Timeline.Tracks.Clear();
        Timeline.Clips.Clear();
        Timeline.Markers.Clear();

        var a = new TimelineTrackItem { Name = "Video A" };
        var b = new TimelineTrackItem { Name = "Video B" };
        var c = new TimelineTrackItem { Name = "Audio" };
        Timeline.Tracks.Add(a);
        Timeline.Tracks.Add(b);
        Timeline.Tracks.Add(c);

        Timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = a.Id,
            StartTime = 2,
            Duration = 8,
            Label = "Intro",
        });
        Timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = a.Id,
            StartTime = 18,
            Duration = 12,
            Label = "Main",
        });
        Timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = b.Id,
            StartTime = 6,
            Duration = 10,
            Label = "B-roll",
        });
        Timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = c.Id,
            StartTime = 0,
            Duration = 40,
            Label = "Music bed",
        });

        Timeline.PlayheadTime = 0;
        LogLine("Sample project loaded.");
    }

    private void LogLine(string line)
    {
        LogItems.Add(line);
        if (LogItems.Count > 200)
            LogItems.RemoveAt(0);
    }

    private ObservableCollection<string> LogItems { get; } = new();

    private void OnPlayClick(object? sender, RoutedEventArgs e) => Timeline.Play();

    private void OnStopClick(object? sender, RoutedEventArgs e) => Timeline.Stop();

    private void OnAddTrackClick(object? sender, RoutedEventArgs e)
    {
        Timeline.AddTrack($"Track {Timeline.Tracks.Count + 1}");
        LogLine("Track added.");
    }

    private void OnAddClipClick(object? sender, RoutedEventArgs e)
    {
        if (Timeline.Tracks.Count == 0)
            return;
        var track = Timeline.Tracks.FirstOrDefault(t => t.Id == Timeline.SelectedTrackId) ?? Timeline.Tracks[0];
        Timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = track.Id,
            StartTime = Timeline.PlayheadTime,
            Duration = 5,
            Label = $"Clip @ {Timeline.PlayheadTime:F1}s",
        });
        LogLine("Clip added at playhead.");
    }

    private void OnRemoveClipClick(object? sender, RoutedEventArgs e)
    {
        var sel = Timeline.GetSelectedClips().FirstOrDefault();
        if (sel == null)
        {
            LogLine("No clip selected.");
            return;
        }

        Timeline.RemoveClip(sel);
        LogLine("Removed selected clip.");
    }

    private void OnSplitClick(object? sender, RoutedEventArgs e)
    {
        var created = Timeline.SplitClipAtPlayhead();
        LogLine(created is null ? "Select a clip spanning the playhead to split." : $"Split created: {created.Label}");
    }

    private void OnUndoClick(object? sender, RoutedEventArgs e)
    {
        Timeline.Undo();
        LogLine(Timeline.CanUndo ? "Undone." : "Nothing to undo.");
    }

    private void OnRedoClick(object? sender, RoutedEventArgs e)
    {
        Timeline.Redo();
        LogLine(Timeline.CanRedo ? "Redone." : "Nothing to redo.");
    }

    private void OnSnapToggleChanged(object? sender, RoutedEventArgs e)
    {
        Timeline.SnapSettings.IsEnabled = SnapToggle.IsChecked == true;
        LogLine($"Snap {(Timeline.SnapSettings.IsEnabled ? "on" : "off")}.");
    }

    private void OnRemoveTrackClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Timeline.SelectedTrackId))
        {
            LogLine("No track selected.");
            return;
        }

        if (Timeline.GetSelectedClips().Count > 0)
        {
            LogLine("Clear clip selection first to remove a track.");
            return;
        }

        Timeline.RemoveSelectedTrack();
        LogLine("Removed selected track.");
    }
}
