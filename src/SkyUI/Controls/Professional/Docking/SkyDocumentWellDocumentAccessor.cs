using System.Collections;

namespace SkyUI.Controls.Professional;

/// <summary>Resolves <see cref="ISkyDocument"/> instances from an <see cref="IEnumerable"/> items source.</summary>
internal static class SkyDocumentWellDocumentAccessor
{
    public static IReadOnlyList<ISkyDocument> Snapshot(IEnumerable? source)
    {
        if (source is null)
            return Array.Empty<ISkyDocument>();

        var list = new List<ISkyDocument>();
        foreach (var item in source)
        {
            if (TryAsDocument(item, out var document))
                list.Add(document);
        }

        return list;
    }

    public static bool TryAsDocument(object? item, out ISkyDocument document)
    {
        if (item is ISkyDocument skyDocument)
        {
            document = skyDocument;
            return true;
        }

        document = null!;
        return false;
    }

    public static IList? AsMutableList(IEnumerable? source)
    {
        if (source is not IList list || list.IsReadOnly || list.IsFixedSize)
            return null;
        return list;
    }

    public static bool ContainsDocument(IEnumerable? source, ISkyDocument document)
    {
        if (source is null)
            return false;
        foreach (var item in source)
        {
            if (TryAsDocument(item, out var candidate) && ReferenceEquals(candidate, document))
                return true;
        }

        return false;
    }

    public static int IndexOf(IEnumerable? source, ISkyDocument document)
    {
        if (source is null)
            return -1;

        var index = 0;
        foreach (var item in source)
        {
            if (!TryAsDocument(item, out var candidate))
                continue;
            if (ReferenceEquals(candidate, document))
                return index;
            index++;
        }

        return -1;
    }
}
