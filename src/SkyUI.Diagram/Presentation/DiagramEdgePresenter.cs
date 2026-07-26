using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using SkyUI.Diagram.Contracts;
using SkyUI.Diagram.Model;
using SkyUI.Diagram.Routing;

namespace SkyUI.Diagram.Presentation;

public sealed class DiagramEdgePresenter : Canvas
{
    private readonly Line _hitLine = new() { Stroke = Brushes.Transparent, StrokeThickness = 14, IsHitTestVisible = true };
    private readonly Line _visLine = new() { Stroke = new SolidColorBrush(Color.Parse("#6a6a6a")), StrokeThickness = 2, IsHitTestVisible = false };
    private readonly Ellipse _srcThumb = CreateThumb();
    private readonly Ellipse _tgtThumb = CreateThumb();
    private readonly DiagramSurface _host;
    private readonly DiagramEdge _edge;
    private readonly IEdgePathComputer _router;

    public DiagramEdgePresenter(DiagramSurface host, DiagramEdge edge, IEdgePathComputer router)
    {
        _host = host;
        _edge = edge;
        _router = router;
        Children.Add(_hitLine);
        Children.Add(_visLine);
        Children.Add(_srcThumb);
        Children.Add(_tgtThumb);
        _hitLine.PointerPressed += OnEdgePressed;
        _srcThumb.PointerPressed += (_, e) => OnThumbPressed(e, sourceEnd: true);
        _tgtThumb.PointerPressed += (_, e) => OnThumbPressed(e, sourceEnd: false);
    }

    public DiagramEdge Edge => _edge;

    public void SetSelected(bool selected)
    {
        _visLine.Stroke = selected ? new SolidColorBrush(Color.Parse("#1ed760")) : new SolidColorBrush(Color.Parse("#6a6a6a"));
        _srcThumb.IsVisible = selected;
        _tgtThumb.IsVisible = selected;
    }

    public void UpdateGeometry(IDiagramModel model, Size diagramSize)
    {
        var (a, b) = _router.GetEndpoints(_edge, model);
        const double pad = 18;
        var minX = Math.Min(a.X, b.X) - pad;
        var minY = Math.Min(a.Y, b.Y) - pad;
        var maxX = Math.Max(a.X, b.X) + pad;
        var maxY = Math.Max(a.Y, b.Y) + pad;
        Canvas.SetLeft(this, minX);
        Canvas.SetTop(this, minY);
        Width = Math.Max(1, maxX - minX);
        Height = Math.Max(1, maxY - minY);
        var la = new Point(a.X - minX, a.Y - minY);
        var lb = new Point(b.X - minX, b.Y - minY);
        _hitLine.StartPoint = la;
        _hitLine.EndPoint = lb;
        _visLine.StartPoint = la;
        _visLine.EndPoint = lb;
        Canvas.SetLeft(_srcThumb, la.X - 6);
        Canvas.SetTop(_srcThumb, la.Y - 6);
        Canvas.SetLeft(_tgtThumb, lb.X - 6);
        Canvas.SetTop(_tgtThumb, lb.Y - 6);
    }

    private void OnEdgePressed(object? sender, PointerPressedEventArgs e)
    {
        _host.SelectEdge(_edge.Id);
        e.Handled = true;
    }

    private void OnThumbPressed(PointerPressedEventArgs e, bool sourceEnd)
    {
        _host.SelectEdge(_edge.Id);
        _host.BeginReconnectEdge(_edge, sourceEnd, e);
        e.Handled = true;
    }

    private static Ellipse CreateThumb()
    {
        var e = new Ellipse
        {
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush(Color.Parse("#1ed760")),
            Stroke = Brushes.White,
            StrokeThickness = 1,
            IsVisible = false,
            IsHitTestVisible = true,
            ZIndex = 2,
        };
        return e;
    }
}
