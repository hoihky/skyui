using Avalonia.Media;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Integration;
using SkyUI.Demo.Models;

namespace SkyUI.Demo.Integration;

public sealed class DemoTimelineClipThumbnailProvider : ITimelineClipThumbnailProvider
{
    private readonly DemoSpriteAtlasCatalog atlas;

    public DemoTimelineClipThumbnailProvider(DemoSpriteAtlasCatalog atlas) => this.atlas = atlas;

    public Task<IImage?> GetThumbnailAsync(TimelineClipItem clip, CancellationToken cancellationToken = default)
    {
        if (clip.Sprite is null)
            return Task.FromResult<IImage?>(null);
        var image = atlas.ResolveCel(clip.Sprite.AtlasId, clip.Sprite.SpriteName, clip.Sprite.FrameIndex);
        return Task.FromResult<IImage?>(image);
    }
}
