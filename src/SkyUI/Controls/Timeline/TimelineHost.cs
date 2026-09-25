using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SkyUI.Controls.Timeline.Commands;
using SkyUI.Controls.Timeline.Editing;
using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Layout;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.Controls.Timeline;

/// <summary>Wires model, layout, rendering, commands, and gesture interactors for <see cref="VideoTimeline"/>.</summary>
public sealed class TimelineHost : IDisposable
{
    private readonly VideoTimeline control;
    private readonly TimelineProject project = new();
    private readonly TimelineLayoutEngine layout = new();
    private readonly TimelineSelectionModel selection = new();
    private readonly TimelineUndoStack undoStack = new();
    private readonly TimelineRenderer renderer = new();
    private readonly TimelineGestureCoordinator gestures = new();
    private readonly TimelineLaneGestureInteractor laneGesture = new();
    private readonly TimelineClipGestureInteractor clipGesture = new();
    private readonly TimelineRulerScrubGestureInteractor rulerGesture = new();
    private readonly TimelineTrackHeaderGestureInteractor headerGesture = new();
    private readonly DispatcherTimer playTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly HashSet<TimelineClipItem> hookedClips = new();
    private readonly HashSet<TimelineTrackItem> hookedTracks = new();
    private readonly HashSet<TimelineMarkerItem> hookedMarkers = new();

    public TimelineInteractionContext? Interaction => interaction;

    private TimelineInteractionContext? interaction;
    private bool syncScroll;
    private EventHandler? verticalScrollLayoutRestoreHandler;
    private (Vector Vertical, Vector Main, Vector Ruler) pendingLayoutScrollRestore;
    private int verticalScrollLayoutRestoreAttempts;

    public VideoTimeline Control => control;

    public TimelineHost(VideoTimeline control)
    {
        this.control = control;
        playTimer.Tick += OnPlayTick;
        selection.SelectionChanged += OnSelectionChanged;
        undoStack.StateChanged += OnUndoStackStateChanged;

        gestures.Register(new TimelineZoomGestureInteractor());
        gestures.Register(new TimelineKeyboardGestureInteractor());
        gestures.Register(laneGesture);
        gestures.Register(rulerGesture);
        gestures.Register(clipGesture);
        gestures.Register(headerGesture);

        HookTracks(project.Tracks);
        HookClips(project.Clips);
        HookMarkers(project.Markers);
    }

    public TimelineProject Project => project;

    public ITimelineLayoutEngine Layout => layout;

    public TimelineSelectionModel Selection => selection;

    public TimelineUndoStack UndoStack => undoStack;

    public TimelineSnapSettings SnapSettings => layout.SnapSettings;

    public bool CanUndo => undoStack.CanUndo;

    public bool CanRedo => undoStack.CanRedo;

    public ObservableCollection<TimelineTrackItem> Tracks => project.Tracks;

    public ObservableCollection<TimelineClipItem> Clips => project.Clips;

    public ObservableCollection<TimelineMarkerItem> Markers => project.Markers;

    public string? SelectedTrackId => selection.SelectedTrackId;

    public void ApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachScrollSync();
        var rulerScroll = e.NameScope.Find<ScrollViewer>(VideoTimeline.PartRulerScroll);
        var rulerCanvas = e.NameScope.Find<Canvas>(VideoTimeline.PartRulerCanvas);
        var verticalTrackScroll = e.NameScope.Find<ScrollViewer>(VideoTimeline.PartVerticalTrackScroll);
        var headerStack = e.NameScope.Find<StackPanel>(VideoTimeline.PartHeaderStack);
        var mainScroll = e.NameScope.Find<ScrollViewer>(VideoTimeline.PartMainScroll);
        var mainCanvas = e.NameScope.Find<Canvas>(VideoTimeline.PartMainCanvas);

        if (verticalTrackScroll != null)
            verticalTrackScroll.SizeChanged += OnVerticalTrackScrollSizeChanged;

        interaction = new TimelineInteractionContext
        {
            Host = this,
            Project = project,
            Layout = layout,
            Selection = selection,
            UndoStack = undoStack,
            Renderer = renderer,
            RulerScroll = rulerScroll,
            RulerCanvas = rulerCanvas,
            VerticalTrackScroll = verticalTrackScroll,
            HeaderStack = headerStack,
            MainScroll = mainScroll,
            MainCanvas = mainCanvas,
            FullRebuild = FullRebuild,
            UpdateOverlays = () => renderer.UpdateOverlays(interaction!),
            RaiseUndoRedoStateChanged = () => control.RaiseUndoRedoStateChanged(),
            RaiseSelectionChanged = () => control.RaiseSelectionChanged(),
            RaiseClipEdit = control.RaiseClipEdit,
            RaiseClipSelectionChanged = clip => control.RaiseClipSelectionChanged(clip),
            RaiseTimeRangeChanged = range => control.RaiseTimeRangeChanged(range),
            RaiseTrackReordered = (t, o, n) => control.RaiseTrackReordered(t, o, n),
            RaiseTrackSelected = t => control.RaiseTrackSelected(t),
            RaiseMarkerAdded = m => control.RaiseMarkerAdded(m),
            RaiseClipsRemoved = c => control.RaiseClipsRemoved(c),
            RaiseClipsPasted = c => control.RaiseClipsPasted(c),
        };

        AttachScrollSync();
        gestures.Attach(interaction);
        FullRebuild();
    }

    public void DetachTemplate()
    {
        gestures.Detach();
        DetachScrollSync();
        interaction = null;
    }

    public void Dispose()
    {
        StopPlayback();
        DetachTemplate();
        UnhookTracks(project.Tracks);
        UnhookClips(project.Clips);
        UnhookMarkers(project.Markers);
        playTimer.Tick -= OnPlayTick;
        selection.SelectionChanged -= OnSelectionChanged;
        undoStack.StateChanged -= OnUndoStackStateChanged;
        gestures.Dispose();
    }

    public void SyncLayoutFromControl()
    {
        layout.Duration = control.Duration;
        layout.PixelsPerSecond = control.PixelsPerSecond;
        project.Duration = control.Duration;
    }

    public void SetSelectedTrackId(string? value)
    {
        if (selection.SelectedTrackId == value)
            return;
        selection.SelectedTrackId = value;
    }

    public void SetTracks(ObservableCollection<TimelineTrackItem> value)
    {
        UnhookTracks(project.Tracks);
        project.Tracks = value;
        HookTracks(project.Tracks);
        FullRebuild();
    }

    public void SetClips(ObservableCollection<TimelineClipItem> value)
    {
        UnhookClips(project.Clips);
        project.Clips = value;
        HookClips(project.Clips);
        FullRebuild();
    }

    public void SetMarkers(ObservableCollection<TimelineMarkerItem> value)
    {
        UnhookMarkers(project.Markers);
        project.Markers = value;
        HookMarkers(project.Markers);
        FullRebuild();
    }

    public void Undo() => undoStack.Undo();

    public void Redo() => undoStack.Redo();

    public void RegisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        layout.RegisterSnapTargetProvider(provider);

    public void UnregisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        layout.UnregisterSnapTargetProvider(provider);

    public void RegisterGestureInteractor(ITimelineGestureInteractor interactor) =>
        gestures.Register(interactor);

    public IReadOnlyList<TimelineClipItem> GetSelectedClips() => selection.ResolveClips(project.Clips);

    public TimelineClipItem? SplitClipAtPlayhead(double playheadTime)
    {
        var clip = GetSelectedClips().FirstOrDefault();
        if (clip is null)
            return null;

        var beforeDuration = clip.Duration;
        control.RaiseClipEdit(clip, "split", changing: true);
        undoStack.Execute(new TimelineSplitClipCommand(clip, project.Clips, playheadTime, beforeDuration));
        var created = project.Clips.FirstOrDefault(c =>
            c.Id != clip.Id
            && c.TrackId == clip.TrackId
            && Math.Abs(c.StartTime - playheadTime) < 1e-6);
        if (created is not null)
            selection.SelectSingleClip(created.Id);
        if (interaction is not null)
            renderer.RefreshClipChrome(interaction);
        control.RaiseClipEdit(clip, "split", changing: false);
        return created;
    }

    public void Play() => control.IsPlaying = true;

    public void StopPlayback()
    {
        control.IsPlaying = false;
        playTimer.Stop();
    }

    public void ClearTimeRangeSelection()
    {
        if (interaction is null)
            return;
        interaction.HasTimeRangeSelection = false;
        renderer.UpdateOverlays(interaction);
        control.RaiseTimeRangeChanged(interaction.TimeRangeSelection);
    }

    public TimelineMarkerItem AddMarker(double timeSeconds, string label, object? tag = null)
    {
        var marker = new TimelineMarkerItem { Time = timeSeconds, Label = label, Tag = tag };
        project.Markers.Add(marker);
        control.RaiseMarkerAdded(marker);
        return marker;
    }

    public void AddTrack(string? name = null)
    {
        project.Tracks.Add(new TimelineTrackItem { Name = name ?? $"Track {project.Tracks.Count + 1}" });
        FullRebuild();
    }

    public void RemoveTrack(TimelineTrackItem track)
    {
        var id = track.Id;
        for (var i = project.Clips.Count - 1; i >= 0; i--)
        {
            if (project.Clips[i].TrackId == id)
                project.Clips.RemoveAt(i);
        }

        project.Tracks.Remove(track);
        if (selection.SelectedTrackId == id)
            SetSelectedTrackId(null);
        control.RaiseTrackRemoved(track);
        FullRebuild();
    }

    public void RemoveSelectedTrack()
    {
        if (selection.SelectedClipIds.Count > 0 || string.IsNullOrEmpty(selection.SelectedTrackId))
            return;
        var track = project.Tracks.FirstOrDefault(t => t.Id == selection.SelectedTrackId);
        if (track != null)
            RemoveTrack(track);
    }

    public void RemoveClip(TimelineClipItem clip)
    {
        project.Clips.Remove(clip);
        selection.ClearClipSelection();
        FullRebuild();
        control.RaiseClipSelectionChanged(clip);
    }

    public void CopySelection() => CopyInternal();

    public void CutSelection()
    {
        CopyInternal();
        DeleteSelectedClips();
    }

    public void PasteClipboardAtPlayhead(double playheadTime)
    {
        if (interaction is null || interaction.Clipboard.Count == 0 || project.Tracks.Count == 0)
            return;

        var trackId = ResolvePasteTargetTrackId();
        var pasted = new List<TimelineClipItem>();
        var cursor = playheadTime;
        foreach (var proto in interaction.Clipboard)
        {
            var nc = TimelineClipOperations.ClonePrototype(proto);
            nc.TrackId = trackId;
            nc.StartTime = cursor;
            cursor += proto.Duration + 0.05;
            project.Clips.Add(nc);
            pasted.Add(nc);
        }

        FullRebuild();
        control.RaiseClipsPasted(pasted);
    }

    public void OnPlayheadChanged()
    {
        if (interaction is null)
            return;
        renderer.UpdateOverlays(interaction);
    }

    public void OnIsPlayingChanged(bool playing)
    {
        if (playing)
            playTimer.Start();
        else
            playTimer.Stop();
    }

    public void OnTrackSelectionBrushChanged()
    {
        if (interaction is null)
            return;
        renderer.ApplyTrackSelectionChrome(interaction);
    }

    public void FullRebuild()
    {
        if (interaction is null)
            return;
        SyncLayoutFromControl();
        var scroll = CaptureScrollOffsets();
        RebuildAll();
        ApplyScrollRestore(scroll);
    }

    public void DeleteSelectedClips() => DeleteSelectedClipsCore();

    public void ScrollTrackRowIntoView(int row)
    {
        if (interaction?.VerticalTrackScroll is null || row < 0)
            return;

        var sv = interaction.VerticalTrackScroll;
        var trackHeight = TimelineRenderMetrics.TrackHeight;
        var top = row * trackHeight;
        var bottom = top + trackHeight;
        var offsetY = sv.Offset.Y;
        var viewportH = sv.Viewport.Height;
        if (viewportH <= 0)
            return;

        if (top < offsetY)
            sv.Offset = new Vector(sv.Offset.X, top);
        else if (bottom > offsetY + viewportH)
            sv.Offset = new Vector(sv.Offset.X, bottom - viewportH);
    }

    private void RebuildAll()
    {
        if (interaction is null)
            return;
        renderer.RebuildHeaders(interaction);
        renderer.RebuildMainCanvas(
            interaction,
            laneGesture.OnBackgroundPressed,
            laneGesture.OnBackgroundMoved,
            laneGesture.OnBackgroundReleased,
            laneGesture.OnDoubleTapped,
            laneGesture.OnLanePressed,
            clipGesture.OnTrimPressed,
            clipGesture.OnTrimMoved,
            clipGesture.OnTrimReleased,
            clipGesture.OnClipPressed,
            clipGesture.OnClipMoved,
            clipGesture.OnClipReleased);
        RebuildRuler();
        WireHeaderGestures();
        renderer.UpdateOverlays(interaction);
    }

    private void RebuildRuler()
    {
        if (interaction is null)
            return;
        renderer.RebuildRuler(
            interaction,
            rulerGesture.OnRulerPressed,
            rulerGesture.OnRulerMoved,
            rulerGesture.OnRulerReleased);
    }

    private void RefreshVirtualizedLanes()
    {
        if (interaction is null)
            return;
        renderer.RefreshVirtualizedLanes(
            interaction,
            laneGesture.OnLanePressed,
            laneGesture.OnDoubleTapped,
            clipGesture.OnTrimPressed,
            clipGesture.OnTrimMoved,
            clipGesture.OnTrimReleased,
            clipGesture.OnClipPressed,
            clipGesture.OnClipMoved,
            clipGesture.OnClipReleased);
    }

    private void WireHeaderGestures()
    {
        if (interaction is null)
            return;
        headerGesture.Attach(interaction);
        foreach (var kv in renderer.HeaderChrome)
        {
            kv.Value.PointerPressed += headerGesture.OnHeaderPressed;
            kv.Value.PointerMoved += headerGesture.OnHeaderMoved;
            kv.Value.PointerReleased += headerGesture.OnHeaderReleased;
        }
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (interaction is null)
            return;
        control.UpdateSelectedTrackIdBacking(selection.SelectedTrackId);
        renderer.ApplyTrackSelectionChrome(interaction);
        renderer.RefreshClipChrome(interaction);
        control.RaiseSelectionChanged();
        var track = project.Tracks.FirstOrDefault(t => t.Id == selection.SelectedTrackId);
        if (track != null)
            control.RaiseTrackSelected(track);
    }

    private void OnUndoStackStateChanged(object? sender, EventArgs e)
    {
        control.RaiseUndoRedoStateChanged();
        if (interaction is null)
            return;
        foreach (var clip in project.Clips)
            renderer.SyncClipVisual(interaction, clip);
        renderer.RefreshClipChrome(interaction);
    }

    private void OnPlayTick(object? sender, EventArgs e)
    {
        if (!control.IsPlaying)
            return;
        var next = control.PlayheadTime + playTimer.Interval.TotalSeconds;
        if (next >= control.Duration)
        {
            control.PlayheadTime = control.Duration;
            StopPlayback();
            return;
        }

        control.PlayheadTime = next;
    }

    private void CopyInternal()
    {
        if (interaction is null)
            return;
        interaction.Clipboard.Clear();
        foreach (var clip in GetSelectedClips())
            interaction.Clipboard.Add(TimelineClipOperations.ClonePrototype(clip));
    }

    private void DeleteSelectedClipsCore()
    {
        if (selection.SelectedClipIds.Count == 0)
            return;
        var removed = new List<TimelineClipItem>();
        for (var i = project.Clips.Count - 1; i >= 0; i--)
        {
            var clip = project.Clips[i];
            if (selection.IsClipSelected(clip.Id))
            {
                removed.Add(clip);
                project.Clips.RemoveAt(i);
                UnhookClip(clip);
            }
        }

        selection.ClearClipSelection();
        FullRebuild();
        if (removed.Count > 0)
            control.RaiseClipsRemoved(removed);
    }

    private string ResolvePasteTargetTrackId()
    {
        if (selection.SelectedClipIds.Count == 1)
        {
            var one = GetSelectedClips().FirstOrDefault();
            if (one != null)
                return one.TrackId;
        }

        if (!string.IsNullOrEmpty(selection.SelectedTrackId)
            && project.Tracks.Any(t => t.Id == selection.SelectedTrackId))
            return selection.SelectedTrackId!;

        return project.Tracks.FirstOrDefault()?.Id ?? "";
    }

    private void HookTracks(ObservableCollection<TimelineTrackItem> list)
    {
        foreach (var t in list)
            HookTrackItem(t);
        list.CollectionChanged += OnTracksCollectionChanged;
    }

    private void UnhookTracks(ObservableCollection<TimelineTrackItem> list)
    {
        list.CollectionChanged -= OnTracksCollectionChanged;
        foreach (var t in list.ToList())
            UnhookTrackItem(t);
    }

    private void HookClips(ObservableCollection<TimelineClipItem> list)
    {
        foreach (var c in list)
            HookClip(c);
        list.CollectionChanged += OnClipsCollectionChanged;
    }

    private void UnhookClips(ObservableCollection<TimelineClipItem> list)
    {
        list.CollectionChanged -= OnClipsCollectionChanged;
        foreach (var c in list.ToList())
            UnhookClip(c);
    }

    private void HookMarkers(ObservableCollection<TimelineMarkerItem> list)
    {
        foreach (var m in list)
            HookMarkerItem(m);
        list.CollectionChanged += OnMarkersCollectionChanged;
    }

    private void UnhookMarkers(ObservableCollection<TimelineMarkerItem> list)
    {
        list.CollectionChanged -= OnMarkersCollectionChanged;
        foreach (var m in list.ToList())
            UnhookMarkerItem(m);
    }

    private void OnTracksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<TimelineTrackItem> list)
        {
            if (e.OldItems != null)
            {
                foreach (TimelineTrackItem t in e.OldItems)
                    UnhookTrackItem(t);
            }

            if (e.NewItems != null)
            {
                foreach (TimelineTrackItem t in e.NewItems)
                    HookTrackItem(t);
            }
        }

        FullRebuild();
    }

    private void OnClipsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<TimelineClipItem> list)
        {
            if (e.OldItems != null)
            {
                foreach (TimelineClipItem c in e.OldItems)
                    UnhookClip(c);
            }

            if (e.NewItems != null)
            {
                foreach (TimelineClipItem c in e.NewItems)
                    HookClip(c);
            }
        }

        FullRebuild();
    }

    private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<TimelineMarkerItem> list)
        {
            if (e.OldItems != null)
            {
                foreach (TimelineMarkerItem m in e.OldItems)
                    UnhookMarkerItem(m);
            }

            if (e.NewItems != null)
            {
                foreach (TimelineMarkerItem m in e.NewItems)
                    HookMarkerItem(m);
            }
        }

        if (interaction != null)
            RebuildRuler();
    }

    private void HookTrackItem(TimelineTrackItem t)
    {
        if (!hookedTracks.Add(t))
            return;
        t.PropertyChanged += OnTrackItemPropertyChanged;
    }

    private void UnhookTrackItem(TimelineTrackItem t)
    {
        if (!hookedTracks.Remove(t))
            return;
        t.PropertyChanged -= OnTrackItemPropertyChanged;
    }

    private void OnTrackItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TimelineTrackItem.Name))
            FullRebuild();
    }

    private void HookClip(TimelineClipItem c)
    {
        if (!hookedClips.Add(c))
            return;
        c.PropertyChanged += OnClipPropertyChanged;
    }

    private void UnhookClip(TimelineClipItem c)
    {
        if (!hookedClips.Remove(c))
            return;
        c.PropertyChanged -= OnClipPropertyChanged;
    }

    private void OnClipPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (interaction is null || sender is not TimelineClipItem clip)
            return;
        if (e.PropertyName is nameof(TimelineClipItem.TrackId))
            renderer.SyncClipVisual(interaction, clip);
        else if (e.PropertyName is nameof(TimelineClipItem.StartTime) or nameof(TimelineClipItem.Duration)
                 or nameof(TimelineClipItem.Label))
            renderer.LayoutClip(interaction, clip);
    }

    private void HookMarkerItem(TimelineMarkerItem m)
    {
        if (!hookedMarkers.Add(m))
            return;
        m.PropertyChanged += OnMarkerItemPropertyChanged;
    }

    private void UnhookMarkerItem(TimelineMarkerItem m)
    {
        if (!hookedMarkers.Remove(m))
            return;
        m.PropertyChanged -= OnMarkerItemPropertyChanged;
    }

    private void OnMarkerItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (interaction is null)
            return;
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName is nameof(TimelineMarkerItem.Time) or nameof(TimelineMarkerItem.Label))
            RebuildRuler();
    }

    private void OnVerticalTrackScrollSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (Math.Abs(e.NewSize.Height - e.PreviousSize.Height) <= 0.5)
            return;
        FullRebuild();
    }

    private void AttachScrollSync()
    {
        if (interaction?.RulerScroll != null)
            interaction.RulerScroll.ScrollChanged += OnRulerScrollChanged;
        if (interaction?.MainScroll != null)
            interaction.MainScroll.ScrollChanged += OnMainScrollChanged;
        if (interaction?.VerticalTrackScroll != null)
            interaction.VerticalTrackScroll.ScrollChanged += OnVerticalTrackScrollChanged;
    }

    private void DetachScrollSync()
    {
        TeardownVerticalScrollLayoutRestoreListener();
        if (interaction?.RulerScroll != null)
            interaction.RulerScroll.ScrollChanged -= OnRulerScrollChanged;
        if (interaction?.MainScroll != null)
            interaction.MainScroll.ScrollChanged -= OnMainScrollChanged;
        if (interaction?.VerticalTrackScroll != null)
        {
            interaction.VerticalTrackScroll.ScrollChanged -= OnVerticalTrackScrollChanged;
            interaction.VerticalTrackScroll.SizeChanged -= OnVerticalTrackScrollSizeChanged;
        }
    }

    private void OnVerticalTrackScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (interaction is null || Math.Abs(e.OffsetDelta.Y) < 1e-6)
            return;
        RefreshVirtualizedLanes();
    }

    private void OnRulerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (syncScroll || interaction?.MainScroll == null || interaction.RulerScroll == null)
            return;
        if (Math.Abs(e.OffsetDelta.X) < 1e-6)
            return;
        syncScroll = true;
        interaction.MainScroll.Offset = new Vector(interaction.RulerScroll.Offset.X, interaction.MainScroll.Offset.Y);
        syncScroll = false;
    }

    private void OnMainScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (interaction?.MainScroll == null || interaction.RulerScroll == null || syncScroll)
            return;
        syncScroll = true;
        interaction.RulerScroll.Offset = new Vector(interaction.MainScroll.Offset.X, interaction.RulerScroll.Offset.Y);
        syncScroll = false;
    }

    private (Vector Vertical, Vector Main, Vector Ruler) CaptureScrollOffsets() =>
    (
        interaction?.VerticalTrackScroll?.Offset ?? default,
        interaction?.MainScroll?.Offset ?? default,
        interaction?.RulerScroll?.Offset ?? default
    );

    private static Vector ClampScrollOffset(ScrollViewer sv, Vector desired)
    {
        var maxX = Math.Max(0, sv.Extent.Width - sv.Viewport.Width);
        var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
        if (double.IsNaN(maxX) || double.IsInfinity(maxX))
            maxX = 0;
        if (double.IsNaN(maxY) || double.IsInfinity(maxY))
            maxY = 0;
        return new Vector(Math.Clamp(desired.X, 0, maxX), Math.Clamp(desired.Y, 0, maxY));
    }

    private void RestoreScrollOffsetsClamped((Vector Vertical, Vector Main, Vector Ruler) s)
    {
        if (interaction is null)
            return;
        syncScroll = true;
        try
        {
            if (interaction.VerticalTrackScroll != null)
            {
                var sv = interaction.VerticalTrackScroll;
                var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
                var desiredY = s.Vertical.Y;
                if (maxY >= 1 || desiredY <= 8)
                    sv.Offset = ClampScrollOffset(sv, new Vector(0, desiredY));
            }

            if (interaction.MainScroll != null)
                interaction.MainScroll.Offset = ClampScrollOffset(interaction.MainScroll, s.Main);
            if (interaction.RulerScroll != null)
                interaction.RulerScroll.Offset = ClampScrollOffset(interaction.RulerScroll, s.Ruler);
        }
        finally
        {
            syncScroll = false;
        }
    }

    private void TeardownVerticalScrollLayoutRestoreListener()
    {
        if (interaction?.VerticalTrackScroll != null && verticalScrollLayoutRestoreHandler != null)
            interaction.VerticalTrackScroll.LayoutUpdated -= verticalScrollLayoutRestoreHandler;
        verticalScrollLayoutRestoreHandler = null;
        verticalScrollLayoutRestoreAttempts = 0;
    }

    private void OnVerticalScrollLayoutRestore(object? sender, EventArgs e)
    {
        if (interaction?.VerticalTrackScroll == null || verticalScrollLayoutRestoreHandler == null)
            return;
        verticalScrollLayoutRestoreAttempts++;
        RestoreScrollOffsetsClamped(pendingLayoutScrollRestore);
        var sv = interaction.VerticalTrackScroll;
        var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
        var desiredY = pendingLayoutScrollRestore.Vertical.Y;
        if (maxY >= 1 || desiredY <= 8 || verticalScrollLayoutRestoreAttempts >= 48)
            TeardownVerticalScrollLayoutRestoreListener();
    }

    private void ApplyScrollRestore((Vector Vertical, Vector Main, Vector Ruler) s)
    {
        RestoreScrollOffsetsClamped(s);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Loaded);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Background);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Render);

        pendingLayoutScrollRestore = s;
        if (interaction?.VerticalTrackScroll != null && verticalScrollLayoutRestoreHandler == null)
        {
            verticalScrollLayoutRestoreAttempts = 0;
            verticalScrollLayoutRestoreHandler = OnVerticalScrollLayoutRestore;
            interaction.VerticalTrackScroll.LayoutUpdated += verticalScrollLayoutRestoreHandler;
        }
    }
}
