using Avalonia.Controls;
using SkyUI.Diagram.Model;

namespace SkyUI.Diagram.Presentation;

public sealed class DefaultDiagramNodePresenterFactory : IDiagramNodePresenterFactory
{
    public Control CreatePresenter(DiagramSurface host, DiagramNode node) => new DiagramNodePresenter(host, node);
}
