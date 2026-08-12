using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed horizontal menu bar. Apply to top-level <see cref="Menu"/>.</summary>
public class SkyMenuBar : Menu
{
    public SkyMenuBar() => Classes.Add("sky");
}
