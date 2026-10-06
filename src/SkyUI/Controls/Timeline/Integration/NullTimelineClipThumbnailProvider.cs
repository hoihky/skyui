using Avalonia.Media;

namespace SkyUI.Controls.Timeline.Integration;

/// <summary>No-op thumbnail provider for hosts that do not render clip thumbnails yet.</summary>
public sealed class NullTimelineClipThumbnailProvider : ITimelineClipThumbnailProvider
{
    public static NullTimelineClipThumbnailProvider Instance { get; } = new();

    public Task<IImage?> GetThumbnailAsync(TimelineClipItem clip, CancellationToken cancellationToken = default) =>
        Task.FromResult<IImage?>(null);
}
