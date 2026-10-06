namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineAddKeyframeCommand : ITimelineEditCommand
{
    private readonly ICollection<TimelineKeyframeItem> keyframes;
    private readonly TimelineKeyframeItem keyframe;

    public TimelineAddKeyframeCommand(ICollection<TimelineKeyframeItem> keyframes, TimelineKeyframeItem keyframe)
    {
        this.keyframes = keyframes;
        this.keyframe = keyframe;
    }

    public string Description => "Add keyframe";

    public void Execute()
    {
        if (!keyframes.Contains(keyframe))
            keyframes.Add(keyframe);
    }

    public void Undo() => keyframes.Remove(keyframe);
}
