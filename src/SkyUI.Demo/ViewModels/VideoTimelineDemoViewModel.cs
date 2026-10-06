using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Composition;
using SkyUI.Controls.Timeline.Integration;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Serialization;
using SkyUI.Demo.Models;

namespace SkyUI.Demo.ViewModels;

public sealed class VideoTimelineDemoViewModel : INotifyPropertyChanged
{
    private readonly TimelineProjectDocumentMapper documentMapper = new();
    private readonly JsonTimelineProjectSerializer projectSerializer = new();
    private readonly SpriteAtlasStub atlasStub = new();
    private VideoTimeline? timeline;
    private string playheadReadout = "Playhead: f0";
    private string previewSummary = "Preview: (no timeline)";
    private bool magneticSnap = true;
    private bool loopTimeRange = true;
    private bool onionSkinEnabled = true;

    public VideoTimelineDemoViewModel()
    {
        LogItems = new ObservableCollection<string>();
        PreviewLayers = new ObservableCollection<SpritePreviewLayerViewModel>();
        PlayCommand = new RelayCommand(() => timeline?.Play());
        StopCommand = new RelayCommand(() => timeline?.Stop());
        AddSpriteTrackCommand = new RelayCommand(AddSpriteTrack);
        AddClipCommand = new RelayCommand(AddClipAtPlayhead);
        LoadSampleCommand = new RelayCommand(LoadSampleProject);
        ExportJsonCommand = new RelayCommand(ExportProjectJson);
        ExtendHoldCommand = new RelayCommand(() => timeline?.ExtendSelectedClipHoldFrames(1));
        ShrinkHoldCommand = new RelayCommand(() => timeline?.ExtendSelectedClipHoldFrames(-1));
        AddOpacityKeyframeCommand = new RelayCommand(AddOpacityKeyframe);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> LogItems { get; }

    public ObservableCollection<SpritePreviewLayerViewModel> PreviewLayers { get; }

    public string PlayheadReadout
    {
        get => playheadReadout;
        private set => SetField(ref playheadReadout, value);
    }

    public string PreviewSummary
    {
        get => previewSummary;
        private set => SetField(ref previewSummary, value);
    }

    public bool MagneticSnap
    {
        get => magneticSnap;
        set
        {
            if (!SetField(ref magneticSnap, value) || timeline is null)
                return;
            timeline.SnapSettings.IsEnabled = value;
        }
    }

    public bool LoopTimeRange
    {
        get => loopTimeRange;
        set
        {
            if (!SetField(ref loopTimeRange, value) || timeline is null)
                return;
            timeline.LoopTimeRange = value;
        }
    }

    public ICommand PlayCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand AddSpriteTrackCommand { get; }

    public ICommand AddClipCommand { get; }

    public ICommand LoadSampleCommand { get; }

    public ICommand ExportJsonCommand { get; }

    public ICommand ExtendHoldCommand { get; }

    public ICommand ShrinkHoldCommand { get; }

    public ICommand AddOpacityKeyframeCommand { get; }

    public bool OnionSkinEnabled
    {
        get => onionSkinEnabled;
        set
        {
            if (!SetField(ref onionSkinEnabled, value) || timeline is null)
                return;
            timeline.OnionSkinSettings.IsEnabled = value;
            ApplyPreviewSnapshot(timeline.CreatePreviewSnapshot());
        }
    }

    private void ApplyPreviewSnapshot(TimelinePreviewFrameSnapshot snapshot)
    {
        PreviewLayers.Clear();
        foreach (var layer in snapshot.SpriteLayers)
        {
            var cel = layer.Sprite?.SpriteName ?? "(empty)";
            PreviewLayers.Add(new SpritePreviewLayerViewModel(layer.Track.Name, cel, atlasStub.ResolveBrush(cel)));
        }

        if (timeline?.OnionSkinSettings.IsEnabled == true && snapshot.OnionSkinFrames is { Count: > 0 })
            PreviewSummary = FormatOnionPreview(snapshot.OnionSkinFrames);
        else
            PreviewSummary = FormatPreview(snapshot.Frame, snapshot.SpriteLayers);
    }

    public void AttachTimeline(VideoTimeline control)
    {
        timeline = control;
        timeline.SnapSettings.IsEnabled = MagneticSnap;
        timeline.LoopTimeRange = LoopTimeRange;
        timeline.OnionSkinSettings.IsEnabled = OnionSkinEnabled;
        timeline.OnionSkinSettings.PreviousFrameCount = 2;
        timeline.OnionSkinSettings.NextFrameCount = 1;
        timeline.ClipThumbnailProvider = NullTimelineClipThumbnailProvider.Instance;
        WireTimelineEvents(control);
        LoadSampleProject();
    }

    public void Log(string line)
    {
        LogItems.Add(line);
        if (LogItems.Count > 200)
            LogItems.RemoveAt(0);
    }

    private void WireTimelineEvents(VideoTimeline control)
    {
        control.PreviewFrameChanged += (_, e) => ApplyPreviewSnapshot(e.Snapshot);
        control.PlayheadChanged += (_, e) =>
            PlayheadReadout =
                $"Playhead: f{e.PlayheadFrame} ({e.TimeSeconds:F3}s) · {control.MediaClock.FrameDuration.TotalMilliseconds:F1} ms/f";
        control.ClipChanged += (_, e) => Log($"Clip {e.EditKind}: {e.Clip.Label}");
        control.SelectionChanged += (_, e) =>
            Log($"Selection: {e.SelectedClips.Count} clip(s), track={e.SelectedTrack?.Name ?? "(none)"}");
    }

    private void LoadSampleProject()
    {
        if (timeline is null)
            return;

        timeline.TimeUnit = TimelineTimeUnit.Frames;
        timeline.Fps = 24;
        timeline.Duration = 5;
        timeline.Tracks.Clear();
        timeline.Clips.Clear();
        timeline.Markers.Clear();
        timeline.Keyframes.Clear();

        var layerA = CreateSpriteTrack("Layer A", "#4FC3F7");
        var layerB = CreateSpriteTrack("Layer B", "#81C784");
        var layerC = CreateSpriteTrack("Layer C", "#FFB74D");
        timeline.Tracks.Add(layerA);
        timeline.Tracks.Add(layerB);
        timeline.Tracks.Add(layerC);
        var opacityTrack = new TimelineTrackItem
        {
            Name = "Opacity",
            Kind = TimelineTrackKind.Property,
            AccentColor = "#CE93D8",
        };
        timeline.Tracks.Add(opacityTrack);

        timeline.Markers.Add(new TimelineMarkerItem { Time = 1, Label = "Pose A" });
        timeline.Markers.Add(new TimelineMarkerItem { Time = 3, Label = "Pose B" });

        var fps = timeline.Fps;
        timeline.Clips.Add(CreateSpriteClip(layerA.Id, 2, 48, "hero_run", 12, fps));
        timeline.Clips.Add(CreateSpriteClip(layerA.Id, 72, 36, "hero_idle", 4, fps));
        timeline.Clips.Add(CreateSpriteClip(layerB.Id, 24, 60, "fx_spark", 8, fps));
        timeline.Clips.Add(CreateSpriteClip(layerC.Id, 0, 120, "background", 0, fps));
        var hero = timeline.Clips[0];
        if (hero.Sprite is not null)
            hero.Sprite.HoldLastCel = true;

        timeline.PlayheadTime = 0;
        ApplyPreviewSnapshot(timeline.CreatePreviewSnapshot());
        Log("Sprite sample project loaded.");
    }

    private void AddSpriteTrack()
    {
        if (timeline is null)
            return;
        var name = $"Layer {timeline.Tracks.Count + 1}";
        timeline.Tracks.Add(CreateSpriteTrack(name, null));
        Log($"Sprite track added: {name}");
    }

    private void AddClipAtPlayhead()
    {
        if (timeline is null || timeline.Tracks.Count == 0)
            return;
        var track = timeline.Tracks.FirstOrDefault(t => t.Id == timeline.SelectedTrackId)
                    ?? timeline.Tracks.First(t => t.Kind == TimelineTrackKind.Sprite);
        var startFrame = timeline.PlayheadFrame;
        var clip = CreateSpriteClip(track.Id, startFrame, 24, "new_cel", startFrame, timeline.Fps);
        clip.StartTime = timeline.PlayheadTime;
        timeline.Clips.Add(clip);
        Log($"Clip added at f{startFrame}.");
    }

    private void ExportProjectJson()
    {
        if (timeline is null)
            return;
        var doc = documentMapper.ToDocument(timeline.Project);
        var json = projectSerializer.Serialize(doc);
        Log($"Exported JSON ({json.Length} chars). First line: {json.Split('\n')[0]}");
    }

    private void AddOpacityKeyframe()
    {
        if (timeline is null)
            return;
        timeline.SetOpacityKeyframeAtPlayhead();
        Log("Opacity keyframe at playhead (Ctrl+K).");
    }

    private static string FormatPreview(int frame, IReadOnlyList<TimelineSpriteLayerFrameSample> layers)
    {
        var parts = new List<string> { $"Preview @ f{frame}" };
        foreach (var layer in layers)
        {
            var cel = layer.Sprite?.SpriteName ?? "(empty)";
            parts.Add($"{layer.Track.Name}={cel}");
        }

        return string.Join(" | ", parts);
    }

    private static string FormatOnionPreview(IReadOnlyList<SkyUI.Controls.Timeline.OnionSkin.TimelineOnionSkinFrameSample> frames)
    {
        var parts = new List<string> { $"Onion ({frames.Count} frames)" };
        foreach (var frame in frames)
        {
            var tag = frame.IsCenterFrame ? "*" : "";
            var cel = frame.Layers.FirstOrDefault(l => l.Sprite is not null)?.Sprite?.SpriteName ?? "-";
            parts.Add($"f{frame.Frame}{tag}:{cel}");
        }

        return string.Join(" | ", parts);
    }

    private static TimelineTrackItem CreateSpriteTrack(string name, string? accent) =>
        new()
        {
            Name = name,
            Kind = TimelineTrackKind.Sprite,
            AccentColor = accent,
        };

    private static TimelineClipItem CreateSpriteClip(
        string trackId,
        int startFrame,
        int durationFrames,
        string spriteName,
        int atlasFrameIndex,
        double fps) =>
        new()
        {
            TrackId = trackId,
            StartTime = startFrame / fps,
            Duration = durationFrames / fps,
            Label = spriteName,
            Sprite = new TimelineSpriteClipMetadata
            {
                AtlasId = "demo-atlas",
                SpriteName = spriteName,
                FrameIndex = atlasFrameIndex,
                HoldFrames = durationFrames,
            },
        };

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
