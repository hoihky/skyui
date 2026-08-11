using Avalonia.Controls;
using Avalonia.Layout;

namespace SkyUI.Controls;

/// <summary>Sky-themed <see cref="TabControl"/> (pill tab strip).</summary>
public class SkyTabView : TabControl
{
    static SkyTabView()
    {
        TabStripPlacementProperty.OverrideDefaultValue<SkyTabView>(Dock.Top);
    }
}
