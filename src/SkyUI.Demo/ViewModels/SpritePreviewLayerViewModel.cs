using Avalonia.Media;

namespace SkyUI.Demo.ViewModels;

public sealed class SpritePreviewLayerViewModel
{
    public SpritePreviewLayerViewModel(string layerName, string celName, IBrush swatch)
    {
        LayerName = layerName;
        CelName = celName;
        Swatch = swatch;
    }

    public string LayerName { get; }

    public string CelName { get; }

    public IBrush Swatch { get; }
}
