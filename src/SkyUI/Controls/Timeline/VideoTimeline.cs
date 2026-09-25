using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
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
    private string? _selectedTrackIdBacking;

    static VideoTimeline()
    {
        DurationProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnDurationChanged());
        PixelsPerSecondProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.OnPixelsPerSecondChanged());
        PlayheadTimeProperty.Changed.AddClassHandler<VideoTimeline>((s, _) =>
        {
            s._host.OnPlayheadChanged();
            s.PlayheadChanged?.Invoke(s, new TimelineTimeEventArgs(s.PlayheadTime));
        });
        IsPlayingProperty.Changed.AddClassHandler<VideoTimeline>((s, e) =>
            s._host.OnIsPlayingChanged(e.NewValue is true));
        TrackSelectionBrushProperty.Changed.AddClassHandler<VideoTimeline>((s, _) =>
            s._host.OnTrackSelectionBrushChanged());
    }

    public VideoTimeline()
    {
        _host = new Timeline.TimelineHost(this);
        Focusable = true;
        AddHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView, RoutingStrategies.Tunnel);
    }

    public event EventHandler<TimelineTimeEventArgs>? PlayheadChanged;
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

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _host.StopPlayback();
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

    private static double CoerceDuration(AvaloniaObject o, double v) =>
        TimelineCoordinateSystem.ClampDuration(v);

    private static double CoercePps(AvaloniaObject o, double v) =>
        TimelineCoordinateSystem.ClampPixelsPerSecond(v);

    private static double CoercePlayhead(AvaloniaObject o, double v)
    {
        if (o is not VideoTimeline timeline)
            return v < 0 ? 0 : v;
        return TimelineCoordinateSystem.ClampPlayhead(v, timeline.Duration);
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
