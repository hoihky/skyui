using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkyUI.Core.Theming;

internal static class SkyDensityApplicator
{
    private static readonly string[] OverrideKeys =
    [
        SkyDensityKeys.ButtonPadding,
        SkyDensityKeys.ButtonMinHeight,
        SkyDensityKeys.NavPillPadding,
        SkyDensityKeys.NavPillMinHeight,
        SkyDensityKeys.FieldPadding,
        SkyDensityKeys.FieldMinHeight,
        SkyDensityKeys.ComboPadding,
        SkyDensityKeys.ComboMinHeight,
        SkyDensityKeys.ListItemPadding,
        SkyDensityKeys.ListItemMinHeight,
        SkyDensityKeys.NavItemPadding,
        SkyDensityKeys.ListBoxPadding,
        SkyDensityKeys.PickerPadding,
        SkyDensityKeys.PickerMinHeight,
        SkyDensityKeys.ToggleMinHeight,
        SkyDensityKeys.PlaySize,
        SkyDensityKeys.PlayPadding,
    ];

    public static void Apply(IResourceHost host, SkyDensity density)
    {
        if (host is not Application { Resources: { } resources })
            return;

        if (density == SkyDensity.Comfortable)
        {
            ClearOverrides(resources);
            return;
        }

        resources[SkyDensityKeys.ButtonPadding] = new Thickness(12, 6);
        resources[SkyDensityKeys.ButtonMinHeight] = 32.0;
        resources[SkyDensityKeys.NavPillPadding] = new Thickness(24, 8);
        resources[SkyDensityKeys.NavPillMinHeight] = 40.0;
        resources[SkyDensityKeys.FieldPadding] = new Thickness(12, 8);
        resources[SkyDensityKeys.FieldMinHeight] = 36.0;
        resources[SkyDensityKeys.ComboPadding] = new Thickness(12, 8);
        resources[SkyDensityKeys.ComboMinHeight] = 36.0;
        resources[SkyDensityKeys.ListItemPadding] = new Thickness(10, 8);
        resources[SkyDensityKeys.ListItemMinHeight] = 32.0;
        resources[SkyDensityKeys.NavItemPadding] = new Thickness(10, 8);
        resources[SkyDensityKeys.ListBoxPadding] = new Thickness(2);
        resources[SkyDensityKeys.PickerPadding] = new Thickness(12, 8, 0, 8);
        resources[SkyDensityKeys.PickerMinHeight] = 36.0;
        resources[SkyDensityKeys.ToggleMinHeight] = 32.0;
        resources[SkyDensityKeys.PlaySize] = 48.0;
        resources[SkyDensityKeys.PlayPadding] = new Thickness(10);
    }

    private static void ClearOverrides(IResourceDictionary resources)
    {
        foreach (var key in OverrideKeys)
            resources.Remove(key);
    }
}
