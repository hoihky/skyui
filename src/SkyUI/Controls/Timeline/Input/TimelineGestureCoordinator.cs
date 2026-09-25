namespace SkyUI.Controls.Timeline.Input;

/// <summary>Registers and attaches timeline gesture interactors in priority order.</summary>
public sealed class TimelineGestureCoordinator : IDisposable
{
    private readonly List<ITimelineGestureInteractor> interactors = new();
    private TimelineInteractionContext? context;

    public void Register(ITimelineGestureInteractor interactor)
    {
        if (!interactors.Contains(interactor))
            interactors.Add(interactor);
        interactors.Sort(static (a, b) => b.Priority.CompareTo(a.Priority));
    }

    public void Attach(TimelineInteractionContext ctx)
    {
        context = ctx;
        foreach (var interactor in interactors)
            interactor.Attach(ctx);
    }

    public void Detach()
    {
        foreach (var interactor in interactors)
            interactor.Detach();
        context = null;
    }

    public void Dispose() => Detach();
}
