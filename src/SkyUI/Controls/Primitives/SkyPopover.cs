using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed flyout for lightweight popover content (non-menu panels).</summary>
public class SkyPopover : Flyout
{
    public SkyPopover() => FlyoutPresenterClasses.Add("sky");
}
