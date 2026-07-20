using Avalonia;

namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Maps pointer coordinates to diagram semantics (DIP: interaction depends on abstraction).
/// </summary>
public interface IDiagramSceneHitTester
{
    DiagramHitResult HitTest(Point diagramPoint, IDiagramModel model, IDiagramSelection selection);
}

public readonly record struct DiagramHitResult(DiagramHitKind Kind, string? NodeId, string? PortId, string? EdgeId, ResizeHandle ResizeHandle)
{
    public static DiagramHitResult Canvas() => new(DiagramHitKind.Canvas, null, null, null, ResizeHandle.None);

    public static DiagramHitResult NodeBody(string nodeId) => new(DiagramHitKind.NodeBody, nodeId, null, null, ResizeHandle.None);

    public static DiagramHitResult Port(string nodeId, string portId) => new(DiagramHitKind.Port, nodeId, portId, null, ResizeHandle.None);

    public static DiagramHitResult Edge(string edgeId) => new(DiagramHitKind.Edge, null, null, edgeId, ResizeHandle.None);

    public static DiagramHitResult Resize(string nodeId, ResizeHandle handle) =>
        new(DiagramHitKind.ResizeGrip, nodeId, null, null, handle);
}

public enum DiagramHitKind
{
    Canvas,
    NodeBody,
    Port,
    Edge,
    ResizeGrip,
}

public enum ResizeHandle
{
    None,
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
}
