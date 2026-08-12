using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed flyout menu (e.g. attached to a toolbar button).</summary>
public class SkyMenuFlyout : MenuFlyout
{
    public SkyMenuFlyout() => FlyoutPresenterClasses.Add("sky");
}
