using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using SkyUI.Diagram.Contracts;

namespace SkyUI.Diagram.Model;

public sealed class DiagramNode : IDiagramNode, INotifyPropertyChanged
{
    private string _label;
    private IImage? _image;
    private Rect _bounds;
    private readonly List<IDiagramPort> _ports;

    public DiagramNode(string id, string nodeTypeKey, string label, Rect initialBounds)
    {
        Id = id;
        NodeTypeKey = nodeTypeKey;
        _label = label;
        _bounds = initialBounds;
        _ports =
        [
            new DiagramPort("L", id, PortSide.Left),
            new DiagramPort("T", id, PortSide.Top),
            new DiagramPort("R", id, PortSide.Right),
            new DiagramPort("B", id, PortSide.Bottom),
        ];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }

    public string NodeTypeKey { get; }

    public string Label
    {
        get => _label;
        set
        {
            if (_label == value)
                return;
            _label = value;
            OnPropertyChanged();
        }
    }

    public IImage? Image
    {
        get => _image;
        set
        {
            if (ReferenceEquals(_image, value))
                return;
            _image = value;
            OnPropertyChanged();
        }
    }

    public Rect Bounds
    {
        get => _bounds;
        set
        {
            if (_bounds == value)
                return;
            _bounds = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<IDiagramPort> Ports => _ports;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
