namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Directed link between two ports (SRP: edge does not own layout geometry; routing is separate).
/// </summary>
public interface IDiagramEdge : IDiagramElement
{
    string SourceNodeId { get; }

    string SourcePortId { get; }

    string TargetNodeId { get; }

    string TargetPortId { get; }
}
