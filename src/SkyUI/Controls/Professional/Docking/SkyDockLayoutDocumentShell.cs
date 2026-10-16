using System.Collections;

namespace SkyUI.Controls.Professional;

internal static class SkyDockLayoutDocumentShell
{
    public static IReadOnlyList<SkyDocumentLayoutEntry> Capture(IEnumerable? documents)
    {
        if (documents is null)
            return Array.Empty<SkyDocumentLayoutEntry>();

        var list = new List<SkyDocumentLayoutEntry>();
        foreach (var item in documents)
        {
            if (!SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                continue;
            list.Add(new SkyDocumentLayoutEntry
            {
                Id = document.Id,
                ShellPlacement = document.ShellPlacement,
                ShellDockBack = document.ShellDockBack,
                ShellFloatLeft = document.ShellFloatLeft,
                ShellFloatTop = document.ShellFloatTop,
            });
        }

        return list;
    }

    public static void Apply(IReadOnlyList<SkyDocumentLayoutEntry>? entries, IEnumerable? documents)
    {
        if (entries is null || entries.Count == 0 || documents is null)
            return;

        var map = entries.ToDictionary(e => e.Id);
        foreach (var item in documents)
        {
            if (!SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                continue;
            if (!map.TryGetValue(document.Id, out var entry))
                continue;
            document.ShellPlacement = entry.ShellPlacement;
            document.ShellDockBack = entry.ShellDockBack;
            document.ShellFloatLeft = entry.ShellFloatLeft;
            document.ShellFloatTop = entry.ShellFloatTop;
        }
    }
}
