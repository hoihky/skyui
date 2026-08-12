using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed single expand/collapse panel.</summary>
public class SkyExpander : Expander
{
    public SkyExpander()
    {
        Classes.Add("sky");
        Classes.Add("sky-expander");
    }
}
