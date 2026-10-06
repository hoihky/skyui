namespace SkyUI.Controls.Timeline.Serialization;

public interface ITimelineProjectSerializer
{
    string Serialize(TimelineProjectDocument document);

    TimelineProjectDocument Deserialize(string json);
}
