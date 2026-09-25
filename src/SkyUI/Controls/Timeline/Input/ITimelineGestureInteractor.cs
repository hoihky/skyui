namespace SkyUI.Controls.Timeline.Input;

/// <summary>Pointer/keyboard gesture handler for timeline editing (SkyPullToRefresh-style plug-in).</summary>
public interface ITimelineGestureInteractor : IDisposable
{
    /// <summary>Higher priority interactors receive first chance to handle input.</summary>
    int Priority { get; }

    void Attach(TimelineInteractionContext context);

    void Detach();
}
