using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Model;

public sealed class DiagramModel : IDiagramModel
{
    private readonly Dictionary<string, DiagramNode> _nodes = new();
    private readonly Dictionary<string, DiagramEdge> _edges = new();

    public ObservableCollection<DiagramNode> Nodes { get; } = new();

    public ObservableCollection<DiagramEdge> Edges { get; } = new();

    IEnumerable<IDiagramNode> IDiagramModel.Nodes => Nodes;

    IEnumerable<IDiagramEdge> IDiagramModel.Edges => Edges;

    public event EventHandler<DiagramModelChangedEventArgs>? Changed;

    public IDiagramNode? FindNode(string nodeId) =>
        _nodes.TryGetValue(nodeId, out var n) ? n : null;

    public IDiagramEdge? FindEdge(string edgeId) =>
        _edges.TryGetValue(edgeId, out var e) ? e : null;

    public DiagramNode AddNode(string nodeTypeKey, string label, Rect initialBounds)
    {
        var id = Guid.NewGuid().ToString("N");
        var node = new DiagramNode(id, nodeTypeKey, label, initialBounds);
        _nodes.Add(id, node);
        Nodes.Add(node);
        node.PropertyChanged += OnNodePropertyChanged;
        Raise(DiagramModelChangeKind.Structure, id);
        return node;
    }

    IDiagramNode IDiagramModel.AddNode(string nodeTypeKey, string label, Rect initialBounds) =>
        AddNode(nodeTypeKey, label, initialBounds);

    public bool RemoveNode(string nodeId)
    {
        if (!_nodes.Remove(nodeId, out var node))
            return false;

        node.PropertyChanged -= OnNodePropertyChanged;
        Nodes.Remove(node);
        var edgesToRemove = Edges.Where(e =>
            e.SourceNodeId == nodeId || e.TargetNodeId == nodeId).ToList();
        foreach (var e in edgesToRemove)
            RemoveEdge(e.Id);
        Raise(DiagramModelChangeKind.Structure, nodeId);
        return true;
    }

    public DiagramEdge? TryAddEdge(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        if (sourceNodeId == targetNodeId)
            return null;
        if (!_nodes.ContainsKey(sourceNodeId) || !_nodes.ContainsKey(targetNodeId))
            return null;
        if (!PortExists(sourceNodeId, sourcePortId) || !PortExists(targetNodeId, targetPortId))
            return null;
        if (Edges.Any(e =>
                e.SourceNodeId == sourceNodeId && e.SourcePortId == sourcePortId &&
                e.TargetNodeId == targetNodeId && e.TargetPortId == targetPortId))
            return null;

        var id = Guid.NewGuid().ToString("N");
        var edge = new DiagramEdge(id, sourceNodeId, sourcePortId, targetNodeId, targetPortId);
        _edges.Add(id, edge);
        Edges.Add(edge);
        Raise(DiagramModelChangeKind.Edge, id);
        return edge;
    }

    IDiagramEdge? IDiagramModel.TryAddEdge(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId) =>
        TryAddEdge(sourceNodeId, sourcePortId, targetNodeId, targetPortId);

    public bool RemoveEdge(string edgeId)
    {
        if (!_edges.Remove(edgeId, out var edge))
            return false;
        Edges.Remove(edge);
        Raise(DiagramModelChangeKind.Edge, edgeId);
        return true;
    }

    public bool TrySetEdgeEndpoint(string edgeId, bool sourceEnd, string newNodeId, string newPortId)
    {
        if (!_edges.TryGetValue(edgeId, out var edge))
            return false;
        if (!_nodes.ContainsKey(newNodeId) || !PortExists(newNodeId, newPortId))
            return false;

        var newSource = sourceEnd ? newNodeId : edge.SourceNodeId;
        var newSourcePort = sourceEnd ? newPortId : edge.SourcePortId;
        var newTarget = sourceEnd ? edge.TargetNodeId : newNodeId;
        var newTargetPort = sourceEnd ? edge.TargetPortId : newPortId;
        if (newSource == newTarget)
            return false;

        if (Edges.Any(e =>
                e.Id != edgeId &&
                e.SourceNodeId == newSource && e.SourcePortId == newSourcePort &&
                e.TargetNodeId == newTarget && e.TargetPortId == newTargetPort))
            return false;

        if (sourceEnd)
        {
            edge.SourceNodeId = newNodeId;
            edge.SourcePortId = newPortId;
        }
        else
        {
            edge.TargetNodeId = newNodeId;
            edge.TargetPortId = newPortId;
        }

        Raise(DiagramModelChangeKind.Edge, edgeId);
        return true;
    }

    private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not DiagramNode n)
            return;
        if (e.PropertyName is nameof(DiagramNode.Bounds))
            Raise(DiagramModelChangeKind.NodeLayout, n.Id);
        else
            Raise(DiagramModelChangeKind.NodeContent, n.Id);
    }

    private bool PortExists(string nodeId, string portId) =>
        _nodes.TryGetValue(nodeId, out var n) && n.Ports.Any(p => p.Id == portId);

    private void Raise(DiagramModelChangeKind kind, string? elementId = null) =>
        Changed?.Invoke(this, new DiagramModelChangedEventArgs(kind, elementId));
}
