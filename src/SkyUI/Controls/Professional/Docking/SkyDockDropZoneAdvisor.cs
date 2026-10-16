using Avalonia;

namespace SkyUI.Controls.Professional;

/// <summary>Maps pointer positions to dock zones for drag-drop (pure logic, unit-tested).</summary>
public static class SkyDockDropZoneAdvisor
{
    public const double DefaultEdgeRatio = 0.18;

    public static SkyDockZone? Resolve(Rect bounds, Point point, double edgeRatio = DefaultEdgeRatio)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return null;
        if (!bounds.Contains(point))
            return null;

        var relX = (point.X - bounds.Left) / bounds.Width;
        var relY = (point.Y - bounds.Top) / bounds.Height;
        return ResolveFromRelative(relX, relY, edgeRatio);
    }

    /// <summary>
    /// Resolves dock targets from the document host rectangle (center), with outer-manager fallback.
    /// </summary>
    public static SkyDockZone? ResolveForDockHost(
        Rect managerBounds,
        Rect documentBounds,
        Point pointInManager,
        double edgeRatio = DefaultEdgeRatio)
    {
        if (managerBounds.Width <= 0 || managerBounds.Height <= 0)
            return null;

        if (documentBounds.Width > 0 && documentBounds.Height > 0 && documentBounds.Contains(pointInManager))
        {
            var relX = (pointInManager.X - documentBounds.Left) / documentBounds.Width;
            var relY = (pointInManager.Y - documentBounds.Top) / documentBounds.Height;
            return ResolveFromRelative(relX, relY, edgeRatio);
        }

        return Resolve(managerBounds, pointInManager, edgeRatio);
    }

    private static SkyDockZone ResolveFromRelative(double relX, double relY, double edgeRatio)
    {
        if (relX < edgeRatio)
            return SkyDockZone.Left;
        if (relX > 1 - edgeRatio)
            return SkyDockZone.Right;
        if (relY > 1 - edgeRatio)
            return SkyDockZone.Bottom;
        return SkyDockZone.Float;
    }
}
