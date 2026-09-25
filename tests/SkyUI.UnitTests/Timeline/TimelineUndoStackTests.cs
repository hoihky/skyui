using SkyUI.Controls;
using SkyUI.Controls.Timeline.Commands;

namespace SkyUI.UnitTests.Timeline;

public class TimelineUndoStackTests
{
    [Fact]
    public void Execute_push_undo_redo_round_trip()
    {
        var stack = new TimelineUndoStack();
        var state = 0;
        var command = new RecordingCommand(() => state = 1, () => state = 0);

        stack.Execute(command);
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
        Assert.Equal(1, state);

        stack.Undo();
        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
        Assert.Equal(0, state);

        stack.Redo();
        Assert.True(stack.CanUndo);
        Assert.Equal(1, state);
    }

    [Fact]
    public void Execute_clears_redo_stack()
    {
        var stack = new TimelineUndoStack();
        var state = 0;
        stack.Execute(new RecordingCommand(() => state = 1, () => state = 0));
        stack.Undo();
        stack.Execute(new RecordingCommand(() => state = 2, () => state = 0));

        Assert.False(stack.CanRedo);
        Assert.Equal(2, state);
    }

    [Fact]
    public void Trim_command_restores_original_values()
    {
        var clip = new TimelineClipItem { StartTime = 2, Duration = 8 };
        var command = new TimelineTrimClipCommand(clip, 2, 8, 3, 6);

        command.Execute();
        Assert.Equal(3, clip.StartTime);
        Assert.Equal(6, clip.Duration);

        command.Undo();
        Assert.Equal(2, clip.StartTime);
        Assert.Equal(8, clip.Duration);
    }

    [Fact]
    public void Move_command_restores_all_clip_positions()
    {
        var a = new TimelineClipItem { StartTime = 1, Duration = 2, TrackId = "t1" };
        var b = new TimelineClipItem { StartTime = 5, Duration = 2, TrackId = "t2" };
        var clips = new[] { a, b };
        var before = TimelineMoveClipsCommand.Capture(clips);
        a.StartTime = 3;
        a.TrackId = "t2";
        b.StartTime = 7;
        var after = TimelineMoveClipsCommand.Capture(clips);

        var command = new TimelineMoveClipsCommand(clips, before, after);
        command.Undo();

        Assert.Equal(1, a.StartTime);
        Assert.Equal("t1", a.TrackId);
        Assert.Equal(5, b.StartTime);
        Assert.Equal("t2", b.TrackId);
    }

    [Fact]
    public void Clear_empties_undo_and_redo()
    {
        var stack = new TimelineUndoStack();
        stack.Execute(new RecordingCommand(() => { }, () => { }));
        stack.Undo();
        stack.Clear();

        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void StateChanged_fires_on_execute_undo_and_redo()
    {
        var stack = new TimelineUndoStack();
        var changes = 0;
        stack.StateChanged += (_, _) => changes++;
        var command = new RecordingCommand(() => { }, () => { });

        stack.Execute(command);
        stack.Undo();
        stack.Redo();

        Assert.Equal(3, changes);
    }

    private sealed class RecordingCommand(Action execute, Action undo) : ITimelineCommand
    {
        public string Description => "test";

        public void Execute() => execute();

        public void Undo() => undo();
    }
}
