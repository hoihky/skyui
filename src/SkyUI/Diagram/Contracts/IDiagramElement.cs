namespace SkyUI.Diagram.Contracts;

/// <summary>
/// Any addressable item on a diagram surface (ISP: minimal identity contract).
/// </summary>
public interface IDiagramElement
{
    string Id { get; }
}
