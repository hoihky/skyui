namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Selection state decoupled from the model (SRP: selection vs document graph).
/// </summary>
public interface IDiagramSelection
{
    IReadOnlyList<string> SelectedNodeIds { get; }

    string? SelectedEdgeId { get; }

    void SelectNode(string nodeId, bool additive = false);

    void SelectEdge(string? edgeId);

    void Clear();

    event EventHandler? SelectionChanged;
}
