using SkyUI.Controls;

namespace SkyUI.UnitTests.Timeline;

public class TimelineSelectionModelTests
{
    [Fact]
    public void SelectSingleClip_clears_track_and_replaces_clip_selection()
    {
        var model = new TimelineSelectionModel { SelectedTrackId = "track-a" };
        var changes = 0;
        model.SelectionChanged += (_, _) => changes++;

        model.SelectSingleClip("clip-1");

        Assert.Null(model.SelectedTrackId);
        Assert.Single(model.SelectedClipIds);
        Assert.True(model.IsClipSelected("clip-1"));
        Assert.Equal(1, changes);
    }

    [Fact]
    public void ToggleClip_adds_and_removes_ids()
    {
        var model = new TimelineSelectionModel();
        model.ToggleClip("clip-1");
        Assert.True(model.IsClipSelected("clip-1"));

        model.ToggleClip("clip-1");
        Assert.False(model.IsClipSelected("clip-1"));
    }

    [Fact]
    public void ResolveClips_returns_only_selected_items()
    {
        var clipA = new TimelineClipItem();
        var clipB = new TimelineClipItem();
        var clipC = new TimelineClipItem();
        var model = new TimelineSelectionModel();
        model.SetClipSelection([clipA.Id, clipC.Id]);

        var selected = model.ResolveClips([clipA, clipB, clipC]);
        Assert.Equal(2, selected.Count);
        Assert.Contains(selected, c => c.Id == clipA.Id);
        Assert.Contains(selected, c => c.Id == clipC.Id);
    }

    [Fact]
    public void ClearClipSelection_is_noop_when_empty()
    {
        var model = new TimelineSelectionModel();
        var changes = 0;
        model.SelectionChanged += (_, _) => changes++;

        model.ClearClipSelection();

        Assert.Equal(0, changes);
    }

    [Fact]
    public void ClearAll_resets_track_and_clip_selection()
    {
        var model = new TimelineSelectionModel();
        model.SelectedTrackId = "track-a";
        model.SelectSingleClip("clip-1");

        model.ClearAll();

        Assert.Null(model.SelectedTrackId);
        Assert.Empty(model.SelectedClipIds);
    }

    [Fact]
    public void SetClipSelection_replaces_previous_ids()
    {
        var model = new TimelineSelectionModel();
        model.SetClipSelection(["a", "b"]);
        model.SetClipSelection(["c"]);

        Assert.Single(model.SelectedClipIds);
        Assert.True(model.IsClipSelected("c"));
        Assert.False(model.IsClipSelected("a"));
    }
}
