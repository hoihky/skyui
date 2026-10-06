namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineExtendClipHoldCommand : ITimelineEditCommand
{
    private readonly TimelineClipItem clip;
    private readonly int deltaFrames;
    private readonly double timelineDuration;
    private readonly Timeline.Editing.TimelineClipHoldEditor holdEditor;
    private readonly double beforeDuration;
    private readonly int? beforeHoldFrames;
    private double afterDuration;

    public TimelineExtendClipHoldCommand(
        TimelineClipItem clip,
        int deltaFrames,
        double timelineDuration,
        Timeline.Editing.TimelineClipHoldEditor holdEditor)
    {
        this.clip = clip;
        this.deltaFrames = deltaFrames;
        this.timelineDuration = timelineDuration;
        this.holdEditor = holdEditor;
        beforeDuration = clip.Duration;
        beforeHoldFrames = clip.Sprite?.HoldFrames;
    }

    public string Description => "Adjust hold frames";

    public void Execute()
    {
        holdEditor.ApplyHoldDelta(clip, deltaFrames, timelineDuration);
        afterDuration = clip.Duration;
    }

    public void Undo()
    {
        clip.Duration = beforeDuration;
        if (clip.Sprite is not null && beforeHoldFrames is not null)
            clip.Sprite.HoldFrames = beforeHoldFrames.Value;
    }
}
