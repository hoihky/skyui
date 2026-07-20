using Avalonia;
using Avalonia.Media;

namespace SkyUI.Diagram.Contracts;

/// <summary>
/// A node on the diagram: positioned box with typed content and connection ports (SRP: node vs edge).
/// </summary>
public interface IDiagramNode : IDiagramElement
{
    string NodeTypeKey { get; }

    string Label { get; set; }

    IImage? Image { get; set; }

    Rect Bounds { get; set; }

    IReadOnlyList<IDiagramPort> Ports { get; }
}

