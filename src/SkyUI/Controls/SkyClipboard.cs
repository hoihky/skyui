using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace SkyUI.Controls;

/// <summary>Helper for copy/paste of plain text and tabular (TSV) clipboard data.</summary>
public static class SkyClipboard
{
    public static async Task SetTextAsync(string text, Visual? owner = null)
    {
        var clipboard = ResolveClipboard(owner);
        if (clipboard is null)
            return;

        await clipboard.SetTextAsync(text).ConfigureAwait(false);
    }

    public static async Task<string?> GetTextAsync(Visual? owner = null)
    {
        var clipboard = ResolveClipboard(owner);
        if (clipboard is null)
            return null;

        return await clipboard.TryGetTextAsync().ConfigureAwait(false);
    }

    public static Task SetTabularAsync(IReadOnlyList<IReadOnlyList<string?>> rows, Visual? owner = null) =>
        SetTextAsync(SkyTabularFormat.ToTsv(rows), owner);

    public static async Task<IReadOnlyList<string[]>?> TryGetTabularAsync(Visual? owner = null)
    {
        var text = await GetTextAsync(owner).ConfigureAwait(false);
        return SkyTabularFormat.TryParseTsv(text);
    }

    public static async Task<bool> ContainsTextAsync(Visual? owner = null)
    {
        var clipboard = ResolveClipboard(owner);
        if (clipboard is null)
            return false;

        return await clipboard.TryGetTextAsync().ConfigureAwait(false) is not null;
    }

    private static IClipboard? ResolveClipboard(Visual? owner) =>
        SkyTopLevelResolver.Resolve(owner)?.Clipboard;
}
