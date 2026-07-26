using Avalonia;

namespace SkyUI.Diagram.Contracts;

/// <summary>
/// A logical anchor on a node where edges may attach (OCP: new node shapes implement port placement).
/// </summary>
public interface IDiagramPort : IDiagramElement
{
    string OwnerNodeId { get; }

    /// <summary>0–1 distance along the chosen side from the side's start corner.</summary>
    double OffsetAlongSide { get; }

    PortSide Side { get; }

    /// <summary>World-space attachment point for the current node layout.</summary>
    Point GetWorldPosition(Rect nodeBounds);
}
