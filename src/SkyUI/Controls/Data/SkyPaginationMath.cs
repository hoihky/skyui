using System.Collections;

namespace SkyUI.Controls;

/// <summary>Pure paging calculations shared by <see cref="SkyPagination"/> and <see cref="SkyDataPager"/>.</summary>
public static class SkyPaginationMath
{
    public static int ComputePageCount(int totalCount, int pageSize)
    {
        if (pageSize <= 0)
            return 0;

        if (totalCount <= 0)
            return totalCount == 0 ? 0 : 1;

        return (totalCount + pageSize - 1) / pageSize;
    }

    public static int ClampPage(int page, int pageCount)
    {
        if (pageCount <= 0)
            return 1;

        if (page < 1)
            return 1;

        return page > pageCount ? pageCount : page;
    }

    public static (int Start, int End) GetInclusiveRange(int currentPage, int pageSize, int totalCount)
    {
        if (totalCount <= 0)
            return (0, 0);

        var start = (currentPage - 1) * pageSize + 1;
        var end = Math.Min(currentPage * pageSize, totalCount);
        return (start, end);
    }

    public static int CountItems(IEnumerable? source)
    {
        if (source is null)
            return 0;

        if (source is ICollection collection)
            return collection.Count;

        var count = 0;
        foreach (var _ in source)
            count++;

        return count;
    }

    public static IEnumerable Slice(IEnumerable? source, int currentPage, int pageSize)
    {
        if (source is null || pageSize <= 0)
            return Array.Empty<object>();

        if (source is IList list)
            return SliceList(list, currentPage, pageSize);

        return SliceEnumerable(source, currentPage, pageSize).ToArray();
    }

    private static IEnumerable SliceList(IList list, int currentPage, int pageSize)
    {
        var skip = (currentPage - 1) * pageSize;
        if (skip >= list.Count)
            return Array.Empty<object>();

        var take = Math.Min(pageSize, list.Count - skip);
        var slice = new object?[take];
        for (var index = 0; index < take; index++)
            slice[index] = list[skip + index];

        return slice;
    }

    private static IEnumerable<object?> SliceEnumerable(IEnumerable source, int currentPage, int pageSize)
    {
        var skip = (currentPage - 1) * pageSize;
        var skipped = 0;
        var taken = 0;

        foreach (var item in source)
        {
            if (skipped < skip)
            {
                skipped++;
                continue;
            }

            if (taken >= pageSize)
                break;

            yield return item;
            taken++;
        }
    }
}
