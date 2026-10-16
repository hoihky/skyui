using System.Collections.ObjectModel;
using Avalonia.Controls;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDockManagerTests
{
    [Fact]
    public void SkyDockManagerController_filters_panes_by_zone()
    {
        var panes = new ObservableCollection<SkyDockPane>
        {
            new("Explorer", new TextBlock(), SkyDockZone.Left),
            new("Properties", new TextBlock(), SkyDockZone.Right),
            new("Output", new TextBlock(), SkyDockZone.Bottom),
        };
        var controller = new SkyDockManagerController { ToolPanes = panes };

        Assert.Equal(2, controller.GetPanesInZone(SkyDockZone.Left).Count + controller.GetPanesInZone(SkyDockZone.Right).Count);
        Assert.Single(controller.GetPanesInZone(SkyDockZone.Bottom));
    }

    [Fact]
    public void SkyDockManagerController_ensure_selection_picks_first_tab()
    {
        var left = new SkyDockPane("A", null, SkyDockZone.Left);
        var controller = new SkyDockManagerController
        {
            ToolPanes = new ObservableCollection<SkyDockPane> { left },
        };

        controller.EnsureSelection(SkyDockZone.Left);
        Assert.True(left.IsSelected);
    }

    [Fact]
    public void SkyDockManagerController_select_pane_clears_siblings()
    {
        var a = new SkyDockPane("A", null, SkyDockZone.Left);
        var b = new SkyDockPane("B", null, SkyDockZone.Left);
        var controller = new SkyDockManagerController
        {
            ToolPanes = new ObservableCollection<SkyDockPane> { a, b },
        };

        controller.SelectPane(b);
        Assert.False(a.IsSelected);
        Assert.True(b.IsSelected);
    }

    [Fact]
    public void SkyDockManagerController_float_and_dock_pane()
    {
        var pane = new SkyDockPane("Tool", null, SkyDockZone.Left);
        var controller = new SkyDockManagerController { ToolPanes = new[] { pane } };

        controller.FloatPane(pane);
        Assert.Equal(SkyDockZone.Float, pane.Zone);
        Assert.Single(controller.GetFloatingPanes());

        controller.DockPane(pane, SkyDockZone.Right);
        Assert.Equal(SkyDockZone.Right, pane.Zone);
        Assert.Empty(controller.GetFloatingPanes());
    }

    [Fact]
    public void SkyDockManagerController_dock_to_document_zone_throws()
    {
        var pane = new SkyDockPane("X", null, SkyDockZone.Left);
        var controller = new SkyDockManagerController();
        Assert.Throws<ArgumentException>(() => controller.DockPane(pane, SkyDockZone.Document));
    }

    [Fact]
    public void SkyDockLayoutSerializer_round_trips_json()
    {
        var state = new SkyDockLayoutState
        {
            LeftToolPaneWidth = 300,
            ToolPanes =
            [
                new SkyDockPaneLayoutEntry
                {
                    Id = Guid.NewGuid(),
                    Title = "Explorer",
                    Zone = SkyDockZone.Left,
                    PreferredLength = 280,
                },
            ],
        };

        var json = SkyDockLayoutSerializer.ToJson(state);
        var restored = SkyDockLayoutSerializer.TryFromJson(json);
        Assert.NotNull(restored);
        Assert.Equal(300, restored!.LeftToolPaneWidth);
        Assert.Equal("Explorer", restored.ToolPanes[0].Title);
    }

    [Fact]
    public void SkyDockManager_register_and_remove_tool_pane()
    {
        var panes = new ObservableCollection<SkyDockPane>();
        var manager = new SkyDockManager { ToolPanes = panes };
        var pane = new SkyDockPane("Toolbox", new TextBlock(), SkyDockZone.Left);

        Assert.True(manager.RegisterToolPane(pane));
        Assert.Single(panes);
        Assert.True(manager.RemoveToolPane(pane));
        Assert.Empty(panes);
    }

    [Fact]
    public void SkyDockManager_capture_and_restore_layout()
    {
        var panes = new ObservableCollection<SkyDockPane>
        {
            new(Guid.NewGuid(), "Saved", null, SkyDockZone.Left) { PreferredLength = 240 },
        };
        var doc = new SkyDocument("Doc") { ShellPlacement = SkyDocumentShellPlacement.Bottom };
        var manager = new SkyDockManager
        {
            ToolPanes = panes,
            Documents = new ObservableCollection<ISkyDocument> { doc },
            LeftToolPaneWidth = 240,
            RightToolPaneWidth = 260,
            BottomToolPaneHeight = 180,
        };

        var json = manager.CaptureLayoutJson();
        manager.LeftToolPaneWidth = 100;
        doc.ShellPlacement = SkyDocumentShellPlacement.Central;
        Assert.True(manager.TryRestoreLayoutJson(json));
        Assert.Equal(240, manager.LeftToolPaneWidth);
        Assert.Equal(SkyDocumentShellPlacement.Bottom, doc.ShellPlacement);
    }

    [Fact]
    public void SkyDockManager_float_tool_pane_updates_zone()
    {
        var pane = new SkyDockPane("Palette", new TextBlock(), SkyDockZone.Right);
        var manager = new SkyDockManager { ToolPanes = new ObservableCollection<SkyDockPane> { pane } };
        manager.FloatToolPane(pane);
        Assert.Equal(SkyDockZone.Float, pane.Zone);
    }

    [Fact]
    public void SkyDockManagerController_try_add_rejects_duplicate_id()
    {
        var pane = new SkyDockPane("One", null, SkyDockZone.Left);
        var list = new ObservableCollection<SkyDockPane> { pane };
        var controller = new SkyDockManagerController { ToolPanes = list };
        Assert.False(controller.TryAddPane(pane, list));
    }

    [Fact]
    public void SkyDockManagerController_capture_layout_excludes_document_zone()
    {
        var controller = new SkyDockManagerController
        {
            ToolPanes = new ObservableCollection<SkyDockPane>
            {
                new("Tool", null, SkyDockZone.Left),
            },
        };

        var layout = controller.CaptureLayout(200, 200, 120);
        Assert.All(layout.ToolPanes, e => Assert.NotEqual(SkyDockZone.Document, e.Zone));
    }
}
