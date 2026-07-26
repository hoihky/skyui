namespace SkyUI.FilterEditor;

/// <summary>Strategy for turning a filter tree into SQL (DIP: swap dialect / vendor without changing the editor).</summary>
public interface IFilterSqlExporter
{
    string ToSql(FilterDocument document);
}
