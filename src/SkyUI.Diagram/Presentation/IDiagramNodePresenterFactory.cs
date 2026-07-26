using Avalonia.Controls;
using SkyUI.Diagram.Model;

namespace SkyUI.Diagram.Presentation;

/// <summary>
/// Creates visual presenters for nodes (OCP: plug in factories per app for custom node types).
/// </summary>
public interface IDiagramNodePresenterFactory
{
    Control CreatePresenter(DiagramSurface host, DiagramNode node);
}
