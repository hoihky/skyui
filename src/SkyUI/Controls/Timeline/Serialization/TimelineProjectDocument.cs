using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Serialization;

/// <summary>JSON-friendly project snapshot for host apps (sprite timeline).</summary>
public sealed class TimelineProjectDocument
{
    public int SchemaVersion { get; set; } = 1;

    public double Duration { get; set; }

    public double Fps { get; set; } = 24;

    public TimelineTimeUnit TimeUnit { get; set; } = TimelineTimeUnit.Frames;

    public List<TimelineTrackDocument> Tracks { get; set; } = [];

    public List<TimelineClipDocument> Clips { get; set; } = [];

    public List<TimelineMarkerDocument> Markers { get; set; } = [];

    public List<TimelineKeyframeDocument> Keyframes { get; set; } = [];
}

public sealed class TimelineTrackDocument
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "Track";

    public TimelineTrackKind Kind { get; set; } = TimelineTrackKind.Standard;

    public bool IsVisible { get; set; } = true;

    public bool IsLocked { get; set; }

    public string? AccentColor { get; set; }
}

public sealed class TimelineClipDocument
{
    public string Id { get; set; } = "";

    public string TrackId { get; set; } = "";

    public double StartTime { get; set; }

    public double Duration { get; set; } = 1;

    public string Label { get; set; } = "";

    public TimelineSpriteClipDocument? Sprite { get; set; }
}

public sealed class TimelineSpriteClipDocument
{
    public string AtlasId { get; set; } = "";

    public string SpriteName { get; set; } = "";

    public int FrameIndex { get; set; }

    public int HoldFrames { get; set; } = 1;

    public bool HoldLastCel { get; set; }

    public bool HasSourceRect { get; set; }

    public double SourceX { get; set; }

    public double SourceY { get; set; }

    public double SourceWidth { get; set; }

    public double SourceHeight { get; set; }
}

public sealed class TimelineMarkerDocument
{
    public string Id { get; set; } = "";

    public double Time { get; set; }

    public string Label { get; set; } = "";
}

public sealed class TimelineKeyframeDocument
{
    public string Id { get; set; } = "";

    public string TrackId { get; set; } = "";

    public string PropertyName { get; set; } = "";

    public double Time { get; set; }

    public double? NumericValue { get; set; }

    public string? TextValue { get; set; }
}
