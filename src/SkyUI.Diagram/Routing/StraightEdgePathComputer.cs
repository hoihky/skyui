using System.Linq;
using Avalonia;
using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Routing;

/// <summary>
/// Straight segment between port world positions (default router; replace for orthogonal/spline flows).
/// </summary>
public sealed class StraightEdgePathComputer : IEdgePathComputer
{
    public (Point Start, Point End) GetEndpoints(IDiagramEdge edge, IDiagramModel model)
    {
        var sn = model.FindNode(edge.SourceNodeId);
        var tn = model.FindNode(edge.TargetNodeId);
        if (sn == null || tn == null)
            return (default, default);

        var sp = sn.Ports.FirstOrDefault(p => p.Id == edge.SourcePortId);
        var tp = tn.Ports.FirstOrDefault(p => p.Id == edge.TargetPortId);
        if (sp == null || tp == null)
            return (default, default);

        return (sp.GetWorldPosition(sn.Bounds), tp.GetWorldPosition(tn.Bounds));
    }
}
