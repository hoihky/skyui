using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class TimelineClipSpriteTests
{
    [Fact]
    public void Sprite_metadata_changes_raise_clip_PropertyChanged()
    {
        var clip = new TimelineClipItem
        {
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "a" },
        };
        var raised = false;
        clip.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimelineClipItem.Sprite))
                raised = true;
        };
        clip.Sprite!.SpriteName = "b";
        Assert.True(raised);
    }
}
