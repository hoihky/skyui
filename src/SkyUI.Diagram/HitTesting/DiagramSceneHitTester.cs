using System.Linq;
using Avalonia;
using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.HitTesting;

/// <summary>
/// Geometry-first hit testing over the model (LSP: alternative testers can swap strategy).
/// </summary>
public sealed class DiagramSceneHitTester : IDiagramSceneHitTester
{
    private const double PortRadius = 24;
    private const double EdgeHitTolerance = 8;
    private const double Grip = 6;

    private static readonly ResizeHandle[] ResizeOrder =
    [
        ResizeHandle.TopLeft, ResizeHandle.Top, ResizeHandle.TopRight, ResizeHandle.Right,
        ResizeHandle.BottomRight, ResizeHandle.Bottom, ResizeHandle.BottomLeft, ResizeHandle.Left,
    ];

    public DiagramHitResult HitTest(Point diagramPoint, IDiagramModel model, IDiagramSelection selection)
    {
        if (TryHitResize(diagramPoint, model, selection, out var resize))
            return resize;

        if (TryHitPort(diagramPoint, model, out var port))
            return port;

        if (TryHitEdge(diagramPoint, model, out var edgeId))
            return DiagramHitResult.Edge(edgeId);

        if (TryHitNodeBody(diagramPoint, model, out var nodeId))
            return DiagramHitResult.NodeBody(nodeId);

        return DiagramHitResult.Canvas();
    }

    private static bool TryHitResize(Point p, IDiagramModel model, IDiagramSelection selection, out DiagramHitResult result)
    {
        result = default;
        if (selection.SelectedNodeIds.Count != 1)
            return false;
        var id = selection.SelectedNodeIds[0];
        var node = model.FindNode(id);
        if (node == null)
            return false;
        var b = node.Bounds;
        foreach (var h in ResizeOrder)
        {
            if (!TryGrip(p, b, h, out var matched))
                continue;
            result = DiagramHitResult.Resize(id, matched);
            return true;
        }

        return false;
    }

    private static bool TryGrip(Point p, Rect bounds, ResizeHandle expected, out ResizeHandle handle)
    {
        var anchor = expected switch
        {
            ResizeHandle.TopLeft => bounds.TopLeft,
            ResizeHandle.Top => new Point(bounds.Center.X, bounds.Top),
            ResizeHandle.TopRight => bounds.TopRight,
            ResizeHandle.Right => new Point(bounds.Right, bounds.Center.Y),
            ResizeHandle.BottomRight => bounds.BottomRight,
            ResizeHandle.Bottom => new Point(bounds.Center.X, bounds.Bottom),
            ResizeHandle.BottomLeft => bounds.BottomLeft,
            ResizeHandle.Left => new Point(bounds.Left, bounds.Center.Y),
            _ => default,
        };
        var rect = new Rect(anchor.X - Grip * 0.5, anchor.Y - Grip * 0.5, Grip, Grip);
        if (rect.Contains(p))
        {
            handle = expected;
            return true;
        }

        handle = ResizeHandle.None;
        return false;
    }

    private static bool TryHitPort(Point p, IDiagramModel model, out DiagramHitResult result)
    {
        result = default;
        foreach (var node in model.Nodes.Reverse())
        {
            foreach (var port in node.Ports)
            {
                var c = port.GetWorldPosition(node.Bounds);
                var dx = p.X - c.X;
                var dy = p.Y - c.Y;
                if (dx * dx + dy * dy <= PortRadius * PortRadius)
                {
                    result = DiagramHitResult.Port(node.Id, port.Id);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryHitEdge(Point p, IDiagramModel model, out string edgeId)
    {
        edgeId = string.Empty;
        var router = new Routing.StraightEdgePathComputer();
        foreach (var edge in model.Edges.Reverse())
        {
            var (a, b) = router.GetEndpoints(edge, model);
            if (DistanceToSegmentSquared(p, a, b) <= EdgeHitTolerance * EdgeHitTolerance)
            {
                edgeId = edge.Id;
                return true;
            }
        }

        return false;
    }

    private static bool TryHitNodeBody(Point p, IDiagramModel model, out string nodeId)
    {
        nodeId = string.Empty;
        foreach (var node in model.Nodes.Reverse())
        {
            if (node.Bounds.Contains(p))
            {
                nodeId = node.Id;
                return true;
            }
        }

        return false;
    }

    private static double DistanceToSegmentSquared(Point p, Point a, Point b)
    {
        var abx = b.X - a.X;
        var aby = b.Y - a.Y;
        var apx = p.X - a.X;
        var apy = p.Y - a.Y;
        var abLen2 = abx * abx + aby * aby;
        if (abLen2 < double.Epsilon)
            return apx * apx + apy * apy;
        var t = Math.Clamp((apx * abx + apy * aby) / abLen2, 0, 1);
        var px = a.X + abx * t;
        var py = a.Y + aby * t;
        var dx = p.X - px;
        var dy = p.Y - py;
        return dx * dx + dy * dy;
    }
}
