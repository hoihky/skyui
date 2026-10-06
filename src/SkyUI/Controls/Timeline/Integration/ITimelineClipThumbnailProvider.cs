using Avalonia.Media;

namespace SkyUI.Controls.Timeline.Integration;

/// <summary>Optional async thumbnails for clip chrome (Tier 2); safe to leave null.</summary>
public interface ITimelineClipThumbnailProvider
{
    Task<IImage?> GetThumbnailAsync(TimelineClipItem clip, CancellationToken cancellationToken = default);
}
