using Avalonia;
using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Model;

public sealed class DiagramPort : IDiagramPort
{
    public DiagramPort(string id, string ownerNodeId, PortSide side, double offsetAlongSide = 0.5)
    {
        Id = id;
        OwnerNodeId = ownerNodeId;
        Side = side;
        OffsetAlongSide = offsetAlongSide;
    }

    public string Id { get; }

    public string OwnerNodeId { get; }

    public double OffsetAlongSide { get; }

    public PortSide Side { get; }

    public Point GetWorldPosition(Rect nodeBounds)
    {
        var t = Math.Clamp(OffsetAlongSide, 0, 1);
        return Side switch
        {
            PortSide.Left => new Point(nodeBounds.Left, nodeBounds.Top + t * nodeBounds.Height),
            PortSide.Right => new Point(nodeBounds.Right, nodeBounds.Top + t * nodeBounds.Height),
            PortSide.Top => new Point(nodeBounds.Left + t * nodeBounds.Width, nodeBounds.Top),
            PortSide.Bottom => new Point(nodeBounds.Left + t * nodeBounds.Width, nodeBounds.Bottom),
            _ => nodeBounds.Center,
        };
    }
}
