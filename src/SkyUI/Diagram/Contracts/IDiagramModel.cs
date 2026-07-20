using System.Collections.Generic;

namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Mutable graph document (DIP: presenters depend on this abstraction, not concrete storage).
/// </summary>
public interface IDiagramModel
{
    IEnumerable<IDiagramNode> Nodes { get; }

    IEnumerable<IDiagramEdge> Edges { get; }

    IDiagramNode? FindNode(string nodeId);

    IDiagramEdge? FindEdge(string edgeId);

    IDiagramNode AddNode(string nodeTypeKey, string label, Avalonia.Rect initialBounds);

    bool RemoveNode(string nodeId);

    IDiagramEdge? TryAddEdge(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId);

    bool RemoveEdge(string edgeId);

    /// <summary>Reconnect one endpoint of an edge to another port.</summary>
    bool TrySetEdgeEndpoint(string edgeId, bool sourceEnd, string newNodeId, string newPortId);

    event EventHandler<DiagramModelChangedEventArgs>? Changed;
}

public sealed class DiagramModelChangedEventArgs : EventArgs
{
    public DiagramModelChangedEventArgs(DiagramModelChangeKind kind, string? elementId = null)
    {
        Kind = kind;
        ElementId = elementId;
    }

    public DiagramModelChangeKind Kind { get; }

    public string? ElementId { get; }
}

public enum DiagramModelChangeKind
{
    Structure,
    NodeLayout,
    NodeContent,
    Edge,
}
