namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineSplitClipCommand : ITimelineEditCommand
{
    private readonly TimelineClipItem leftClip;
    private readonly ICollection<TimelineClipItem> clips;
    private readonly double splitTime;
    private readonly double beforeDuration;
    private readonly double minClipDurationSeconds;
    private TimelineClipItem? rightClip;

    public TimelineSplitClipCommand(
        TimelineClipItem leftClip,
        ICollection<TimelineClipItem> clips,
        double splitTime,
        double beforeDuration,
        double minClipDurationSeconds)
    {
        this.leftClip = leftClip;
        this.clips = clips;
        this.splitTime = splitTime;
        this.beforeDuration = beforeDuration;
        this.minClipDurationSeconds = minClipDurationSeconds;
    }

    public string Description => "Split clip";

    public void Execute()
    {
        if (rightClip is not null)
        {
            leftClip.Duration = splitTime - leftClip.StartTime;
            if (!clips.Contains(rightClip))
                clips.Add(rightClip);
            return;
        }

        rightClip = Timeline.Editing.TimelineClipOperations.SplitAt(
            leftClip,
            splitTime,
            clips,
            minClipDurationSeconds);
    }

    public void Undo()
    {
        if (rightClip is null)
            return;
        clips.Remove(rightClip);
        leftClip.Duration = beforeDuration;
        rightClip = null;
    }
}
