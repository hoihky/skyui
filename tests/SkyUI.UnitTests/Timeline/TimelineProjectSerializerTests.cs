using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Serialization;

namespace SkyUI.UnitTests.Timeline;

public class TimelineProjectSerializerTests
{
    [Fact]
    public void Json_round_trip_preserves_sprite_metadata()
    {
        var project = new TimelineProject { Fps = 30, TimeUnit = TimelineTimeUnit.Frames, Duration = 10 };
        var track = new TimelineTrackItem
        {
            Name = "Sprites",
            Kind = TimelineTrackKind.Sprite,
            AccentColor = "#FF0000",
        };
        project.Tracks.Add(track);
        project.Clips.Add(new TimelineClipItem
        {
            TrackId = track.Id,
            StartTime = 1,
            Duration = 2,
            Label = "cel",
            Sprite = new TimelineSpriteClipMetadata
            {
                AtlasId = "main",
                SpriteName = "blink",
                FrameIndex = 3,
            },
        });

        var mapper = new TimelineProjectDocumentMapper();
        var serializer = new JsonTimelineProjectSerializer();
        var json = serializer.Serialize(mapper.ToDocument(project));
        var restored = new TimelineProject();
        mapper.ApplyToProject(serializer.Deserialize(json), restored);

        Assert.Equal(30, restored.Fps);
        Assert.Single(restored.Tracks);
        Assert.Equal(TimelineTrackKind.Sprite, restored.Tracks[0].Kind);
        Assert.Equal("#FF0000", restored.Tracks[0].AccentColor);
        Assert.Equal("blink", restored.Clips[0].Sprite?.SpriteName);
        Assert.Equal("main", restored.Clips[0].Sprite?.AtlasId);
    }
}
