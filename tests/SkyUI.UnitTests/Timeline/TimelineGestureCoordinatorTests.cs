using SkyUI.Controls.Timeline.Input;

namespace SkyUI.UnitTests.Timeline;

public class TimelineGestureCoordinatorTests
{
    [Fact]
    public void Register_sorts_by_priority_descending()
    {
        var coordinator = new TimelineGestureCoordinator();
        var order = new List<int>();
        coordinator.Register(new RecordingInteractor(10, order));
        coordinator.Register(new RecordingInteractor(50, order));
        coordinator.Register(new RecordingInteractor(30, order));

        var ctx = new TimelineInteractionContext
        {
            Host = null!,
            Project = new Controls.Timeline.Model.TimelineProject(),
            Layout = new Controls.Timeline.Layout.TimelineLayoutEngine(),
            Selection = new Controls.TimelineSelectionModel(),
            UndoStack = new Controls.TimelineUndoStack(),
            Renderer = new Controls.Timeline.Rendering.TimelineRenderer(),
        };

        coordinator.Attach(ctx);
        Assert.Equal([50, 30, 10], order);
        coordinator.Dispose();
        Assert.Empty(order);
    }

    [Fact]
    public void Register_does_not_add_duplicate_interactor()
    {
        var coordinator = new TimelineGestureCoordinator();
        var interactor = new RecordingInteractor(10, []);
        coordinator.Register(interactor);
        coordinator.Register(interactor);

        var ctx = new TimelineInteractionContext
        {
            Host = null!,
            Project = new Controls.Timeline.Model.TimelineProject(),
            Layout = new Controls.Timeline.Layout.TimelineLayoutEngine(),
            Selection = new Controls.TimelineSelectionModel(),
            UndoStack = new Controls.TimelineUndoStack(),
            Renderer = new Controls.Timeline.Rendering.TimelineRenderer(),
        };

        var order = new List<int>();
        ((RecordingInteractor)interactor).SetOrder(order);
        coordinator.Attach(ctx);

        Assert.Single(order);
        coordinator.Dispose();
    }

    private sealed class RecordingInteractor(int priority, List<int> order) : ITimelineGestureInteractor
    {
        private List<int> order = order;

        public int Priority => priority;

        public void SetOrder(List<int> target) => order = target;

        public void Attach(TimelineInteractionContext context) => order.Add(Priority);

        public void Detach() => order.Clear();

        public void Dispose() => Detach();
    }
}
