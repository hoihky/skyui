using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Model;

public sealed class DiagramEdge : IDiagramEdge
{
    public DiagramEdge(string id, string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        Id = id;
        SourceNodeId = sourceNodeId;
        SourcePortId = sourcePortId;
        TargetNodeId = targetNodeId;
        TargetPortId = targetPortId;
    }

    public string Id { get; }

    public string SourceNodeId { get; internal set; }

    public string SourcePortId { get; internal set; }

    public string TargetNodeId { get; internal set; }

    public string TargetPortId { get; internal set; }
}
