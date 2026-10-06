using SkyUI.Controls.Timeline.Commands;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Keyframes;

/// <summary>Creates and updates keyframes with undo support.</summary>
public sealed class TimelineKeyframeEditor
{
    private readonly TimelineProject project;
    private readonly TimelineKeyframeCatalog catalog;
    private readonly TimelineUndoStack undoStack;

    public TimelineKeyframeEditor(
        TimelineProject project,
        TimelineKeyframeCatalog catalog,
        TimelineUndoStack undoStack)
    {
        this.project = project;
        this.catalog = catalog;
        this.undoStack = undoStack;
    }

    public TimelineKeyframeItem SetKeyframe(
        string trackId,
        string propertyName,
        double timeSeconds,
        object? value)
    {
        var existing = catalog.FindAtTime(trackId, propertyName, timeSeconds);
        if (existing is not null)
        {
            var beforeValue = existing.Value;
            undoStack.Execute(new TimelineChangeKeyframeValueCommand(existing, beforeValue, value));
            return existing;
        }

        var keyframe = new TimelineKeyframeItem
        {
            TrackId = trackId,
            PropertyName = propertyName,
            Time = timeSeconds,
            Value = value,
        };
        undoStack.Execute(new TimelineAddKeyframeCommand(project.Keyframes, keyframe));
        return keyframe;
    }

    public void CommitMove(TimelineKeyframeItem keyframe, double beforeTimeSeconds, double afterTimeSeconds)
    {
        afterTimeSeconds = Math.Max(0, afterTimeSeconds);
        if (Math.Abs(beforeTimeSeconds - afterTimeSeconds) < 1e-9)
            return;
        undoStack.Execute(new TimelineMoveKeyframeCommand(keyframe, beforeTimeSeconds, afterTimeSeconds));
    }
}
