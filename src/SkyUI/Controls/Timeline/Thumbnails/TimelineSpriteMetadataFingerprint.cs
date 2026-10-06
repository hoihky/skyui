using SkyUI.Controls;

namespace SkyUI.Controls.Timeline.Thumbnails;

/// <summary>Stable cache fingerprint for sprite clip metadata.</summary>
public sealed class TimelineSpriteMetadataFingerprint
{
    public string Compute(TimelineClipItem clip)
    {
        if (clip.Sprite is null)
            return $"label:{clip.Label}";
        var s = clip.Sprite;
        return string.Join(
            '|',
            s.AtlasId,
            s.SpriteName,
            s.FrameIndex.ToString(),
            s.HoldFrames.ToString(),
            s.HoldLastCel.ToString(),
            s.HasSourceRect.ToString(),
            s.SourceRect.X.ToString("F2"),
            s.SourceRect.Y.ToString("F2"),
            s.SourceRect.Width.ToString("F2"),
            s.SourceRect.Height.ToString("F2"));
    }
}
