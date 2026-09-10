using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SkyUI.Controls;

internal static class SkyTopLevelResolver
{
    public static TopLevel? Resolve(Visual? owner)
    {
        if (owner is not null)
        {
            var topLevel = TopLevel.GetTopLevel(owner);
            if (topLevel is not null)
                return topLevel;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            return TopLevel.GetTopLevel(singleView.MainView);

        return null;
    }
}
