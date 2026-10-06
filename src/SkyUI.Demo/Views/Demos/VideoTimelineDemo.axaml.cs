using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class VideoTimelineDemo : UserControl
{
    public VideoTimelineDemo()
    {
        InitializeComponent();
        var viewModel = new VideoTimelineDemoViewModel();
        DataContext = viewModel;
        viewModel.AttachTimeline(Timeline);
        Timeline.TimeRangeSelectionChanged += (_, e) =>
            viewModel.Log($"Range: {e.Range.Min:F2}s – {e.Range.Max:F2}s");
        Timeline.TrackOrderChanged += (_, e) =>
            viewModel.Log($"Track reordered: {e.Track.Name}");
        Timeline.UndoRedoStateChanged += (_, _) =>
            viewModel.Log($"Undo={Timeline.CanUndo}, Redo={Timeline.CanRedo}");
    }
}
