using System.Collections;

namespace SkyUI.Controls.Professional;

internal static class SkyDocumentShellAccessor
{
    public static SkyDocumentShellPlacement ToShellPlacement(SkyDockZone zone) =>
        zone switch
        {
            SkyDockZone.Left => SkyDocumentShellPlacement.Left,
            SkyDockZone.Right => SkyDocumentShellPlacement.Right,
            SkyDockZone.Bottom => SkyDocumentShellPlacement.Bottom,
            SkyDockZone.Float => SkyDocumentShellPlacement.Float,
            _ => SkyDocumentShellPlacement.Central,
        };

    public static SkyDockZone ToDockZone(SkyDocumentShellPlacement placement) =>
        placement switch
        {
            SkyDocumentShellPlacement.Left => SkyDockZone.Left,
            SkyDocumentShellPlacement.Right => SkyDockZone.Right,
            SkyDocumentShellPlacement.Bottom => SkyDockZone.Bottom,
            SkyDocumentShellPlacement.Float => SkyDockZone.Float,
            _ => SkyDockZone.Document,
        };

    public static IReadOnlyList<ISkyDocument> GetByPlacement(IEnumerable? source, SkyDocumentShellPlacement placement)
    {
        if (source is null)
            return Array.Empty<ISkyDocument>();

        var list = new List<ISkyDocument>();
        foreach (var item in source)
        {
            if (SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document)
                && document.ShellPlacement == placement)
                list.Add(document);
        }

        return list;
    }

    public static int IndexOf(IEnumerable? source, ISkyDocument document, SkyDocumentShellPlacement placement)
    {
        if (source is null)
            return -1;

        var index = 0;
        foreach (var item in source)
        {
            if (!SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var candidate))
                continue;
            if (candidate.ShellPlacement != placement)
                continue;
            if (ReferenceEquals(candidate, document))
                return index;
            index++;
        }

        return -1;
    }

    public static int MapDocumentIndexToListIndex(IList list, int documentIndex, SkyDocumentShellPlacement placement)
    {
        var seen = 0;
        for (var i = 0; i < list.Count; i++)
        {
            if (!SkyDocumentWellDocumentAccessor.TryAsDocument(list[i], out var candidate))
                continue;
            if (candidate.ShellPlacement != placement)
                continue;
            if (seen == documentIndex)
                return i;
            seen++;
        }

        return list.Count;
    }
}
