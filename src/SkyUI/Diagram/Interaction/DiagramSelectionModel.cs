using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Interaction;

public sealed class DiagramSelectionModel : IDiagramSelection
{
    private readonly List<string> _nodes = new();
    private string? _edgeId;

    public IReadOnlyList<string> SelectedNodeIds => _nodes;

    public string? SelectedEdgeId => _edgeId;

    public event EventHandler? SelectionChanged;

    public void SelectNode(string nodeId, bool additive = false)
    {
        if (!additive)
            _nodes.Clear();
        if (!_nodes.Contains(nodeId))
            _nodes.Add(nodeId);
        _edgeId = null;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectEdge(string? edgeId)
    {
        _nodes.Clear();
        _edgeId = edgeId;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _nodes.Clear();
        _edgeId = null;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
