using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using SkyUI.Controls.Timeline.Composition;
using SkyUI.Controls.Timeline.Frame;
using SkyUI.Controls.Timeline.Integration;
using SkyUI.Controls.Timeline.OnionSkin;
using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Layout;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls;

/// <summary>
/// Multi-track video-style timeline host. Editing logic lives in
/// <see cref="Timeline.TimelineHost"/> (model, layout, rendering, commands, gestures).
/// </summary>
public sealed class VideoTimeline : TemplatedControl
{
    public const string PartRulerScroll = "PART_RulerScroll";
    public const string PartRulerCanvas = "PART_RulerCanvas";
    public const string PartVerticalTrackScroll = "PART_VerticalTrackScroll";
    public const string PartHeaderStack = "PART_HeaderStack";
    public const string PartMainScroll = "PART_MainScroll";
    public const string PartMainCanvas = "PART_MainCanvas";

    public static readonly StyledProperty<double> DurationProperty =
        AvaloniaProperty.Register<VideoTimeline, double>(nameof(Duration), 120, coerce: CoerceDuration);

    public static readonly StyledProperty<double> PixelsPerSecondProperty =
        AvaloniaProperty.Register<VideoTimeline, double>(nameof(PixelsPerSecond), 40, coerce: CoercePps);

    public static readonly StyledProperty<double> PlayheadTimeProperty =
        AvaloniaProperty.Register<VideoTimeline, double>(nameof(PlayheadTime), 0, coerce: CoercePlayhead);

    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<VideoTimeline, bool>(nameof(IsPlaying));

    public static readonly StyledProperty<double> FpsProperty =
        AvaloniaProperty.Register<VideoTimeline, double>(nameof(Fps), 24, coerce: CoerceFps);

    public static readonly StyledProperty<TimelineTimeUnit> TimeUnitProperty =
        AvaloniaProperty.Register<VideoTimeline, TimelineTimeUnit>(nameof(TimeUnit), TimelineTimeUnit.Frames);

    public static readonly StyledProperty<bool> LoopTimeRangeProperty =
        AvaloniaProperty.Register<VideoTimeline, bool>(nameof(LoopTimeRange), true);

    public static readonly StyledProperty<IBrush?> ClipLaneBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(ClipLaneBrush));

    public static readonly StyledProperty<IBrush?> ClipBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(ClipBrush));

    public static readonly StyledProperty<IBrush?> ClipSelectedBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(ClipSelectedBrush));

    public static readonly StyledProperty<IBrush?> PlayheadBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(PlayheadBrush));

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(SelectionBrush));

    public static readonly StyledProperty<IBrush?> RulerTickBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(RulerTickBrush));

    public static readonly StyledProperty<IBrush?> TrackSelectionBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(TrackSelectionBrush));

    public static readonly StyledProperty<IBrush?> LaneSeparatorBrushProperty =
        AvaloniaProperty.Register<VideoTimeline, IBrush?>(nameof(LaneSeparatorBrush));

    public static readonly DirectProperty<VideoTimeline, string?> SelectedTrackIdProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, string?>(
            nameof(SelectedTrackId),
            o => o._selectedTrackIdBacking,
            (o, v) => o._host.SetSelectedTrackId(v));

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineTrackItem>> TracksProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineTrackItem>>(
            nameof(Tracks),
            o => o.Tracks,
            (o, v) => o.SetTracks(v));

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineClipItem>> ClipsProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineClipItem>>(
            nameof(Clips),
            o => o.Clips,
            (o, v) => o.SetClips(v));

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineMarkerItem>> MarkersProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineMarkerItem>>(
            nameof(Markers),
            o => o.Markers,
            (o, v) => o.SetMarkers(v));

    private readonly Timeline.TimelineHost _host;
    private readonly TimelineMediaClock mediaClock = new();
    private readonly TimelinePreviewSynchronizer previewSynchronizer = new();
    private string? _selectedTrackIdBacking;
    private int _lastNotifiedPlayheadFrame = int.MinValue;

    public TimelineOnionSkinSettings OnionSkinSettings { get; } = new();

    public ITimelineClipThumbnailProvider? ClipThumbnailProvider { get; set; }

    static VideoTimeline()
    {
        DurationProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnDurationChanged());
        PixelsPerSecondProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnPixelsPerSecondChanged());
        PlayheadTimeProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.NotifyPlayheadChanged());
        FpsProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnTimeModeChanged());
        TimeUnitProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnTimeModeChanged());
        IsPlayingProperty.Changed.AddClassHandler<VideoTimeline>((s, e) =>
            s._host.OnIsPlayingChanged(e.NewValue is true));
        TrackSelectionBrushProperty.Changed.AddClassHandler<VideoTimeline>((s, _) =>
            s._host.OnTrackSelectionBrushChanged());
    }

    public VideoTimeline()
    {
        _host = new Timeline.TimelineHost(this);
        mediaClock.Bind(this);
        previewSynchronizer.Bind(this);
        previewSynchronizer.PreviewFrameChanged += (_, e) => PreviewFrameChanged?.Invoke(this, e);
        Focusable = true;
        AddHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView, RoutingStrategies.Tunnel);
    }

    public ITimelineMediaClock MediaClock => mediaClock;

    public TimelinePreviewSynchronizer PreviewSync => previewSynchronizer;

    public event EventHandler<TimelineTimeEventArgs>? PlayheadChanged;

    public event EventHandler<TimelineCurrentFrameEventArgs>? CurrentFrameChanged;

    public event EventHandler<TimelinePreviewFrameEventArgs>? PreviewFrameChanged;
    public event EventHandler<TimelineRangeEventArgs>? TimeRangeSelectionChanged;
    public event EventHandler<TimelineClipEventArgs>? ClipSelectionChanged;
    public event EventHandler<TimelineMarkerEventArgs>? MarkerAdded;
    public event EventHandler<TimelineClipsEventArgs>? ClipsRemoved;
    public event EventHandler<TimelineClipsEventArgs>? ClipsPasted;
    public event EventHandler<TimelineTrackReorderEventArgs>? TrackOrderChanged;
    public event EventHandler<TimelineTrackEventArgs>? TrackSelected;
    public event EventHandler<TimelineTrackEventArgs>? TrackRemoved;
    public event EventHandler<TimelineClipEditEventArgs>? ClipChanging;
    public event EventHandler<TimelineClipEditEventArgs>? ClipChanged;
    public event EventHandler<TimelineSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler? UndoRedoStateChanged;

    public TimelineProject Project => _host.Project;

    internal Timeline.TimelineHost Host => _host;

    public TimelineSnapSettings SnapSettings => _host.SnapSettings;

    public bool CanUndo => _host.CanUndo;

    public bool CanRedo => _host.CanRedo;

    public ObservableCollection<TimelineTrackItem> Tracks => _host.Tracks;

    public ObservableCollection<TimelineClipItem> Clips => _host.Clips;

    public ObservableCollection<TimelineMarkerItem> Markers => _host.Markers;

    public double Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double PixelsPerSecond
    {
        get => GetValue(PixelsPerSecondProperty);
        set => SetValue(PixelsPerSecondProperty, value);
    }

    public double PlayheadTime
    {
        get => GetValue(PlayheadTimeProperty);
        set => SetValue(PlayheadTimeProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public double Fps
    {
        get => GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    public TimelineTimeUnit TimeUnit
    {
        get => GetValue(TimeUnitProperty);
        set => SetValue(TimeUnitProperty, value);
    }

    /// <summary>When true and a time range is selected, playback loops within that range.</summary>
    public bool LoopTimeRange
    {
        get => GetValue(LoopTimeRangeProperty);
        set => SetValue(LoopTimeRangeProperty, value);
    }

    public int PlayheadFrame => _host.PlayheadFrame;

    public ITimelineClipFrameMapper ClipFrameMapper => _host.ClipFrameMapper;

    public TimelineSpriteFrameSampler SpriteFrameSampler => _host.SpriteFrameSampler;

    public TimelineClipFrameSpan GetClipFrameSpan(TimelineClipItem clip) =>
        _host.ClipFrameMapper.DescribeClip(clip);

    public int GetClipStartFrame(TimelineClipItem clip) => _host.ClipFrameMapper.ClipStartFrame(clip);

    public int GetClipDurationFrames(TimelineClipItem clip) => _host.ClipFrameMapper.ClipDurationFrames(clip);

    public IReadOnlyList<TimelineSpriteLayerFrameSample> SampleSpriteLayersAtPlayhead() =>
        SampleSpriteLayersAtFrame(PlayheadFrame);

    public IReadOnlyList<TimelineSpriteLayerFrameSample> SampleSpriteLayersAtFrame(int frame) =>
        _host.SpriteFrameSampler.SampleAtFrame(frame, Clips);

    public IReadOnlyList<TimelineOnionSkinFrameSample> SampleOnionSkinAtPlayhead() =>
        SampleOnionSkinAtFrame(PlayheadFrame);

    public IReadOnlyList<TimelineOnionSkinFrameSample> SampleOnionSkinAtFrame(int frame) =>
        _host.OnionSkinSampler.Sample(
            OnionSkinSettings,
            frame,
            _host.MaxPlayheadFrame,
            Clips);

    public TimelinePreviewFrameSnapshot CreatePreviewSnapshot() =>
        previewSynchronizer.CreateSnapshot(PlayheadFrame, PlayheadTime);

    public ObservableCollection<TimelineKeyframeItem> Keyframes => _host.Keyframes;

    public bool ExtendSelectedClipHoldFrames(int deltaFrames) =>
        _host.AdjustSelectedClipHoldFrames(deltaFrames);

    public bool SetOpacityKeyframeAtPlayhead() => _host.SetOpacityKeyframeAtPlayheadForSelectedTrack();

    public TimelineKeyframeItem SetKeyframeAtPlayhead(string propertyName, object? value)
    {
        var trackId = _host.SelectedTrackId ?? _host.Tracks.FirstOrDefault()?.Id ?? "";
        var time = _host.QuantizePlayhead(PlayheadTime);
        return _host.KeyframeEditor.SetKeyframe(trackId, propertyName, time, value);
    }

    public void StepPlayheadFrames(int frameDelta) => _host.StepPlayheadFrames(frameDelta);

    public void TogglePlayPause() => _host.TogglePlayPause();

    public IBrush? ClipLaneBrush
    {
        get => GetValue(ClipLaneBrushProperty);
        set => SetValue(ClipLaneBrushProperty, value);
    }

    public IBrush? ClipBrush
    {
        get => GetValue(ClipBrushProperty);
        set => SetValue(ClipBrushProperty, value);
    }

    public IBrush? ClipSelectedBrush
    {
        get => GetValue(ClipSelectedBrushProperty);
        set => SetValue(ClipSelectedBrushProperty, value);
    }

    public IBrush? PlayheadBrush
    {
        get => GetValue(PlayheadBrushProperty);
        set => SetValue(PlayheadBrushProperty, value);
    }

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public IBrush? RulerTickBrush
    {
        get => GetValue(RulerTickBrushProperty);
        set => SetValue(RulerTickBrushProperty, value);
    }

    public IBrush? TrackSelectionBrush
    {
        get => GetValue(TrackSelectionBrushProperty);
        set => SetValue(TrackSelectionBrushProperty, value);
    }

    public IBrush? LaneSeparatorBrush
    {
        get => GetValue(LaneSeparatorBrushProperty);
        set => SetValue(LaneSeparatorBrushProperty, value);
    }

    public string? SelectedTrackId => _host.SelectedTrackId;

    public bool HasTimeRangeSelection => _host.Interaction?.HasTimeRangeSelection ?? false;

    public TimelineTimeRange TimeRangeSelection =>
        _host.Interaction?.TimeRangeSelection ?? default;

    public void Undo() => _host.Undo();

    public void Redo() => _host.Redo();

    public void RegisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        _host.RegisterSnapTargetProvider(provider);

    public void UnregisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        _host.UnregisterSnapTargetProvider(provider);

    /// <summary>Plug in custom pointer/keyboard interactors (same pattern as SkyPullToRefresh).</summary>
    public void RegisterGestureInteractor(ITimelineGestureInteractor interactor) =>
        _host.RegisterGestureInteractor(interactor);

    public IReadOnlyList<TimelineClipItem> GetSelectedClips() => _host.GetSelectedClips();

    public TimelineClipItem? SplitClipAtPlayhead() => _host.SplitClipAtPlayhead(PlayheadTime);

    public void Play() => _host.Play();

    public void Stop() => _host.StopPlayback();

    public void Seek(double timeSeconds) => PlayheadTime = timeSeconds;

    public void ClearTimeRangeSelection() => _host.ClearTimeRangeSelection();

    public TimelineMarkerItem AddMarker(double timeSeconds, string label, object? tag = null) =>
        _host.AddMarker(timeSeconds, label, tag);

    public void AddTrack(string? name = null) => _host.AddTrack(name);

    public void AddSpriteTrack(string? name = null) => _host.AddTrack(name, TimelineTrackKind.Sprite);

    public void AddPropertyTrack(string? name = null) => _host.AddTrack(name, TimelineTrackKind.Property);

    public void RemoveTrack(TimelineTrackItem track) => _host.RemoveTrack(track);

    public void RemoveSelectedTrack() => _host.RemoveSelectedTrack();

    public void RemoveClip(TimelineClipItem clip) => _host.RemoveClip(clip);

    public void CopySelectionToClipboard() => _host.CopySelection();

    public void CutSelectionToClipboard() => _host.CutSelection();

    public void PasteClipboardAtPlayhead() => _host.PasteClipboardAtPlayhead(PlayheadTime);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _host.ApplyTemplate(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        mediaClock.Bind(this);
        previewSynchronizer.Bind(this);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _host.StopPlayback();
        previewSynchronizer.Unbind();
        mediaClock.Unbind();
        _host.DetachTemplate();
    }

    internal void UpdateSelectedTrackIdBacking(string? value) =>
        SetAndRaise(SelectedTrackIdProperty, ref _selectedTrackIdBacking, value);

    internal void SetPixelsPerSecondFromHost(double value) =>
        SetCurrentValue(PixelsPerSecondProperty, value);

    internal void RaiseUndoRedoStateChanged() => UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);

    internal void RaiseSelectionChanged() =>
        SelectionChanged?.Invoke(
            this,
            new TimelineSelectionChangedEventArgs(GetSelectedClips(), FindTrack(SelectedTrackId)));

    internal void RaiseClipEdit(TimelineClipItem clip, string editKind, bool changing)
    {
        var args = new TimelineClipEditEventArgs(clip, editKind);
        if (changing)
            ClipChanging?.Invoke(this, args);
        else
            ClipChanged?.Invoke(this, args);
    }

    internal void RaiseClipSelectionChanged(TimelineClipItem clip) =>
        ClipSelectionChanged?.Invoke(this, new TimelineClipEventArgs(clip));

    internal void RaiseTimeRangeChanged(TimelineTimeRange range) =>
        TimeRangeSelectionChanged?.Invoke(this, new TimelineRangeEventArgs(range));

    internal void RaiseTrackReordered(TimelineTrackItem track, int oldIndex, int newIndex) =>
        TrackOrderChanged?.Invoke(this, new TimelineTrackReorderEventArgs(track, oldIndex, newIndex));

    internal void RaiseTrackSelected(TimelineTrackItem track) =>
        TrackSelected?.Invoke(this, new TimelineTrackEventArgs(track));

    internal void RaiseTrackRemoved(TimelineTrackItem track) =>
        TrackRemoved?.Invoke(this, new TimelineTrackEventArgs(track));

    internal void RaiseMarkerAdded(TimelineMarkerItem marker) =>
        MarkerAdded?.Invoke(this, new TimelineMarkerEventArgs(marker));

    internal void RaiseClipsRemoved(IReadOnlyList<TimelineClipItem> clips) =>
        ClipsRemoved?.Invoke(this, new TimelineClipsEventArgs(clips));

    internal void RaiseClipsPasted(IReadOnlyList<TimelineClipItem> clips) =>
        ClipsPasted?.Invoke(this, new TimelineClipsEventArgs(clips));

    private void SetTracks(ObservableCollection<TimelineTrackItem>? value) =>
        _host.SetTracks(value ?? new ObservableCollection<TimelineTrackItem>());

    private void SetClips(ObservableCollection<TimelineClipItem>? value) =>
        _host.SetClips(value ?? new ObservableCollection<TimelineClipItem>());

    private void SetMarkers(ObservableCollection<TimelineMarkerItem>? value) =>
        _host.SetMarkers(value ?? new ObservableCollection<TimelineMarkerItem>());

    private void OnDurationChanged()
    {
        _host.SyncLayoutFromControl();
        _host.FullRebuild();
    }

    private void OnPixelsPerSecondChanged()
    {
        _host.SyncLayoutFromControl();
        _host.FullRebuild();
        PlayheadTime = PlayheadTime;
    }

    private void OnTimeModeChanged()
    {
        _host.SyncLayoutFromControl();
        _host.FullRebuild();
        _lastNotifiedPlayheadFrame = int.MinValue;
        SetCurrentValue(PlayheadTimeProperty, PlayheadTime);
    }

    private static double CoerceDuration(AvaloniaObject o, double v) =>
        TimelineCoordinateSystem.ClampDuration(v);

    private static double CoercePps(AvaloniaObject o, double v) =>
        TimelineCoordinateSystem.ClampPixelsPerSecond(v);

    private static double CoerceFps(AvaloniaObject o, double v) =>
        v < Timeline.Time.TimelineFrameQuantizer.MinFps
            ? Timeline.Time.TimelineFrameQuantizer.MinFps
            : v > Timeline.Time.TimelineFrameQuantizer.MaxFps
                ? Timeline.Time.TimelineFrameQuantizer.MaxFps
                : v;

    private static double CoercePlayhead(AvaloniaObject o, double v)
    {
        if (o is not VideoTimeline timeline)
            return v < 0 ? 0 : v;
        var clamped = TimelineCoordinateSystem.ClampPlayhead(v, timeline.Duration);
        return timeline._host.QuantizePlayhead(clamped);
    }

    private void NotifyPlayheadChanged()
    {
        _host.OnPlayheadChanged();
        var frame = _host.PlayheadFrame;
        PlayheadChanged?.Invoke(this, new TimelineTimeEventArgs(PlayheadTime, frame));
        if (frame == _lastNotifiedPlayheadFrame)
            return;
        _lastNotifiedPlayheadFrame = frame;
        CurrentFrameChanged?.Invoke(this, new TimelineCurrentFrameEventArgs(frame, PlayheadTime));
    }

    private void OnRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        if (e.Source is not Visual v || ReferenceEquals(v, this))
            return;
        if (v.GetVisualAncestors().Contains(this))
            e.Handled = true;
    }

    private TimelineTrackItem? FindTrack(string? id) =>
        string.IsNullOrEmpty(id) ? null : Tracks.FirstOrDefault(t => t.Id == id);
}
