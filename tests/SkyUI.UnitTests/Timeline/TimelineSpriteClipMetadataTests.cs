using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class TimelineSpriteClipMetadataTests
{
    [Fact]
    public void Clone_copies_sprite_fields()
    {
        var source = new TimelineSpriteClipMetadata
        {
            AtlasId = "atlas-1",
            SpriteName = "walk",
            FrameIndex = 7,
            HoldFrames = 12,
            HasSourceRect = true,
            SourceRect = new TimelineSpriteSourceRect(1, 2, 32, 32),
        };
        var clone = source.Clone();
        Assert.NotSame(source, clone);
        Assert.Equal(source.AtlasId, clone.AtlasId);
        Assert.Equal(source.SpriteName, clone.SpriteName);
        Assert.Equal(source.FrameIndex, clone.FrameIndex);
        Assert.Equal(source.HoldFrames, clone.HoldFrames);
        Assert.True(clone.HasSourceRect);
        Assert.Equal(source.SourceRect, clone.SourceRect);
    }

    [Fact]
    public void HoldFrames_clamps_to_at_least_one()
    {
        var meta = new TimelineSpriteClipMetadata { HoldFrames = 0 };
        Assert.Equal(1, meta.HoldFrames);
    }
}
