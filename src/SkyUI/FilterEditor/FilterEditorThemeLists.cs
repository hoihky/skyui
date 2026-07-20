namespace SkyUI.FilterEditor;

/// <summary>Combo box item sources for the default <see cref="FilterEditor"/> theme (avoids fragile XAML array markup).</summary>
public static class FilterEditorThemeLists
{
    public static IReadOnlyList<FilterLogicalKind> LogicalKinds { get; } =
        new[] { FilterLogicalKind.And, FilterLogicalKind.Or };

    public static IReadOnlyList<FilterCompareOperator> CompareOperators { get; } =
        Enum.GetValues<FilterCompareOperator>();
}
