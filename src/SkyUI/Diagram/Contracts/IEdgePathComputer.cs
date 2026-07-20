using Avalonia;

namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Computes edge geometry from the model (OCP: swap straight, orthogonal, curved routers).
/// </summary>
public interface IEdgePathComputer
{
    (Point Start, Point End) GetEndpoints(IDiagramEdge edge, IDiagramModel model);
}
