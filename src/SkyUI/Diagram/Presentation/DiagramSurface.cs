using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using SkyUI.Diagram.Contracts;
using SkyUI.Diagram.HitTesting;
using SkyUI.Diagram.Interaction;
using SkyUI.Diagram.Model;
using SkyUI.Diagram.Routing;

namespace SkyUI.Diagram.Presentation;

/// <summary>
/// Interactive diagram canvas: nodes, edges, selection, drag, resize, connect, reconnect, copy/paste.
/// Depends on abstractions <see cref="IEdgePathComputer"/>, <see cref="IDiagramNodePresenterFactory"/>,
/// <see cref="IDiagramSceneHitTester"/> (DIP); extend behavior via those types (OCP).
/// </summary>
public sealed class DiagramSurface : Panel
{
    public static readonly StyledProperty<DiagramModel?> ModelProperty =
        AvaloniaProperty.Register<DiagramSurface, DiagramModel?>(nameof(Model));

    public static readonly StyledProperty<IEdgePathComputer?> PathComputerProperty =
        AvaloniaProperty.Register<DiagramSurface, IEdgePathComputer?>(nameof(PathComputer));

    public static readonly StyledProperty<IDiagramNodePresenterFactory?> NodeFactoryProperty =
        AvaloniaProperty.Register<DiagramSurface, IDiagramNodePresenterFactory?>(nameof(NodeFactory));

    /// Holds diagram nodes and edge presenters; edges use a higher <see cref="Visual.ZIndex"/> so they draw on top
    /// without a separate full-size layer that would block node input.
    private readonly Canvas _nodesCanvas = new() { Background = Brushes.Transparent, IsHitTestVisible = true };
    private readonly Canvas _overlayCanvas = new() { Background = Brushes.Transparent, IsHitTestVisible = false };

    private const int EdgePresenterZIndex = 10;
    private const int SelectedNodePresenterZIndex = 30;

    private readonly Dictionary<string, DiagramNodePresenter> _nodeViews = new();
    private readonly Dictionary<string, DiagramEdgePresenter> _edgeViews = new();

    private readonly IDiagramSceneHitTester _hitTester = new DiagramSceneHitTester();
    private Line? _previewLine;

    private enum DragKind { None, MoveNode, ResizeNode, Connect, Reconnect }
    private DragKind _drag;
    private Point _pressDiagramPoint;
    private Rect _moveStartBounds;
    private Rect _resizeStartBounds;
    private string? _moveNodeId;
    private ResizeHandle _resizeHandle;
    private string? _connectFromNode;
    private string? _connectFromPort;
    private string? _reconnectEdgeId;
    private bool _reconnectSourceEnd;
    private bool _dragHooks;
    private DiagramModel? _wiredModel;

    private List<NodeClipboardEntry>? _nodeClipboard;
    private List<EdgeClipboardEntry>? _edgeClipboard;

    private sealed class NodeClipboardEntry
    {
        public required string OldId { get; init; }
        public required string NodeTypeKey { get; init; }
        public required string Label { get; init; }
        public required Rect Bounds { get; init; }
    }

    private sealed class EdgeClipboardEntry
    {
        public required string SourceOldId { get; init; }
        public required string SourcePortId { get; init; }
        public required string TargetOldId { get; init; }
        public required string TargetPortId { get; init; }
    }

    static DiagramSurface()
    {
        ModelProperty.Changed.AddClassHandler<DiagramSurface>((s, _) => s.AttachModel());
        PathComputerProperty.Changed.AddClassHandler<DiagramSurface>((s, _) => s.UpdateAllEdges());
    }

    public DiagramSurface()
    {
        PathComputer = new StraightEdgePathComputer();
        NodeFactory = new DefaultDiagramNodePresenterFactory();
        Focusable = true;
        Background = new SolidColorBrush(Color.Parse("#121212"));
        Children.Add(_nodesCanvas);
        Children.Add(_overlayCanvas);
        Selection.SelectionChanged += OnSelectionChanged;
        _nodesCanvas.PointerPressed += OnNodesCanvasPointerPressed;
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public DiagramModel? Model
    {
        get => GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public IEdgePathComputer? PathComputer
    {
        get => GetValue(PathComputerProperty);
        set => SetValue(PathComputerProperty, value);
    }

    public IDiagramNodePresenterFactory? NodeFactory
    {
        get => GetValue(NodeFactoryProperty);
        set => SetValue(NodeFactoryProperty, value);
    }

    public DiagramSelectionModel Selection { get; } = new();

    public void SelectNode(string nodeId, bool additive) => Selection.SelectNode(nodeId, additive);

    public void SelectEdge(string edgeId) => Selection.SelectEdge(edgeId);

    internal void SyncNodeCanvasPosition(DiagramNodePresenter p)
    {
        var n = p.Model;
        Canvas.SetLeft(p, n.Bounds.X);
        Canvas.SetTop(p, n.Bounds.Y);
        p.Width = n.Bounds.Width;
        p.Height = n.Bounds.Height;

        var needed = ComputeContentSize();
        if (needed.Width > Bounds.Width + 2 || needed.Height > Bounds.Height + 2)
            InvalidateMeasure();

        UpdateAllEdges();
    }

    public void BeginMoveNode(DiagramNode node, PointerPressedEventArgs e)
    {
        if (Model == null)
            return;
        _drag = DragKind.MoveNode;
        _moveNodeId = node.Id;
        _moveStartBounds = node.Bounds;
        _pressDiagramPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
        HookDrag();
    }

    public void BeginResizeNode(DiagramNode node, ResizeHandle handle, PointerPressedEventArgs e)
    {
        if (Model == null)
            return;
        _drag = DragKind.ResizeNode;
        _moveNodeId = node.Id;
        _resizeHandle = handle;
        _resizeStartBounds = node.Bounds;
        _pressDiagramPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
        HookDrag();
    }

    public void BeginNewConnection(string nodeId, string portId, PointerPressedEventArgs e)
    {
        if (Model == null)
            return;
        _drag = DragKind.Connect;
        _connectFromNode = nodeId;
        _connectFromPort = portId;
        _pressDiagramPoint = e.GetPosition(this);
        EnsurePreviewLine();
        e.Pointer.Capture(this);
        HookDrag();
    }

    public void BeginReconnectEdge(IDiagramEdge edge, bool sourceEnd, PointerPressedEventArgs e)
    {
        if (Model == null)
            return;
        _drag = DragKind.Reconnect;
        _reconnectEdgeId = edge.Id;
        _reconnectSourceEnd = sourceEnd;
        _pressDiagramPoint = e.GetPosition(this);
        EnsurePreviewLine();
        e.Pointer.Capture(this);
        HookDrag();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var sz = ComputeContentSize();
        _nodesCanvas.Measure(sz);
        _overlayCanvas.Measure(sz);
        return sz;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var sz = ComputeContentSize();
        var w = Math.Max(finalSize.Width, sz.Width);
        var h = Math.Max(finalSize.Height, sz.Height);
        var use = new Size(w, h);
        _nodesCanvas.Arrange(new Rect(use));
        _overlayCanvas.Arrange(new Rect(use));
        UpdateAllEdges();
        return use;
    }

    private void AttachModel()
    {
        if (_wiredModel != null)
        {
            _wiredModel.Nodes.CollectionChanged -= OnNodesCollectionChanged;
            _wiredModel.Edges.CollectionChanged -= OnEdgesCollectionChanged;
            _wiredModel.Changed -= OnModelChanged;
            _wiredModel = null;
        }

        foreach (var v in _nodeViews.Values)
            _nodesCanvas.Children.Remove(v);
        foreach (var v in _edgeViews.Values)
            _nodesCanvas.Children.Remove(v);
        _nodeViews.Clear();
        _edgeViews.Clear();
        ClearPreview();

        if (Model == null)
        {
            InvalidateMeasure();
            return;
        }

        _wiredModel = Model;
        _wiredModel.Nodes.CollectionChanged += OnNodesCollectionChanged;
        _wiredModel.Edges.CollectionChanged += OnEdgesCollectionChanged;
        _wiredModel.Changed += OnModelChanged;

        foreach (var n in Model.Nodes)
            AddNodeView((DiagramNode)n);
        foreach (var edge in Model.Edges)
            AddEdgeView((DiagramEdge)edge);

        InvalidateMeasure();
    }

    private void OnModelChanged(object? sender, DiagramModelChangedEventArgs e)
    {
        if (e.Kind is DiagramModelChangeKind.NodeLayout or DiagramModelChangeKind.Structure or DiagramModelChangeKind.Edge)
            UpdateAllEdges();
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Model == null)
            return;
        if (e.NewItems != null)
        {
            foreach (DiagramNode n in e.NewItems)
                AddNodeView(n);
        }

        if (e.OldItems != null)
        {
            foreach (DiagramNode n in e.OldItems)
                RemoveNodeView(n.Id);
        }

        InvalidateMeasure();
        UpdateAllEdges();
    }

    private void OnEdgesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Model == null)
            return;
        if (e.NewItems != null)
        {
            foreach (DiagramEdge edge in e.NewItems)
                AddEdgeView(edge);
        }

        if (e.OldItems != null)
        {
            foreach (DiagramEdge edge in e.OldItems)
                RemoveEdgeView(edge.Id);
        }

        InvalidateMeasure();
        UpdateAllEdges();
    }

    private void AddNodeView(DiagramNode node)
    {
        if (NodeFactory == null)
            return;
        var v = (DiagramNodePresenter)NodeFactory.CreatePresenter(this, node);
        _nodeViews[node.Id] = v;
        v.ZIndex = 0;
        _nodesCanvas.Children.Add(v);
        SyncNodeCanvasPosition(v);
    }

    private void RemoveNodeView(string id)
    {
        if (_nodeViews.Remove(id, out var v))
            _nodesCanvas.Children.Remove(v);
    }

    private void AddEdgeView(DiagramEdge edge)
    {
        var router = PathComputer ?? new StraightEdgePathComputer();
        var ev = new DiagramEdgePresenter(this, edge, router);
        _edgeViews[edge.Id] = ev;
        ev.ZIndex = EdgePresenterZIndex;
        _nodesCanvas.Children.Add(ev);
        UpdateAllEdges();
    }

    private void RemoveEdgeView(string id)
    {
        if (_edgeViews.Remove(id, out var v))
            _nodesCanvas.Children.Remove(v);
    }

    private void UpdateAllEdges()
    {
        if (Model == null)
            return;
        var sz = ComputeContentSize();
        foreach (var ev in _edgeViews.Values)
            ev.UpdateGeometry(Model, sz);
    }

    private Size ComputeContentSize()
    {
        if (Model == null)
            return new Size(480, 320);
        double w = 480, h = 320;
        foreach (var n in Model.Nodes)
        {
            w = Math.Max(w, n.Bounds.Right + 48);
            h = Math.Max(h, n.Bounds.Bottom + 48);
        }

        return new Size(w, h);
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        foreach (var kv in _nodeViews)
        {
            var sel = Selection.SelectedNodeIds.Contains(kv.Key);
            kv.Value.SetSelected(sel);
            kv.Value.ZIndex = sel ? SelectedNodePresenterZIndex : 0;
        }

        foreach (var kv in _edgeViews)
            kv.Value.SetSelected(Selection.SelectedEdgeId == kv.Key);
    }

    private void OnNodesCanvasPointerPressed(object? sender, PointerPressedEventArgs e) =>
        OnDiagramCanvasBackgroundPressed(e, _nodesCanvas);

    private void OnDiagramCanvasBackgroundPressed(PointerPressedEventArgs e, Canvas canvas)
    {
        if (!ReferenceEquals(e.Source, canvas) || Model == null)
            return;
        var p = e.GetPosition(this);
        var hit = _hitTester.HitTest(p, Model, (IDiagramSelection)Selection);
        switch (hit.Kind)
        {
            case DiagramHitKind.Canvas:
                Selection.Clear();
                Focus();
                e.Handled = true;
                break;
            case DiagramHitKind.NodeBody when hit.NodeId is { } nid:
                SelectNode(nid, (e.KeyModifiers & KeyModifiers.Shift) != 0);
                Focus();
                e.Handled = true;
                break;
            case DiagramHitKind.Edge when hit.EdgeId is { } eid:
                SelectEdge(eid);
                Focus();
                e.Handled = true;
                break;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (Model == null)
            return;
        if (e.Source is TextBox tb && tb.FindAncestorOfType<DiagramNodePresenter>() != null)
            return;

        var chord = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (chord && e.Key == Key.C)
        {
            CopySelectionToClipboard();
            e.Handled = true;
            return;
        }

        if (chord && e.Key == Key.V)
        {
            PasteClipboard();
            e.Handled = true;
            return;
        }

        if (e.Key != Key.Delete && e.Key != Key.Back)
            return;
        foreach (var id in Selection.SelectedNodeIds.ToArray())
            Model.RemoveNode(id);
        if (Selection.SelectedEdgeId is { } eid)
            Model.RemoveEdge(eid);
        Selection.Clear();
        e.Handled = true;
    }

    private void CopySelectionToClipboard()
    {
        if (Model == null)
            return;
        var sel = Selection.SelectedNodeIds;
        if (sel.Count == 0)
        {
            _nodeClipboard = null;
            _edgeClipboard = null;
            return;
        }

        var idSet = new HashSet<string>(sel, StringComparer.Ordinal);
        _nodeClipboard = new List<NodeClipboardEntry>(sel.Count);
        foreach (var id in sel)
        {
            if (Model.FindNode(id) is not DiagramNode n)
                continue;
            _nodeClipboard.Add(new NodeClipboardEntry
            {
                OldId = n.Id,
                NodeTypeKey = n.NodeTypeKey,
                Label = n.Label,
                Bounds = n.Bounds,
            });
        }

        _edgeClipboard = new List<EdgeClipboardEntry>();
        foreach (var edge in Model.Edges)
        {
            if (!idSet.Contains(edge.SourceNodeId) || !idSet.Contains(edge.TargetNodeId))
                continue;
            _edgeClipboard.Add(new EdgeClipboardEntry
            {
                SourceOldId = edge.SourceNodeId,
                SourcePortId = edge.SourcePortId,
                TargetOldId = edge.TargetNodeId,
                TargetPortId = edge.TargetPortId,
            });
        }
    }

    private void PasteClipboard()
    {
        if (Model == null || _nodeClipboard == null || _nodeClipboard.Count == 0)
            return;
        const double off = 28;
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in _nodeClipboard)
        {
            var r = new Rect(entry.Bounds.X + off, entry.Bounds.Y + off, entry.Bounds.Width, entry.Bounds.Height);
            var n = Model.AddNode(entry.NodeTypeKey, entry.Label, r);
            map[entry.OldId] = n.Id;
        }

        if (_edgeClipboard != null)
        {
            foreach (var ed in _edgeClipboard)
            {
                if (map.TryGetValue(ed.SourceOldId, out var s) && map.TryGetValue(ed.TargetOldId, out var t))
                    Model.TryAddEdge(s, ed.SourcePortId, t, ed.TargetPortId);
            }
        }

        Selection.Clear();
        var first = true;
        foreach (var entry in _nodeClipboard)
        {
            if (!map.TryGetValue(entry.OldId, out var nid))
                continue;
            Selection.SelectNode(nid, additive: !first);
            first = false;
        }

        Focus();
    }

    private void HookDrag()
    {
        if (_dragHooks)
            return;
        PointerMoved += OnSurfacePointerMoved;
        PointerReleased += OnSurfacePointerReleased;
        _dragHooks = true;
    }

    private void UnhookDrag()
    {
        if (!_dragHooks)
            return;
        PointerMoved -= OnSurfacePointerMoved;
        PointerReleased -= OnSurfacePointerReleased;
        _dragHooks = false;
    }

    private void OnSurfacePointerMoved(object? sender, PointerEventArgs e)
    {
        if (Model == null)
            return;
        var p = e.GetPosition(this);
        switch (_drag)
        {
            case DragKind.MoveNode when _moveNodeId != null && Model.FindNode(_moveNodeId) is DiagramNode mn:
            {
                var delta = p - _pressDiagramPoint;
                var r = _moveStartBounds;
                var nx = Math.Max(0, r.X + delta.X);
                var ny = Math.Max(0, r.Y + delta.Y);
                mn.Bounds = new Rect(nx, ny, r.Width, r.Height);
                break;
            }
            case DragKind.ResizeNode when _moveNodeId != null && Model.FindNode(_moveNodeId) is DiagramNode rn:
            {
                var delta = p - _pressDiagramPoint;
                rn.Bounds = ApplyResize(_resizeStartBounds, _resizeHandle, delta, 64, 48);
                break;
            }
            case DragKind.Connect when _connectFromNode != null && _connectFromPort != null:
            {
                if (TryGetPortPoint(Model, _connectFromNode, _connectFromPort, out var a))
                    UpdatePreview(a, p);
                break;
            }
            case DragKind.Reconnect when _reconnectEdgeId != null && Model.FindEdge(_reconnectEdgeId) is { } ed:
            {
                var fixedEnd = _reconnectSourceEnd
                    ? (ed.TargetNodeId, ed.TargetPortId)
                    : (ed.SourceNodeId, ed.SourcePortId);
                if (TryGetPortPoint(Model, fixedEnd.Item1, fixedEnd.Item2, out var a))
                    UpdatePreview(a, p);
                break;
            }
        }
    }

    private void OnSurfacePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (Model != null && _drag != DragKind.None)
            {
                var p = e.GetPosition(this);
                switch (_drag)
                {
                    case DragKind.Connect when _connectFromNode != null && _connectFromPort != null:
                    {
                        var hit = _hitTester.HitTest(p, Model, (IDiagramSelection)Selection);
                        if (hit.NodeId != null && hit.NodeId != _connectFromNode)
                        {
                            if (hit.Kind == DiagramHitKind.Port && hit.PortId != null)
                                Model.TryAddEdge(_connectFromNode, _connectFromPort, hit.NodeId, hit.PortId);
                            else if (hit.Kind == DiagramHitKind.NodeBody &&
                                     TryGetNearestPort(Model, hit.NodeId, p, 56 * 56, out var nearPort))
                                Model.TryAddEdge(_connectFromNode, _connectFromPort, hit.NodeId, nearPort);
                        }

                        break;
                    }
                    case DragKind.Reconnect when _reconnectEdgeId != null:
                    {
                        var hit = _hitTester.HitTest(p, Model, (IDiagramSelection)Selection);
                        if (hit.Kind == DiagramHitKind.Port && hit.NodeId != null && hit.PortId != null)
                            Model.TrySetEdgeEndpoint(_reconnectEdgeId, _reconnectSourceEnd, hit.NodeId, hit.PortId);
                        else if (hit.Kind == DiagramHitKind.NodeBody && hit.NodeId != null &&
                                 TryGetNearestPort(Model, hit.NodeId, p, 56 * 56, out var np))
                            Model.TrySetEdgeEndpoint(_reconnectEdgeId, _reconnectSourceEnd, hit.NodeId, np);
                        break;
                    }
                }
            }
        }
        finally
        {
            e.Pointer.Capture(null);
            UnhookDrag();
            _drag = DragKind.None;
            _moveNodeId = null;
            _connectFromNode = null;
            _connectFromPort = null;
            _reconnectEdgeId = null;
            ClearPreview();
        }
    }

    private static Rect ApplyResize(Rect rect, ResizeHandle handle, Vector delta, double minW, double minH)
    {
        var x = rect.X;
        var y = rect.Y;
        var w = rect.Width;
        var h = rect.Height;
        var dx = delta.X;
        var dy = delta.Y;

        switch (handle)
        {
            case ResizeHandle.Left:
            {
                var nw = w - dx;
                if (nw >= minW)
                {
                    x += dx;
                    w = nw;
                }

                break;
            }
            case ResizeHandle.Right:
                w = Math.Max(minW, w + dx);
                break;
            case ResizeHandle.Top:
            {
                var nh = h - dy;
                if (nh >= minH)
                {
                    y += dy;
                    h = nh;
                }

                break;
            }
            case ResizeHandle.Bottom:
                h = Math.Max(minH, h + dy);
                break;
            case ResizeHandle.TopLeft:
            {
                var nw = w - dx;
                var nh = h - dy;
                if (nw >= minW && nh >= minH)
                {
                    x += dx;
                    y += dy;
                    w = nw;
                    h = nh;
                }
                else if (nw >= minW)
                {
                    x += dx;
                    w = nw;
                }
                else if (nh >= minH)
                {
                    y += dy;
                    h = nh;
                }

                break;
            }
            case ResizeHandle.TopRight:
            {
                w = Math.Max(minW, w + dx);
                var nh = h - dy;
                if (nh >= minH)
                {
                    y += dy;
                    h = nh;
                }

                break;
            }
            case ResizeHandle.BottomLeft:
            {
                var nw = w - dx;
                if (nw >= minW)
                {
                    x += dx;
                    w = nw;
                }

                h = Math.Max(minH, h + dy);
                break;
            }
            case ResizeHandle.BottomRight:
                w = Math.Max(minW, w + dx);
                h = Math.Max(minH, h + dy);
                break;
        }

        return new Rect(x, y, w, h);
    }

    private static bool TryGetNearestPort(DiagramModel model, string nodeId, Point p, double maxDistSq, out string portId)
    {
        portId = string.Empty;
        if (model.FindNode(nodeId) is not DiagramNode n)
            return false;
        var bestD2 = double.PositiveInfinity;
        string? bestId = null;
        foreach (var port in n.Ports)
        {
            var c = port.GetWorldPosition(n.Bounds);
            var dx = p.X - c.X;
            var dy = p.Y - c.Y;
            var d2 = dx * dx + dy * dy;
            if (d2 < bestD2)
            {
                bestD2 = d2;
                bestId = port.Id;
            }
        }

        if (bestId != null && bestD2 <= maxDistSq)
        {
            portId = bestId;
            return true;
        }

        return false;
    }

    private static bool TryGetPortPoint(DiagramModel model, string nodeId, string portId, out Point pt)
    {
        pt = default;
        if (model.FindNode(nodeId) is not DiagramNode n)
            return false;
        var sp = n.Ports.FirstOrDefault(p => p.Id == portId);
        if (sp == null)
            return false;
        pt = sp.GetWorldPosition(n.Bounds);
        return true;
    }

    private void EnsurePreviewLine()
    {
        if (_previewLine != null)
            return;
        _previewLine = new Line
        {
            Stroke = new SolidColorBrush(Color.Parse("#888")),
            StrokeThickness = 2,
            StrokeDashArray = new AvaloniaList<double> { 4, 4 },
            IsHitTestVisible = false,
            ZIndex = 100,
        };
        _overlayCanvas.Children.Add(_previewLine);
    }

    private void UpdatePreview(Point a, Point b)
    {
        EnsurePreviewLine();
        _previewLine!.StartPoint = a;
        _previewLine.EndPoint = b;
    }

    private void ClearPreview()
    {
        if (_previewLine == null)
            return;
        _overlayCanvas.Children.Remove(_previewLine);
        _previewLine = null;
    }
}
