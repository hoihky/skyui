using Avalonia;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDocumentShellTests
{
    [Fact]
    public void SkyDocumentShellController_float_remembers_dock_back()
    {
        var document = new SkyDocument("Doc") { ShellPlacement = SkyDocumentShellPlacement.Central };
        var controller = new SkyDocumentShellController();
        controller.ApplyPlacement(document, SkyDocumentShellPlacement.Float, new Point(40, 50));
        Assert.Equal(SkyDocumentShellPlacement.Float, document.ShellPlacement);
        Assert.Equal(SkyDocumentShellPlacement.Central, document.ShellDockBack);
        Assert.Equal(40, document.ShellFloatLeft);
        Assert.Equal(50, document.ShellFloatTop);
        controller.DockBack(document);
        Assert.Equal(SkyDocumentShellPlacement.Central, document.ShellPlacement);
    }

    [Fact]
    public void SkyDocumentShellController_apply_dock_zone_maps_document_to_central()
    {
        var document = new SkyDocument("Doc");
        var controller = new SkyDocumentShellController();
        controller.ApplyDockZone(document, SkyDockZone.Document, default);
        Assert.Equal(SkyDocumentShellPlacement.Central, document.ShellPlacement);
        controller.ApplyDockZone(document, SkyDockZone.Left, default);
        Assert.Equal(SkyDocumentShellPlacement.Left, document.ShellPlacement);
    }

    [Fact]
    public void SkyDockLayoutDocumentShell_round_trips_placement()
    {
        var doc = new SkyDocument("A") { ShellPlacement = SkyDocumentShellPlacement.Right };
        var list = new List<ISkyDocument> { doc };
        var captured = SkyDockLayoutDocumentShell.Capture(list);
        doc.ShellPlacement = SkyDocumentShellPlacement.Central;
        SkyDockLayoutDocumentShell.Apply(captured, list);
        Assert.Equal(SkyDocumentShellPlacement.Right, doc.ShellPlacement);
    }

    [Fact]
    public void SkyDocumentWellController_filters_by_placement()
    {
        var left = new SkyDocument("Left") { ShellPlacement = SkyDocumentShellPlacement.Left };
        var central = new SkyDocument("Central");
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new[] { left, central },
            PlacementFilter = SkyDocumentShellPlacement.Central,
        };
        Assert.Single(controller.Documents);
        Assert.Same(central, controller.Documents[0]);
        controller.PlacementFilter = SkyDocumentShellPlacement.Left;
        Assert.Single(controller.Documents);
        Assert.Same(left, controller.Documents[0]);
    }
}
