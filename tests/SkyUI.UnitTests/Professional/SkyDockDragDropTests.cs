using System.Collections.ObjectModel;
using Avalonia;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDockDragDropTests
{
    [Fact]
    public void SkyDockDropZoneAdvisor_maps_edges_and_center()
    {
        var bounds = new Rect(0, 0, 1000, 800);
        Assert.Equal(SkyDockZone.Left, SkyDockDropZoneAdvisor.Resolve(bounds, new Point(50, 400)));
        Assert.Equal(SkyDockZone.Right, SkyDockDropZoneAdvisor.Resolve(bounds, new Point(950, 400)));
        Assert.Equal(SkyDockZone.Bottom, SkyDockDropZoneAdvisor.Resolve(bounds, new Point(500, 750)));
        Assert.Equal(SkyDockZone.Float, SkyDockDropZoneAdvisor.Resolve(bounds, new Point(500, 400)));
    }

    [Fact]
    public void SkyDockDropZoneAdvisor_uses_document_host_edges_when_pointer_inside_document()
    {
        var manager = new Rect(0, 0, 1000, 800);
        var document = new Rect(200, 0, 600, 600);
        Assert.Equal(SkyDockZone.Left, SkyDockDropZoneAdvisor.ResolveForDockHost(manager, document, new Point(230, 300)));
        Assert.Equal(SkyDockZone.Right, SkyDockDropZoneAdvisor.ResolveForDockHost(manager, document, new Point(750, 300)));
        Assert.Equal(SkyDockZone.Bottom, SkyDockDropZoneAdvisor.ResolveForDockHost(manager, document, new Point(500, 560)));
        Assert.Equal(SkyDockZone.Float, SkyDockDropZoneAdvisor.ResolveForDockHost(manager, document, new Point(500, 300)));
    }

    [Fact]
    public void SkyDockDropZoneAdvisor_returns_null_outside_bounds()
    {
        var bounds = new Rect(0, 0, 100, 100);
        Assert.Null(SkyDockDropZoneAdvisor.Resolve(bounds, new Point(-1, 50)));
    }

    [Fact]
    public void ApplyDrop_float_sets_position()
    {
        var pane = new SkyDockPane("Palette", null, SkyDockZone.Right);
        var controller = new SkyDockManagerController();
        controller.ApplyDrop(pane, SkyDockZone.Float, new Point(120, 80));
        Assert.Equal(SkyDockZone.Float, pane.Zone);
        Assert.Equal(120, pane.FloatLeft);
        Assert.Equal(80, pane.FloatTop);
        Assert.Equal(SkyDockZone.Right, pane.DockBackZone);
    }

    [Fact]
    public void ApplyDrop_same_zone_only_updates_selection()
    {
        var pane = new SkyDockPane("A", null, SkyDockZone.Left);
        var other = new SkyDockPane("B", null, SkyDockZone.Left) { IsSelected = true };
        var controller = new SkyDockManagerController
        {
            ToolPanes = new ObservableCollection<SkyDockPane> { pane, other },
        };
        controller.ApplyDrop(pane, SkyDockZone.Left, default);
        Assert.True(pane.IsSelected);
        Assert.False(other.IsSelected);
    }

    [Fact]
    public void FloatPane_remembers_dock_back_zone()
    {
        var pane = new SkyDockPane("Out", null, SkyDockZone.Bottom);
        var controller = new SkyDockManagerController();
        controller.FloatPane(pane);
        Assert.Equal(SkyDockZone.Bottom, pane.DockBackZone);
        controller.DockBack(pane);
        Assert.Equal(SkyDockZone.Bottom, pane.Zone);
    }

    [Fact]
    public void DockBack_uses_pane_dock_back_zone()
    {
        var pane = new SkyDockPane("X", null, SkyDockZone.Left) { DockBackZone = SkyDockZone.Right };
        pane.Zone = SkyDockZone.Float;
        var controller = new SkyDockManagerController();
        controller.DockBack(pane);
        Assert.Equal(SkyDockZone.Right, pane.Zone);
    }

    [Fact]
    public void Capture_layout_includes_float_coordinates()
    {
        var pane = new SkyDockPane("Float", null, SkyDockZone.Float)
        {
            FloatLeft = 40,
            FloatTop = 60,
            FloatWidth = 300,
            FloatHeight = 200,
        };
        var controller = new SkyDockManagerController
        {
            ToolPanes = new ObservableCollection<SkyDockPane> { pane },
        };

        var entry = controller.CaptureLayout(200, 200, 120).ToolPanes[0];
        Assert.Equal(40, entry.FloatLeft);
        Assert.Equal(60, entry.FloatTop);
        Assert.Equal(300, entry.FloatWidth);
    }

    [Fact]
    public void SkyDockManager_complete_dock_drag_docks_to_left_zone()
    {
        var pane = new SkyDockPane("Tool", null, SkyDockZone.Right);
        var manager = new SkyDockManager
        {
            Width = 800,
            Height = 600,
            ToolPanes = new ObservableCollection<SkyDockPane> { pane },
        };

        manager.BeginDockDrag(pane);
        manager.UpdateDockDrag(new Point(40, 300));
        manager.CompleteDockDrag(new Point(40, 300));
        Assert.Equal(SkyDockZone.Left, pane.Zone);
    }

    [Fact]
    public void SkyDockManager_cancel_dock_drag_leaves_zone_unchanged()
    {
        var pane = new SkyDockPane("Tool", null, SkyDockZone.Right);
        var manager = new SkyDockManager
        {
            Width = 800,
            Height = 600,
            ToolPanes = new ObservableCollection<SkyDockPane> { pane },
        };

        manager.BeginDockDrag(pane);
        manager.CancelDockDrag();
        Assert.Equal(SkyDockZone.Right, pane.Zone);
    }
}
