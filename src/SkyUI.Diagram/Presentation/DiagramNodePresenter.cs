using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Diagram.Contracts;
using SkyUI.Diagram.Model;

namespace SkyUI.Diagram.Presentation;

/// <summary>
/// Default node chrome: drag header, optional image, label editor, ports, resize grips when selected.
/// Ports and grips are grid siblings above the body so hit-testing works (overlay canvas was non-hittable).
/// </summary>
public sealed class DiagramNodePresenter : Grid
{
    private readonly DiagramSurface _host;
    private readonly DiagramNode _model;
    private readonly Border _body;
    private readonly Border _dragStrip;
    private readonly TextBox _label;
    private readonly global::Avalonia.Controls.Image _image;
    private readonly Ellipse[] _portVisuals;
    private readonly Border[] _resizeVisuals = new Border[8];
    private bool _selected;

    public DiagramNodePresenter(DiagramSurface host, DiagramNode model)
    {
        _host = host;
        _model = model;
        MinWidth = 120;
        MinHeight = 88;

        var stack = new StackPanel { Spacing = 0 };

        _dragStrip = new Border
        {
            Height = 22,
            Background = new SolidColorBrush(Color.Parse("#333")),
            Cursor = new Cursor(StandardCursorType.SizeAll),
            Child = new TextBlock
            {
                Text = "⋮⋮",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray,
                FontSize = 11,
            },
        };
        _dragStrip.PointerPressed += OnDragStripPressed;
        stack.Children.Add(_dragStrip);

        _image = new global::Avalonia.Controls.Image
        {
            MaxHeight = 56,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(8, 4, 8, 0),
            IsVisible = false,
        };
        stack.Children.Add(_image);

        _label = new TextBox
        {
            Margin = new Thickness(8, 6, 8, 8),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 40,
            Foreground = Brushes.WhiteSmoke,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CaretBrush = Brushes.White,
        };
        _label.Text = _model.Label;
        _label.LostFocus += (_, _) => _model.Label = _label.Text ?? string.Empty;
        stack.Children.Add(_label);

        _body = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#242424")),
            BorderBrush = new SolidColorBrush(Color.Parse("#444")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = stack,
            ZIndex = 0,
        };

        RowDefinitions = new RowDefinitions { new RowDefinition(GridLength.Star) };
        ColumnDefinitions = new ColumnDefinitions { new ColumnDefinition(GridLength.Star) };
        Children.Add(_body);
        Grid.SetRow(_body, 0);
        Grid.SetColumn(_body, 0);

        _portVisuals = new Ellipse[_model.Ports.Count];
        for (var i = 0; i < _model.Ports.Count; i++)
        {
            var port = (DiagramPort)_model.Ports[i];
            var el = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = new SolidColorBrush(Color.Parse("#1db954")),
                Stroke = Brushes.White,
                StrokeThickness = 1,
                ZIndex = 5,
                IsHitTestVisible = true,
            };
            ApplyPortChrome(port, el);
            var pid = port.Id;
            el.PointerPressed += (_, e) => OnPortPressed(pid, e);
            _portVisuals[i] = el;
            Children.Add(el);
            Grid.SetRow(el, 0);
            Grid.SetColumn(el, 0);
        }

        var handles = new[]
        {
            ResizeHandle.TopLeft, ResizeHandle.Top, ResizeHandle.TopRight, ResizeHandle.Right,
            ResizeHandle.BottomRight, ResizeHandle.Bottom, ResizeHandle.BottomLeft, ResizeHandle.Left,
        };
        for (var i = 0; i < handles.Length; i++)
        {
            var h = handles[i];
            var b = new Border
            {
                Width = 8,
                Height = 8,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#1ed760")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(1),
                Tag = h,
                IsVisible = false,
                ZIndex = 20,
                IsHitTestVisible = true,
                Cursor = CursorFor(h),
            };
            ApplyResizeChrome(h, b);
            b.PointerPressed += (_, e) => OnResizePressed(h, e);
            _resizeVisuals[i] = b;
            Children.Add(b);
            Grid.SetRow(b, 0);
            Grid.SetColumn(b, 0);
        }

        _model.PropertyChanged += OnModelPropertyChanged;
    }

    public DiagramNode Model => _model;

    public void SetSelected(bool selected)
    {
        _selected = selected;
        _body.BorderBrush = selected
            ? new SolidColorBrush(Color.Parse("#1ed760"))
            : new SolidColorBrush(Color.Parse("#444"));
        _body.BorderThickness = new Thickness(selected ? 2 : 1);
        foreach (var r in _resizeVisuals)
            r.IsVisible = selected;
    }

    private static void ApplyPortChrome(DiagramPort port, Ellipse el)
    {
        switch (port.Side)
        {
            case PortSide.Left:
                el.HorizontalAlignment = HorizontalAlignment.Left;
                el.VerticalAlignment = VerticalAlignment.Center;
                el.Margin = new Thickness(-7, 0, 0, 0);
                break;
            case PortSide.Right:
                el.HorizontalAlignment = HorizontalAlignment.Right;
                el.VerticalAlignment = VerticalAlignment.Center;
                el.Margin = new Thickness(0, 0, -7, 0);
                break;
            case PortSide.Top:
                el.HorizontalAlignment = HorizontalAlignment.Center;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.Margin = new Thickness(0, -7, 0, 0);
                break;
            case PortSide.Bottom:
                el.HorizontalAlignment = HorizontalAlignment.Center;
                el.VerticalAlignment = VerticalAlignment.Bottom;
                el.Margin = new Thickness(0, 0, 0, -7);
                break;
        }
    }

    private static void ApplyResizeChrome(ResizeHandle handle, Border b)
    {
        switch (handle)
        {
            case ResizeHandle.TopLeft:
                b.HorizontalAlignment = HorizontalAlignment.Left;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.Margin = new Thickness(-4, -4, 0, 0);
                break;
            case ResizeHandle.Top:
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.Margin = new Thickness(0, -4, 0, 0);
                break;
            case ResizeHandle.TopRight:
                b.HorizontalAlignment = HorizontalAlignment.Right;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.Margin = new Thickness(0, -4, -4, 0);
                break;
            case ResizeHandle.Right:
                b.HorizontalAlignment = HorizontalAlignment.Right;
                b.VerticalAlignment = VerticalAlignment.Center;
                b.Margin = new Thickness(0, 0, -4, 0);
                break;
            case ResizeHandle.BottomRight:
                b.HorizontalAlignment = HorizontalAlignment.Right;
                b.VerticalAlignment = VerticalAlignment.Bottom;
                b.Margin = new Thickness(0, 0, -4, -4);
                break;
            case ResizeHandle.Bottom:
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.VerticalAlignment = VerticalAlignment.Bottom;
                b.Margin = new Thickness(0, 0, 0, -4);
                break;
            case ResizeHandle.BottomLeft:
                b.HorizontalAlignment = HorizontalAlignment.Left;
                b.VerticalAlignment = VerticalAlignment.Bottom;
                b.Margin = new Thickness(-4, 0, 0, -4);
                break;
            case ResizeHandle.Left:
                b.HorizontalAlignment = HorizontalAlignment.Left;
                b.VerticalAlignment = VerticalAlignment.Center;
                b.Margin = new Thickness(-4, 0, 0, 0);
                break;
        }
    }

    private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DiagramNode.Label) && !_label.IsFocused)
            _label.Text = _model.Label;
        if (e.PropertyName == nameof(DiagramNode.Image))
            ApplyImage();
        if (e.PropertyName == nameof(DiagramNode.Bounds))
            _host.SyncNodeCanvasPosition(this);
    }

    private void ApplyImage()
    {
        _image.Source = _model.Image;
        _image.IsVisible = _model.Image != null;
    }

    private static Cursor CursorFor(ResizeHandle h) => h switch
    {
        ResizeHandle.TopLeft or ResizeHandle.BottomRight => new Cursor(StandardCursorType.TopLeftCorner),
        ResizeHandle.TopRight or ResizeHandle.BottomLeft => new Cursor(StandardCursorType.TopRightCorner),
        ResizeHandle.Top or ResizeHandle.Bottom => new Cursor(StandardCursorType.SizeNorthSouth),
        ResizeHandle.Left or ResizeHandle.Right => new Cursor(StandardCursorType.SizeWestEast),
        _ => Cursor.Default,
    };

    private void OnDragStripPressed(object? sender, PointerPressedEventArgs e)
    {
        var additive = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        _host.SelectNode(_model.Id, additive);
        _host.BeginMoveNode(_model, e);
        e.Handled = true;
    }

    private void OnPortPressed(string portId, PointerPressedEventArgs e)
    {
        _host.SelectNode(_model.Id, false);
        var model = _host.Model;
        if (model != null)
        {
            DiagramEdge? sole = null;
            foreach (var edge in model.Edges)
            {
                var touches =
                    (edge.SourceNodeId == _model.Id && edge.SourcePortId == portId) ||
                    (edge.TargetNodeId == _model.Id && edge.TargetPortId == portId);
                if (!touches)
                    continue;
                if (sole != null)
                {
                    sole = null;
                    break;
                }

                sole = edge;
            }

            if (sole != null)
            {
                var sourceEnd = sole.SourceNodeId == _model.Id && sole.SourcePortId == portId;
                _host.SelectEdge(sole.Id);
                _host.BeginReconnectEdge(sole, sourceEnd, e);
                e.Handled = true;
                return;
            }
        }

        _host.BeginNewConnection(_model.Id, portId, e);
        e.Handled = true;
    }

    private void OnResizePressed(ResizeHandle handle, PointerPressedEventArgs e)
    {
        _host.SelectNode(_model.Id, false);
        _host.BeginResizeNode(_model, handle, e);
        e.Handled = true;
    }
}
