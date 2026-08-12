using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed tooltip surface.</summary>
public class SkyTooltip : ToolTip
{
    public SkyTooltip()
    {
        Classes.Add("sky");
    }
}
