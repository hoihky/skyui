using Avalonia.Media;

namespace SkyUI.Demo.ViewModels;

public sealed class SpritePreviewLayerViewModel
{
    public SpritePreviewLayerViewModel(string layerName, string celName, IImage? celImage, IBrush swatchFallback)
    {
        LayerName = layerName;
        CelName = celName;
        CelImage = celImage;
        SwatchFallback = swatchFallback;
    }

    public string LayerName { get; }

    public string CelName { get; }

    public IImage? CelImage { get; }

    public IBrush SwatchFallback { get; }
}
