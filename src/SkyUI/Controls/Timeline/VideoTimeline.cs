using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

/// <summary>
/// Multi-track video-style timeline: scrub/zoom, time-range selection, clip move/copy/cut/paste,
/// markers, playback, track selection highlight, remove track, and header drag-and-drop reorder (with drag threshold).
/// </summary>
/// <remarks>
/// <para><b>MVVM</b>: Bind <see cref="Tracks"/>, <see cref="Clips"/>, and <see cref="Markers"/> to the view model’s
/// <see cref="ObservableCollection{T}"/> instances (same references the VM mutates). <see cref="Duration"/>,
/// <see cref="PixelsPerSecond"/>, <see cref="PlayheadTime"/>, <see cref="IsPlaying"/>, and <see cref="SelectedTrackId"/>
/// are bindable. Use <see cref="PlayheadTime"/> two-way for playback state, or toggle <see cref="IsPlaying"/>.</para>
/// <para>Surface user edits via events (<see cref="TrackOrderChanged"/>, <see cref="MarkerAdded"/>, etc.) from code-behind
/// or behaviors; clip selection is kept inside the control—use <see cref="GetSelectedClips"/> and
/// <see cref="ClipSelectionChanged"/> to sync selection into the VM if needed.</para>
/// </remarks>
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

    public static readonly DirectProperty<VideoTimeline, string?> SelectedTrackIdProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, string?>(
            nameof(SelectedTrackId),
            o => o._selectedTrackId,
            (o, v) => o.SetSelectedTrackId(v));

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineTrackItem>> TracksProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineTrackItem>>(
            nameof(Tracks),
            o => o.Tracks,
            (o, v) => o.Tracks = v ?? new ObservableCollection<TimelineTrackItem>());

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineClipItem>> ClipsProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineClipItem>>(
            nameof(Clips),
            o => o.Clips,
            (o, v) => o.Clips = v ?? new ObservableCollection<TimelineClipItem>());

    public static readonly DirectProperty<VideoTimeline, ObservableCollection<TimelineMarkerItem>> MarkersProperty =
        AvaloniaProperty.RegisterDirect<VideoTimeline, ObservableCollection<TimelineMarkerItem>>(
            nameof(Markers),
            o => o.Markers,
            (o, v) => o.Markers = v ?? new ObservableCollection<TimelineMarkerItem>());

    private const double RulerHeight = 36;
    private const double TrackHeight = 44;
    private const double TrackReorderDragThreshold = 8;
    private const double MinClipDurationSeconds = 0.08;
    private const double TrimHandleWidth = 8;
    private const string BgTag = "__timeline_bg__";

    private ScrollViewer? _rulerScroll;
    private ScrollViewer? _verticalTrackScroll;
    private ScrollViewer? _mainScroll;
    private EventHandler? _verticalScrollLayoutRestoreHandler;
    private (Vector Vertical, Vector Main, Vector Ruler) _pendingLayoutScrollRestore;
    private int _verticalScrollLayoutRestoreAttempts;
    private Canvas? _rulerCanvas;
    private Canvas? _mainCanvas;
    private StackPanel? _headerStack;

    private readonly DispatcherTimer _playTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly HashSet<string> _selectedClipIds = new(StringComparer.Ordinal);
    private readonly List<TimelineClipItem> _clipboard = new();
    private readonly Dictionary<string, Control> _clipBorders = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Border> _clipBodyById = new(StringComparer.Ordinal);
    private readonly HashSet<TimelineClipItem> _hookedClips = new();
    private readonly HashSet<TimelineTrackItem> _hookedTracks = new();
    private readonly HashSet<TimelineMarkerItem> _hookedMarkers = new();

    private bool _syncScroll;
    private Rectangle? _selectionRect;
    private Line? _playheadLine;
    private TimelineClipItem? _dragClip;
    private TimelineClipItem? _trimClip;
    private bool _trimLeftEdge;
    private double _trimAnchorTime;
    private double _trimOrigStart;
    private double _trimOrigDuration;
    private double _dragClipPointerTimeOffset;
    private int _dragClipStartRow;
    private bool _rangeDrag;
    private bool _rangeActive;
    private Point _rangePressPoint;
    private double _rangeStartTime;
    private int? _headerReorderFrom;
    private Point _headerPressPoint;
    private bool _headerReorderDrag;

    private readonly Dictionary<string, Border> _headerChromeByTrackId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Border> _laneChromeByTrackId = new(StringComparer.Ordinal);
    private string? _selectedTrackId;

    private ObservableCollection<TimelineTrackItem> _tracks = new();
    private ObservableCollection<TimelineClipItem> _clips = new();
    private ObservableCollection<TimelineMarkerItem> _markers = new();

    private bool _hasTimeRange;
    private TimelineTimeRange _timeRange;

    static VideoTimeline()
    {
        DurationProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.FullRebuild());
        PixelsPerSecondProperty.Changed.AddClassHandler<VideoTimeline>((s, e) =>
        {
            s.FullRebuild();
            s.PlayheadTime = s.PlayheadTime; // coerce clamp
        });
        PlayheadTimeProperty.Changed.AddClassHandler<VideoTimeline>((s, _) =>
        {
            s.UpdateOverlays();
            s.PlayheadChanged?.Invoke(s, new TimelineTimeEventArgs(s.PlayheadTime));
        });
        IsPlayingProperty.Changed.AddClassHandler<VideoTimeline>((s, e) =>
        {
            if (e.NewValue is true)
                s._playTimer.Start();
            else
                s._playTimer.Stop();
        });
        TrackSelectionBrushProperty.Changed.AddClassHandler<VideoTimeline>((s, _) => s.ApplyTrackSelectionChrome());
    }

    public VideoTimeline()
    {
        HookTracks(_tracks);
        HookClips(_clips);
        HookMarkers(_markers);
        Focusable = true;
        _playTimer.Tick += OnPlayTick;
        AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Bubble);
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
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

    public ObservableCollection<TimelineTrackItem> Tracks
    {
        get => _tracks;
        set
        {
            if (ReferenceEquals(_tracks, value))
                return;
            UnhookTracks(_tracks);
            SetAndRaise(TracksProperty, ref _tracks, value);
            HookTracks(_tracks);
            FullRebuild();
        }
    }

    public ObservableCollection<TimelineClipItem> Clips
    {
        get => _clips;
        set
        {
            if (ReferenceEquals(_clips, value))
                return;
            UnhookClips(_clips);
            SetAndRaise(ClipsProperty, ref _clips, value);
            HookClips(_clips);
            FullRebuild();
        }
    }

    public ObservableCollection<TimelineMarkerItem> Markers
    {
        get => _markers;
        set
        {
            if (ReferenceEquals(_markers, value))
                return;
            UnhookMarkers(_markers);
            SetAndRaise(MarkersProperty, ref _markers, value);
            HookMarkers(_markers);
            FullRebuild();
        }
    }

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

    public string? SelectedTrackId
    {
        get => _selectedTrackId;
        set => SetSelectedTrackId(value);
    }

    private void SetSelectedTrackId(string? value)
    {
        if (_selectedTrackId == value)
            return;
        SetAndRaise(SelectedTrackIdProperty, ref _selectedTrackId, value);
        ApplyTrackSelectionChrome();
        var tr = FindTrack(_selectedTrackId);
        if (tr != null)
            TrackSelected?.Invoke(this, new TimelineTrackEventArgs(tr));
    }

    public bool HasTimeRangeSelection => _hasTimeRange;

    public TimelineTimeRange TimeRangeSelection => _timeRange;

    public IReadOnlyList<TimelineClipItem> GetSelectedClips() =>
        Clips.Where(c => _selectedClipIds.Contains(c.Id)).ToList();

    public void Play() => IsPlaying = true;

    public void Stop()
    {
        IsPlaying = false;
        _playTimer.Stop();
    }

    public void Seek(double timeSeconds) => PlayheadTime = timeSeconds;

    public void ClearTimeRangeSelection()
    {
        _hasTimeRange = false;
        UpdateOverlays();
        TimeRangeSelectionChanged?.Invoke(this, new TimelineRangeEventArgs(_timeRange));
    }

    public TimelineMarkerItem AddMarker(double timeSeconds, string label, object? tag = null)
    {
        var m = new TimelineMarkerItem { Time = timeSeconds, Label = label, Tag = tag };
        Markers.Add(m);
        MarkerAdded?.Invoke(this, new TimelineMarkerEventArgs(m));
        return m;
    }

    public void AddTrack(string? name = null)
    {
        var t = new TimelineTrackItem { Name = name ?? $"Track {Tracks.Count + 1}" };
        Tracks.Add(t);
        FullRebuild();
    }

    public void RemoveTrack(TimelineTrackItem track)
    {
        var id = track.Id;
        for (var i = Clips.Count - 1; i >= 0; i--)
        {
            if (Clips[i].TrackId == id)
                Clips.RemoveAt(i);
        }

        Tracks.Remove(track);
        if (_selectedTrackId == id)
            SetSelectedTrackId(null);
        TrackRemoved?.Invoke(this, new TimelineTrackEventArgs(track));
        FullRebuild();
    }

    /// <summary>Removes the currently selected track (if any) and its clips. Does nothing if a clip selection exists.</summary>
    public void RemoveSelectedTrack()
    {
        if (_selectedClipIds.Count > 0 || string.IsNullOrEmpty(_selectedTrackId))
            return;
        var tr = FindTrack(_selectedTrackId);
        if (tr == null)
            return;
        RemoveTrack(tr);
    }

    public void RemoveClip(TimelineClipItem clip)
    {
        Clips.Remove(clip);
        _selectedClipIds.Remove(clip.Id);
        FullRebuild();
        ClipSelectionChanged?.Invoke(this, new TimelineClipEventArgs(clip));
    }

    public void CopySelectionToClipboard() => CopyInternal();

    public void CutSelectionToClipboard() => CutInternal();

    public void PasteClipboardAtPlayhead()
    {
        if (_clipboard.Count == 0 || Tracks.Count == 0)
            return;
        var trackId = ResolvePasteTargetTrackId();
        var t0 = PlayheadTime;
        var pasted = new List<TimelineClipItem>();
        var cursor = t0;
        foreach (var proto in _clipboard)
        {
            var nc = new TimelineClipItem
            {
                TrackId = trackId,
                StartTime = cursor,
                Duration = proto.Duration,
                Label = proto.Label,
                Tag = proto.Tag,
            };
            cursor += proto.Duration + 0.05;
            Clips.Add(nc);
            pasted.Add(nc);
        }

        FullRebuild();
        ClipsPasted?.Invoke(this, new TimelineClipsEventArgs(pasted));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachScrollSync();

        _rulerScroll = e.NameScope.Find<ScrollViewer>(PartRulerScroll);
        _rulerCanvas = e.NameScope.Find<Canvas>(PartRulerCanvas);
        _verticalTrackScroll = e.NameScope.Find<ScrollViewer>(PartVerticalTrackScroll);
        _headerStack = e.NameScope.Find<StackPanel>(PartHeaderStack);
        _mainScroll = e.NameScope.Find<ScrollViewer>(PartMainScroll);
        _mainCanvas = e.NameScope.Find<Canvas>(PartMainCanvas);

        if (_verticalTrackScroll != null)
            _verticalTrackScroll.SizeChanged += OnVerticalTrackScrollSizeChanged;

        AttachScrollSync();
        UnhookTracks(Tracks);
        UnhookClips(Clips);
        UnhookMarkers(Markers);
        HookTracks(Tracks);
        HookClips(Clips);
        HookMarkers(Markers);
        FullRebuild();
    }

    private void DetachScrollSync()
    {
        TeardownVerticalScrollLayoutRestoreListener();
        if (_rulerScroll != null)
            _rulerScroll.ScrollChanged -= OnRulerScrollChanged;
        if (_mainScroll != null)
            _mainScroll.ScrollChanged -= OnMainScrollChanged;
        if (_verticalTrackScroll != null)
            _verticalTrackScroll.SizeChanged -= OnVerticalTrackScrollSizeChanged;
    }

    private void AttachScrollSync()
    {
        if (_rulerScroll != null)
            _rulerScroll.ScrollChanged += OnRulerScrollChanged;
        if (_mainScroll != null)
            _mainScroll.ScrollChanged += OnMainScrollChanged;
    }

    private void OnRulerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_syncScroll || _mainScroll == null || _rulerScroll == null)
            return;
        if (Math.Abs(e.OffsetDelta.X) < 1e-6)
            return;
        _syncScroll = true;
        _mainScroll.Offset = new Vector(_rulerScroll.Offset.X, _mainScroll.Offset.Y);
        _syncScroll = false;
    }

    private void OnMainScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_mainScroll == null)
            return;
        if (_rulerScroll != null && !_syncScroll)
        {
            _syncScroll = true;
            _rulerScroll.Offset = new Vector(_mainScroll.Offset.X, _rulerScroll.Offset.Y);
            _syncScroll = false;
        }
    }

    private void OnPlayTick(object? sender, EventArgs e)
    {
        if (!IsPlaying)
            return;
        var dt = _playTimer.Interval.TotalSeconds;
        var next = PlayheadTime + dt;
        if (next >= Duration)
        {
            PlayheadTime = Duration;
            Stop();
            return;
        }

        PlayheadTime = next;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_mainScroll == null || _mainCanvas == null)
            return;
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return;
        e.Handled = true;
        var old = PixelsPerSecond;
        var factor = e.Delta.Y > 0 ? 1.12 : 1 / 1.12;
        var pos = e.GetPosition(_mainCanvas);
        var contentX = _mainScroll.Offset.X + pos.X;
        var time = contentX / old;
        var neu = CoercePps(this, Math.Clamp(old * factor, 6, 640));
        SetCurrentValue(PixelsPerSecondProperty, neu);
        var newContentX = time * neu;
        var newOffX = newContentX - pos.X;
        _mainScroll.Offset = new Vector(Math.Max(0, newOffX), _mainScroll.Offset.Y);
        if (_rulerScroll != null)
            _rulerScroll.Offset = new Vector(_mainScroll.Offset.X, _rulerScroll.Offset.Y);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsFocused && !Focus())
            return;
        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            if (_selectedClipIds.Count > 0)
                DeleteSelectedClips();
            else
                RemoveSelectedTrack();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.C)
        {
            CopyInternal();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.X)
        {
            CutInternal();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.V)
        {
            PasteClipboardAtPlayhead();
            e.Handled = true;
        }
    }

    private static double CoerceDuration(AvaloniaObject o, double v) => v < 0.01 ? 0.01 : v;

    private static double CoercePps(AvaloniaObject o, double v) => Math.Clamp(v, 6, 640);

    private static double CoercePlayhead(AvaloniaObject o, double v)
    {
        if (o is not VideoTimeline t)
            return v < 0 ? 0 : v;
        var d = t.Duration;
        if (v < 0)
            return 0;
        return v > d ? d : v;
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
        if (sender is not ObservableCollection<TimelineTrackItem> list)
        {
            FullRebuild();
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                foreach (var t in _hookedTracks.ToArray())
                    UnhookTrackItem(t);
                foreach (var t in list)
                    HookTrackItem(t);
                break;
            default:
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

                break;
        }

        FullRebuild();
    }

    private void HookTrackItem(TimelineTrackItem t)
    {
        if (!_hookedTracks.Add(t))
            return;
        t.PropertyChanged += OnTrackItemPropertyChanged;
    }

    private void UnhookTrackItem(TimelineTrackItem t)
    {
        if (!_hookedTracks.Remove(t))
            return;
        t.PropertyChanged -= OnTrackItemPropertyChanged;
    }

    private void OnTrackItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(TimelineTrackItem.Name))
            RebuildHeaders();
    }

    private void OnClipsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ObservableCollection<TimelineClipItem> list)
        {
            FullRebuild();
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                foreach (var c in _hookedClips.ToArray())
                    UnhookClip(c);
                foreach (var c in list)
                    HookClip(c);
                break;
            default:
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

                break;
        }

        FullRebuild();
    }

    private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ObservableCollection<TimelineMarkerItem> list)
        {
            RebuildRuler();
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                foreach (var m in _hookedMarkers.ToArray())
                    UnhookMarkerItem(m);
                foreach (var m in list)
                    HookMarkerItem(m);
                break;
            default:
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

                break;
        }

        RebuildRuler();
    }

    private void HookMarkerItem(TimelineMarkerItem m)
    {
        if (!_hookedMarkers.Add(m))
            return;
        m.PropertyChanged += OnMarkerItemPropertyChanged;
    }

    private void UnhookMarkerItem(TimelineMarkerItem m)
    {
        if (!_hookedMarkers.Remove(m))
            return;
        m.PropertyChanged -= OnMarkerItemPropertyChanged;
    }

    private void OnMarkerItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName is nameof(TimelineMarkerItem.Time) or nameof(TimelineMarkerItem.Label))
            RebuildRuler();
    }

    private void HookClip(TimelineClipItem c)
    {
        if (!_hookedClips.Add(c))
            return;
        c.PropertyChanged += OnClipPropertyChanged;
    }

    private void UnhookClip(TimelineClipItem c)
    {
        if (!_hookedClips.Remove(c))
            return;
        c.PropertyChanged -= OnClipPropertyChanged;
    }

    private void OnClipPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TimelineClipItem c)
            return;
        if (e.PropertyName is nameof(TimelineClipItem.StartTime) or nameof(TimelineClipItem.Duration)
            or nameof(TimelineClipItem.TrackId) or nameof(TimelineClipItem.Label))
            LayoutClip(c);
    }

    private void FullRebuild()
    {
        var scroll = CaptureScrollOffsets();
        RebuildHeaders();
        RebuildMainCanvas();
        RebuildRuler();
        UpdateOverlays();
        ApplyScrollRestore(scroll);
    }

    private void OnRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        if (e.Source is not Visual v || ReferenceEquals(v, this))
            return;
        if (v.GetVisualAncestors().Contains(this))
            e.Handled = true;
    }

    private (Vector Vertical, Vector Main, Vector Ruler) CaptureScrollOffsets() =>
    (
        _verticalTrackScroll?.Offset ?? default,
        _mainScroll?.Offset ?? default,
        _rulerScroll?.Offset ?? default
    );

    private static Vector ClampScrollOffset(ScrollViewer sv, Vector desired)
    {
        var maxX = Math.Max(0, sv.Extent.Width - sv.Viewport.Width);
        var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
        if (double.IsNaN(maxX) || double.IsInfinity(maxX))
            maxX = 0;
        if (double.IsNaN(maxY) || double.IsInfinity(maxY))
            maxY = 0;
        return new Vector(
            Math.Clamp(desired.X, 0, maxX),
            Math.Clamp(desired.Y, 0, maxY));
    }

    private void RestoreScrollOffsetsClamped((Vector Vertical, Vector Main, Vector Ruler) s)
    {
        _syncScroll = true;
        try
        {
            if (_verticalTrackScroll != null)
            {
                var sv = _verticalTrackScroll;
                var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
                var desiredY = s.Vertical.Y;
                // Before content extent is measured, maxY can read as 0 while the captured offset is large;
                // clamping would snap the thumb to the top. Skip until layout catches up (deferred restores).
                if (maxY >= 1 || desiredY <= 8)
                    sv.Offset = ClampScrollOffset(sv, new Vector(0, desiredY));
            }
            if (_mainScroll != null)
                _mainScroll.Offset = ClampScrollOffset(_mainScroll, s.Main);
            if (_rulerScroll != null)
                _rulerScroll.Offset = ClampScrollOffset(_rulerScroll, s.Ruler);
        }
        finally
        {
            _syncScroll = false;
        }
    }

    private void TeardownVerticalScrollLayoutRestoreListener()
    {
        if (_verticalTrackScroll != null && _verticalScrollLayoutRestoreHandler != null)
            _verticalTrackScroll.LayoutUpdated -= _verticalScrollLayoutRestoreHandler;
        _verticalScrollLayoutRestoreHandler = null;
        _verticalScrollLayoutRestoreAttempts = 0;
    }

    private void OnVerticalScrollLayoutRestore(object? sender, EventArgs e)
    {
        if (_verticalTrackScroll == null || _verticalScrollLayoutRestoreHandler == null)
            return;
        _verticalScrollLayoutRestoreAttempts++;
        RestoreScrollOffsetsClamped(_pendingLayoutScrollRestore);
        var sv = _verticalTrackScroll;
        var maxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
        var desiredY = _pendingLayoutScrollRestore.Vertical.Y;
        if (maxY >= 1 || desiredY <= 8 || _verticalScrollLayoutRestoreAttempts >= 48)
            TeardownVerticalScrollLayoutRestoreListener();
    }

    private void ApplyScrollRestore((Vector Vertical, Vector Main, Vector Ruler) s)
    {
        RestoreScrollOffsetsClamped(s);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Loaded);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Background);
        Dispatcher.UIThread.Post(() => RestoreScrollOffsetsClamped(s), DispatcherPriority.Render);

        _pendingLayoutScrollRestore = s;
        if (_verticalTrackScroll != null && _verticalScrollLayoutRestoreHandler == null)
        {
            _verticalScrollLayoutRestoreAttempts = 0;
            _verticalScrollLayoutRestoreHandler = OnVerticalScrollLayoutRestore;
            _verticalTrackScroll.LayoutUpdated += _verticalScrollLayoutRestoreHandler;
        }
    }

    private void RebuildHeaders()
    {
        if (_headerStack == null)
            return;
        _headerChromeByTrackId.Clear();
        _headerStack.Children.Clear();
        _headerStack.MinHeight = MeasureTrackContentHeight();
        foreach (var t in Tracks)
        {
            var border = new Border
            {
                Height = TrackHeight,
                Padding = new Thickness(10, 0, 8, 0),
                Background = ClipLaneBrush,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new TextBlock
                {
                    Text = t.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Foreground,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                Tag = t.Id,
            };
            border.PointerPressed += OnHeaderPointerPressed;
            border.PointerMoved += OnHeaderPointerMoved;
            border.PointerReleased += OnHeaderPointerReleased;
            _headerStack.Children.Add(border);
            _headerChromeByTrackId[t.Id] = border;
        }

        ApplyTrackSelectionChrome();
    }

    private void RebuildMainCanvas()
    {
        if (_mainCanvas == null || _mainScroll == null)
            return;
        foreach (var c in Clips)
        {
            if (!_hookedClips.Contains(c))
                HookClip(c);
        }

        _mainCanvas.Children.Clear();
        _clipBorders.Clear();
        _clipBodyById.Clear();
        _laneChromeByTrackId.Clear();

        var contentW = Math.Max(ContentWidth(), _mainScroll.Viewport.Width);
        var contentH = MeasureTrackContentHeight();
        _mainCanvas.Width = contentW;
        _mainCanvas.Height = contentH;

        var bg = new Rectangle
        {
            Width = contentW,
            Height = contentH,
            Fill = Brushes.Transparent,
            Tag = BgTag,
        };
        bg.PointerPressed += OnMainBackgroundPressed;
        bg.PointerMoved += OnMainBackgroundMoved;
        bg.PointerReleased += OnMainBackgroundReleased;
        bg.DoubleTapped += OnMainDoubleTapped;
        _mainCanvas.Children.Add(bg);

        for (var i = 0; i < Tracks.Count; i++)
        {
            var t = Tracks[i];
            var y = i * TrackHeight;
            var lane = new Border
            {
                Height = TrackHeight,
                Width = contentW,
                Background = ClipLaneBrush,
                BorderBrush = new SolidColorBrush(Color.Parse("#333333")),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Tag = t.Id,
                Cursor = new Cursor(StandardCursorType.Arrow),
            };
            lane.PointerPressed += OnLanePointerPressed;
            lane.DoubleTapped += OnMainDoubleTapped;
            Canvas.SetLeft(lane, 0);
            Canvas.SetTop(lane, y);
            _mainCanvas.Children.Add(lane);
            _laneChromeByTrackId[t.Id] = lane;
        }

        foreach (var clip in Clips)
            AddClipVisual(clip);

        _selectionRect = new Rectangle
        {
            Fill = SelectionBrush ?? new SolidColorBrush(Color.FromArgb(80, 0, 255, 136)),
            IsHitTestVisible = false,
            IsVisible = false,
        };
        _mainCanvas.Children.Add(_selectionRect);

        _playheadLine = new Line
        {
            Stroke = PlayheadBrush ?? Brushes.LimeGreen,
            StrokeThickness = 2,
            IsHitTestVisible = false,
        };
        _mainCanvas.Children.Add(_playheadLine);
        ApplyTrackSelectionChrome();
        UpdateOverlays();
    }

    private void AddClipVisual(TimelineClipItem clip)
    {
        if (_mainCanvas == null)
            return;
        var row = TrackRowIndex(clip.TrackId);
        if (row < 0)
            return;

        var gripBrush = new SolidColorBrush(Color.FromArgb(130, 255, 255, 255));
        var left = new Border
        {
            Background = gripBrush,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
        };
        left.PointerPressed += (_, e) => OnTrimPressed(clip, leftEdge: true, left, e);
        left.PointerMoved += OnTrimPointerMoved;
        left.PointerReleased += OnTrimPointerReleased;

        var body = new Border
        {
            CornerRadius = new CornerRadius(4),
            Background = _selectedClipIds.Contains(clip.Id) ? ClipSelectedBrush ?? ClipBrush : ClipBrush,
            BorderBrush = new SolidColorBrush(Color.Parse("#333333")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4, 2, 4, 2),
            Child = new TextBlock
            {
                Text = string.IsNullOrEmpty(clip.Label) ? "Clip" : clip.Label,
                Foreground = Foreground,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
        };
        body.Tag = clip.Id;
        body.PointerPressed += OnClipPointerPressed;
        body.PointerMoved += OnClipPointerMoved;
        body.PointerReleased += OnClipPointerReleased;

        var right = new Border
        {
            Background = gripBrush,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
        };
        right.PointerPressed += (_, e) => OnTrimPressed(clip, leftEdge: false, right, e);
        right.PointerMoved += OnTrimPointerMoved;
        right.PointerReleased += OnTrimPointerReleased;

        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TrimHandleWidth) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TrimHandleWidth) });
        Grid.SetColumn(left, 0);
        Grid.SetColumn(body, 1);
        Grid.SetColumn(right, 2);
        root.Children.Add(left);
        root.Children.Add(body);
        root.Children.Add(right);

        _mainCanvas.Children.Add(root);
        _clipBorders[clip.Id] = root;
        _clipBodyById[clip.Id] = body;
        LayoutClip(clip);
    }

    private void OnTrimPressed(TimelineClipItem clip, bool leftEdge, IInputElement captureTo, PointerPressedEventArgs e)
    {
        SelectSingle(clip);
        _trimClip = clip;
        _trimLeftEdge = leftEdge;
        _trimOrigStart = clip.StartTime;
        _trimOrigDuration = clip.Duration;
        _trimAnchorTime = TimeFromMainPointer(e);
        e.Pointer.Capture(captureTo);
        e.Handled = true;
    }

    private void OnTrimPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_trimClip == null || sender is not Border b)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, b))
            return;
        var t = TimeFromMainPointer(e);
        var delta = t - _trimAnchorTime;
        if (_trimLeftEdge)
        {
            var end = _trimOrigStart + _trimOrigDuration;
            var newStart = Math.Clamp(_trimOrigStart + delta, 0, end - MinClipDurationSeconds);
            _trimClip.StartTime = newStart;
            _trimClip.Duration = end - newStart;
        }
        else
        {
            var newEnd = Math.Clamp(_trimOrigStart + _trimOrigDuration + delta, _trimOrigStart + MinClipDurationSeconds, Duration);
            _trimClip.Duration = newEnd - _trimOrigStart;
        }

        LayoutClip(_trimClip);
        e.Handled = true;
    }

    private void OnTrimPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _trimClip = null;
        e.Pointer.Capture(null);
    }

    private void RebuildRuler()
    {
        if (_rulerCanvas == null || _rulerScroll == null)
            return;
        var scroll = CaptureScrollOffsets();
        _rulerCanvas.Children.Clear();
        var w = Math.Max(ContentWidth(), _rulerScroll.Viewport.Width);
        _rulerCanvas.Width = w;
        _rulerCanvas.Height = RulerHeight;

        var tickBrush = RulerTickBrush ?? new SolidColorBrush(Color.Parse("#555555"));
        var pps = PixelsPerSecond;
        var step = NiceTickStep(Duration, w);
        for (var t = 0.0; t <= Duration + 1e-6; t += step)
        {
            var x = t * pps;
            var line = new Line
            {
                StartPoint = new Point(x, RulerHeight - 10),
                EndPoint = new Point(x, RulerHeight),
                Stroke = tickBrush,
                StrokeThickness = 1,
                IsHitTestVisible = false,
            };
            _rulerCanvas.Children.Add(line);
            var tb = new TextBlock
            {
                Text = FormatTime(t),
                FontSize = 10,
                Foreground = Foreground,
            };
            Canvas.SetLeft(tb, x + 2);
            Canvas.SetTop(tb, 2);
            _rulerCanvas.Children.Add(tb);
        }

        foreach (var m in Markers)
        {
            var x = m.Time * pps;
            var tri = new Polygon
            {
                Points = new Points
                {
                    new Point(x - 5, RulerHeight - 2),
                    new Point(x + 5, RulerHeight - 2),
                    new Point(x, RulerHeight - 12),
                },
                Fill = PlayheadBrush ?? Brushes.LimeGreen,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                IsHitTestVisible = false,
            };
            _rulerCanvas.Children.Add(tri);
        }

        var hit = new Rectangle
        {
            Width = w,
            Height = RulerHeight,
            Fill = Brushes.Transparent,
        };
        hit.PointerPressed += OnRulerPressed;
        hit.PointerMoved += OnRulerMoved;
        hit.PointerReleased += OnRulerReleased;
        _rulerCanvas.Children.Insert(0, hit);
        ApplyScrollRestore(scroll);
    }

    private static double NiceTickStep(double duration, double widthPixels)
    {
        if (widthPixels < 120)
            return Math.Max(1, duration / 4);
        var targetLabels = Math.Max(4, widthPixels / 90);
        var raw = duration / targetLabels;
        var pow = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(raw, 0.001))));
        var n = raw / pow;
        double m = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
        return m * pow;
    }

    private static string FormatTime(double seconds)
    {
        var s = (int)Math.Floor(seconds % 60);
        var m = (int)Math.Floor(seconds / 60) % 60;
        var h = (int)Math.Floor(seconds / 3600);
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m}:{s:00}";
    }

    private double ContentWidth() => Math.Max(Duration * PixelsPerSecond, 400);

    private double MeasureTrackContentHeight()
    {
        var baseH = Math.Max(Tracks.Count, 1) * TrackHeight;
        var vh = 0.0;
        if (_verticalTrackScroll != null)
        {
            vh = _verticalTrackScroll.Viewport.Height;
            if (double.IsNaN(vh) || vh < 1)
                vh = _verticalTrackScroll.Bounds.Height;
        }

        return Math.Max(baseH, vh > 0 ? vh : baseH);
    }

    private void OnVerticalTrackScrollSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (Math.Abs(e.NewSize.Height - e.PreviousSize.Height) <= 0.5)
            return;
        var scroll = CaptureScrollOffsets();
        RebuildHeaders();
        RebuildMainCanvas();
        UpdateOverlays();
        ApplyScrollRestore(scroll);
    }

    private TimelineTrackItem? FindTrack(string? id) =>
        string.IsNullOrEmpty(id) ? null : Tracks.FirstOrDefault(t => t.Id == id);

    private void ApplyTrackSelectionChrome()
    {
        var hi = TrackSelectionBrush ?? PlayheadBrush ?? Brushes.LimeGreen;
        var sep = new SolidColorBrush(Color.Parse("#333333"));
        foreach (var kv in _headerChromeByTrackId)
        {
            var sel = kv.Key == _selectedTrackId;
            kv.Value.BorderBrush = sel ? hi : Brushes.Transparent;
            kv.Value.BorderThickness = sel ? new Thickness(2) : new Thickness(0, 0, 0, 1);
        }

        foreach (var kv in _laneChromeByTrackId)
        {
            var sel = kv.Key == _selectedTrackId;
            kv.Value.BorderBrush = sel ? hi : sep;
            kv.Value.BorderThickness = sel ? new Thickness(2) : new Thickness(0, 0, 0, 1);
        }
    }

    private int TrackRowIndex(string trackId)
    {
        for (var i = 0; i < Tracks.Count; i++)
        {
            if (Tracks[i].Id == trackId)
                return i;
        }

        return -1;
    }

    private void LayoutClip(TimelineClipItem clip)
    {
        if (!_clipBorders.TryGetValue(clip.Id, out var b))
            return;
        var row = TrackRowIndex(clip.TrackId);
        if (row < 0)
            return;
        var left = clip.StartTime * PixelsPerSecond;
        var width = Math.Max(10, clip.Duration * PixelsPerSecond);
        Canvas.SetLeft(b, left);
        Canvas.SetTop(b, row * TrackHeight + 5);
        b.Width = width;
        b.Height = TrackHeight - 10;
    }

    private void UpdateOverlays()
    {
        if (_mainCanvas == null || _playheadLine == null || _selectionRect == null)
            return;
        var h = _mainCanvas.Height;
        if (double.IsNaN(h) || h <= 0)
            h = MeasureTrackContentHeight();
        var x = PlayheadTime * PixelsPerSecond;
        _playheadLine.StartPoint = new Point(x, 0);
        _playheadLine.EndPoint = new Point(x, h);

        if (_hasTimeRange)
        {
            _selectionRect.IsVisible = true;
            var x0 = _timeRange.Min * PixelsPerSecond;
            var x1 = _timeRange.Max * PixelsPerSecond;
            Canvas.SetLeft(_selectionRect, x0);
            Canvas.SetTop(_selectionRect, 0);
            _selectionRect.Width = Math.Max(1, x1 - x0);
            _selectionRect.Height = h;
        }
        else
        {
            _selectionRect.IsVisible = false;
        }
    }

    private double TimeFromMainPointer(RoutedEventArgs e)
    {
        if (_mainCanvas == null || _mainScroll == null)
            return 0;
        var p = e is PointerEventArgs pe
            ? pe.GetPosition(_mainCanvas)
            : e is TappedEventArgs te
                ? te.GetPosition(_mainCanvas)
                : default;
        return (p.X + _mainScroll.Offset.X) / PixelsPerSecond;
    }

    private void OnRulerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_rulerCanvas == null || _rulerScroll == null)
            return;
        var p = e.GetPosition(_rulerCanvas);
        var t = (p.X + _rulerScroll.Offset.X) / PixelsPerSecond;
        PlayheadTime = t;
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void OnRulerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
    }

    private void OnRulerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Pointer.Captured != sender || _rulerCanvas == null || _rulerScroll == null)
            return;
        var p = e.GetPosition(_rulerCanvas);
        var t = (p.X + _rulerScroll.Offset.X) / PixelsPerSecond;
        PlayheadTime = t;
    }

    private void OnLanePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border b || b.Tag is not string tid)
            return;
        SetSelectedTrackId(tid);
        ClearClipSelection();
        PlayheadTime = TimeFromMainPointer(e);
        e.Handled = true;
    }

    private void OnMainDoubleTapped(object? sender, TappedEventArgs e)
    {
        var t = TimeFromMainPointer(e);
        var label = $"Note @ {FormatTime(t)}";
        AddMarker(t, label);
        e.Handled = true;
    }

    private void OnMainBackgroundPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_mainCanvas == null)
            return;
        var py = e.GetPosition(_mainCanvas).Y;
        var row = (int)Math.Floor(py / TrackHeight);
        row = Math.Clamp(row, 0, Math.Max(0, Tracks.Count - 1));
        if (Tracks.Count > 0)
            SetSelectedTrackId(Tracks[row].Id);

        _rangeDrag = true;
        _rangeActive = false;
        _rangePressPoint = e.GetPosition(_mainCanvas);
        _rangeStartTime = TimeFromMainPointer(e);
        ClearClipSelection();
        PlayheadTime = _rangeStartTime;
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void OnMainBackgroundMoved(object? sender, PointerEventArgs e)
    {
        if (!_rangeDrag || _mainCanvas == null)
            return;
        var p = e.GetPosition(_mainCanvas);
        if (!_rangeActive)
        {
            var dx = p.X - _rangePressPoint.X;
            var dy = p.Y - _rangePressPoint.Y;
            if (dx * dx + dy * dy < 9)
                return;
            _rangeActive = true;
            _hasTimeRange = true;
            _timeRange = new TimelineTimeRange(_rangeStartTime, _rangeStartTime);
            TimeRangeSelectionChanged?.Invoke(this, new TimelineRangeEventArgs(_timeRange));
        }

        var t = TimeFromMainPointer(e);
        _timeRange = new TimelineTimeRange(_rangeStartTime, t);
        UpdateOverlays();
        TimeRangeSelectionChanged?.Invoke(this, new TimelineRangeEventArgs(_timeRange));
    }

    private void OnMainBackgroundReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_rangeDrag)
        {
            _rangeDrag = false;
            if (!_rangeActive)
                ClearTimeRangeSelection();
            e.Pointer.Capture(null);
        }
    }

    private void OnClipPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border b || b.Tag is not string id)
            return;
        var clip = Clips.FirstOrDefault(c => c.Id == id);
        if (clip == null)
            return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            ToggleSelect(clip);
        else
            SelectSingle(clip);

        _dragClip = clip;
        var t = TimeFromMainPointer(e);
        _dragClipPointerTimeOffset = t - clip.StartTime;
        _dragClipStartRow = TrackRowIndex(clip.TrackId);
        PlayheadTime = clip.StartTime;
        e.Pointer.Capture(b);
        e.Handled = true;
    }

    private void OnClipPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_trimClip != null)
            return;
        if (_dragClip == null || sender is not Border b)
            return;
        var t = TimeFromMainPointer(e) - _dragClipPointerTimeOffset;
        t = Math.Clamp(t, 0, Math.Max(0, Duration - _dragClip.Duration));
        _dragClip.StartTime = t;

        if (_mainCanvas != null)
        {
            var p = e.GetPosition(_mainCanvas);
            var localY = p.Y;
            var row = (int)Math.Floor(localY / TrackHeight);
            row = Math.Clamp(row, 0, Math.Max(0, Tracks.Count - 1));
            if (row >= 0 && row < Tracks.Count)
                _dragClip.TrackId = Tracks[row].Id;
        }

        LayoutClip(_dragClip);
        e.Handled = true;
    }

    private void OnClipPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragClip != null)
        {
            e.Pointer.Capture(null);
            _dragClip = null;
        }
    }

    private void SelectSingle(TimelineClipItem clip)
    {
        SetSelectedTrackId(null);
        _selectedClipIds.Clear();
        _selectedClipIds.Add(clip.Id);
        RefreshClipChrome();
        ClipSelectionChanged?.Invoke(this, new TimelineClipEventArgs(clip));
    }

    private void ToggleSelect(TimelineClipItem clip)
    {
        if (!_selectedClipIds.Add(clip.Id))
            _selectedClipIds.Remove(clip.Id);
        RefreshClipChrome();
        ClipSelectionChanged?.Invoke(this, new TimelineClipEventArgs(clip));
    }

    private void ClearClipSelection()
    {
        if (_selectedClipIds.Count == 0)
            return;
        _selectedClipIds.Clear();
        RefreshClipChrome();
    }

    private void RefreshClipChrome()
    {
        foreach (var kv in _clipBodyById)
        {
            var id = kv.Key;
            kv.Value.Background = _selectedClipIds.Contains(id) ? ClipSelectedBrush ?? ClipBrush : ClipBrush;
        }
    }

    private void CopyInternal()
    {
        _clipboard.Clear();
        foreach (var id in _selectedClipIds)
        {
            var c = Clips.FirstOrDefault(x => x.Id == id);
            if (c == null)
                continue;
            _clipboard.Add(CloneProto(c));
        }
    }

    private void CutInternal()
    {
        CopyInternal();
        DeleteSelectedClips();
    }

    private static TimelineClipItem CloneProto(TimelineClipItem c) =>
        new()
        {
            TrackId = c.TrackId,
            StartTime = c.StartTime,
            Duration = c.Duration,
            Label = c.Label,
            Tag = c.Tag,
        };

    private void DeleteSelectedClips()
    {
        if (_selectedClipIds.Count == 0)
            return;
        var removed = new List<TimelineClipItem>();
        for (var i = Clips.Count - 1; i >= 0; i--)
        {
            var c = Clips[i];
            if (_selectedClipIds.Contains(c.Id))
            {
                removed.Add(c);
                Clips.RemoveAt(i);
                UnhookClip(c);
            }
        }

        _selectedClipIds.Clear();
        FullRebuild();
        if (removed.Count > 0)
            ClipsRemoved?.Invoke(this, new TimelineClipsEventArgs(removed));
    }

    private string ResolvePasteTargetTrackId()
    {
        if (_selectedClipIds.Count == 1)
        {
            var one = Clips.FirstOrDefault(c => _selectedClipIds.Contains(c.Id));
            if (one != null)
                return one.TrackId;
        }

        if (!string.IsNullOrEmpty(_selectedTrackId) && Tracks.Any(t => t.Id == _selectedTrackId))
            return _selectedTrackId!;

        return Tracks.FirstOrDefault()?.Id ?? "";
    }

    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border b || b.Tag is not string tid)
            return;
        _headerReorderFrom = Tracks.ToList().FindIndex(t => t.Id == tid);
        _headerPressPoint = _headerStack != null ? e.GetPosition(_headerStack) : default;
        _headerReorderDrag = false;
        e.Pointer.Capture(b);
    }

    private void OnHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_headerReorderFrom is null || _headerStack == null || sender is not Border b)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, b))
            return;
        var now = e.GetPosition(_headerStack);
        var dy = now.Y - _headerPressPoint.Y;
        if (Math.Abs(dy) >= TrackReorderDragThreshold)
            _headerReorderDrag = true;
    }

    private void OnHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (sender is not Border b || b.Tag is not string tid || _headerStack == null)
                return;
            if (_headerReorderFrom is { } from && Tracks.Count > 0 && _headerReorderDrag)
            {
                var y = e.GetPosition(_headerStack).Y;
                var to = (int)Math.Floor(y / TrackHeight);
                to = Math.Clamp(to, 0, Tracks.Count - 1);
                if (to != from)
                {
                    var item = Tracks[from];
                    Tracks.RemoveAt(from);
                    Tracks.Insert(to, item);
                    TrackOrderChanged?.Invoke(this, new TimelineTrackReorderEventArgs(item, from, to));
                    SetSelectedTrackId(item.Id);
                    FullRebuild();
                }
                else
                    SetSelectedTrackId(tid);
            }
            else
            {
                SetSelectedTrackId(tid);
                ClearClipSelection();
            }
        }
        finally
        {
            _headerReorderFrom = null;
            _headerReorderDrag = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Stop();
        DetachScrollSync();
    }
}
