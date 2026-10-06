namespace SkyUI.Controls.Timeline.Thumbnails;

public readonly struct TimelineClipThumbnailCacheKey : IEquatable<TimelineClipThumbnailCacheKey>
{
    public TimelineClipThumbnailCacheKey(string clipId, string metadataFingerprint)
    {
        ClipId = clipId;
        MetadataFingerprint = metadataFingerprint;
    }

    public string ClipId { get; }

    public string MetadataFingerprint { get; }

    public bool Equals(TimelineClipThumbnailCacheKey other) =>
        ClipId == other.ClipId && MetadataFingerprint == other.MetadataFingerprint;

    public override bool Equals(object? obj) => obj is TimelineClipThumbnailCacheKey other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ClipId, MetadataFingerprint);
}
