using Avalonia;

namespace SkyUI.Controls.Professional;

/// <summary>Applies document shell placement during dock drag-drop.</summary>
public sealed class SkyDocumentShellController
{
    public void ApplyPlacement(ISkyDocument document, SkyDocumentShellPlacement placement, Point? floatPosition = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (placement == SkyDocumentShellPlacement.Float)
        {
            if (document.ShellPlacement != SkyDocumentShellPlacement.Float)
                document.ShellDockBack = document.ShellPlacement;
            document.ShellPlacement = SkyDocumentShellPlacement.Float;
            if (floatPosition is not null)
            {
                document.ShellFloatLeft = floatPosition.Value.X;
                document.ShellFloatTop = floatPosition.Value.Y;
            }

            return;
        }

        document.ShellPlacement = placement;
        document.ShellDockBack = placement;
    }

    public void DockBack(ISkyDocument document) =>
        ApplyPlacement(document, document.ShellDockBack);

    public void ApplyDockZone(ISkyDocument document, SkyDockZone zone, Point dropPointInManager)
    {
        ArgumentNullException.ThrowIfNull(document);
        var placement = SkyDocumentShellAccessor.ToShellPlacement(zone);
        if (placement == SkyDocumentShellPlacement.Float)
            ApplyPlacement(document, placement, dropPointInManager);
        else
            ApplyPlacement(document, placement);
    }
}
