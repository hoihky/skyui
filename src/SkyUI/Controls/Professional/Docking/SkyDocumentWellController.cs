using System.Collections;

namespace SkyUI.Controls.Professional;

/// <summary>
/// Document-well domain logic (selection, close, reorder) decoupled from the visual tree for testability.
/// </summary>
public sealed class SkyDocumentWellController
{
    private IEnumerable? itemsSource;
    private SkyDocumentShellPlacement placementFilter = SkyDocumentShellPlacement.Central;

    public IEnumerable? ItemsSource
    {
        get => itemsSource;
        set => itemsSource = value;
    }

    public SkyDocumentShellPlacement PlacementFilter
    {
        get => placementFilter;
        set => placementFilter = value;
    }

    public IReadOnlyList<ISkyDocument> Documents =>
        SkyDocumentShellAccessor.GetByPlacement(itemsSource, placementFilter);

    public ISkyDocument? GetDocumentAt(int index)
    {
        var docs = Documents;
        return index >= 0 && index < docs.Count ? docs[index] : null;
    }

    public int IndexOf(ISkyDocument document) =>
        SkyDocumentShellAccessor.IndexOf(itemsSource, document, placementFilter);

    public bool Contains(ISkyDocument document) => IndexOf(document) >= 0;

    public bool TryAdd(ISkyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var list = SkyDocumentWellDocumentAccessor.AsMutableList(itemsSource);
        if (list is null)
            return false;
        if (SkyDocumentWellDocumentAccessor.ContainsDocument(itemsSource, document))
            return false;
        list.Add(document);
        return true;
    }

    public bool TryRemove(ISkyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var list = SkyDocumentWellDocumentAccessor.AsMutableList(itemsSource);
        if (list is null)
            return false;

        for (var i = 0; i < list.Count; i++)
        {
            if (SkyDocumentWellDocumentAccessor.TryAsDocument(list[i], out var candidate)
                && ReferenceEquals(candidate, document))
            {
                list.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool TryMove(int oldIndex, int newIndex)
    {
        var list = SkyDocumentWellDocumentAccessor.AsMutableList(itemsSource);
        if (list is null)
            return false;

        var documents = Documents;
        if (oldIndex < 0 || oldIndex >= documents.Count || newIndex < 0 || newIndex >= documents.Count)
            return false;
        if (oldIndex == newIndex)
            return true;

        var fromListIndex = SkyDocumentShellAccessor.MapDocumentIndexToListIndex(list, oldIndex, placementFilter);
        var item = list[fromListIndex];
        if (!SkyDocumentWellDocumentAccessor.TryAsDocument(item, out _))
            return false;

        list.RemoveAt(fromListIndex);
        var toListIndex = SkyDocumentShellAccessor.MapDocumentIndexToListIndex(list, newIndex, placementFilter);
        list.Insert(toListIndex, item);
        return true;
    }

    public int ClampSelectionIndex(int selectedIndex, ISkyDocument? selectedDocument)
    {
        var count = Documents.Count;
        if (count == 0)
            return -1;

        if (selectedDocument is not null)
        {
            var index = IndexOf(selectedDocument);
            if (index >= 0)
                return index;
        }

        if (selectedIndex < 0)
            return 0;
        if (selectedIndex >= count)
            return count - 1;
        return selectedIndex;
    }

    public ISkyDocument? ResolveSelection(int selectedIndex, ISkyDocument? selectedDocument)
    {
        var index = ClampSelectionIndex(selectedIndex, selectedDocument);
        return index < 0 ? null : GetDocumentAt(index);
    }

}
