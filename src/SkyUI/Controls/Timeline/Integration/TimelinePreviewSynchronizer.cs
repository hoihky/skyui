namespace SkyUI.Controls.Timeline.Integration;

/// <summary>Publishes preview snapshots when the timeline playhead frame changes.</summary>
public sealed class TimelinePreviewSynchronizer
{
    private VideoTimeline? timeline;

    public event EventHandler<TimelinePreviewFrameEventArgs>? PreviewFrameChanged;

    public void Bind(VideoTimeline control)
    {
        if (ReferenceEquals(timeline, control))
            return;
        Unbind();
        timeline = control;
        control.CurrentFrameChanged += OnCurrentFrameChanged;
        Publish(control.PlayheadFrame, control.PlayheadTime);
    }

    public void Unbind()
    {
        if (timeline is null)
            return;
        timeline.CurrentFrameChanged -= OnCurrentFrameChanged;
        timeline = null;
    }

    public TimelinePreviewFrameSnapshot CreateSnapshot(int frame, double timeSeconds)
    {
        if (timeline is null)
            return new TimelinePreviewFrameSnapshot(frame, timeSeconds, [], null);

        var layers = timeline.SampleSpriteLayersAtFrame(frame);
        var onion = timeline.OnionSkinSettings.IsEnabled
            ? timeline.SampleOnionSkinAtFrame(frame)
            : null;
        return new TimelinePreviewFrameSnapshot(frame, timeSeconds, layers, onion);
    }

    private void OnCurrentFrameChanged(object? sender, TimelineCurrentFrameEventArgs e) =>
        Publish(e.Frame, e.TimeSeconds);

    private void Publish(int frame, double timeSeconds)
    {
        var snapshot = CreateSnapshot(frame, timeSeconds);
        PreviewFrameChanged?.Invoke(timeline, new TimelinePreviewFrameEventArgs(snapshot));
    }
}
