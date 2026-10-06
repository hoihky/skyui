using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Rendering;

internal sealed class TimelineClipLabelFormatter
{
    public string Format(TimelineClipItem clip)
    {
        if (clip.Sprite is { } sprite)
        {
            if (!string.IsNullOrEmpty(sprite.SpriteName))
                return sprite.SpriteName;
            if (sprite.FrameIndex > 0)
                return $"f{sprite.FrameIndex}";
        }

        return string.IsNullOrEmpty(clip.Label) ? "Clip" : clip.Label;
    }
}
