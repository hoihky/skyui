using Avalonia.Media.Fonts;

namespace SkyUI.Fonts;

internal sealed class SkyNotoFontCollection : EmbeddedFontCollection
{
    public SkyNotoFontCollection()
        : base(
            new Uri("fonts:SkyNoto", UriKind.Absolute),
            new Uri("avares://SkyUI.Fonts/Assets/Fonts", UriKind.Absolute))
    {
    }
}
