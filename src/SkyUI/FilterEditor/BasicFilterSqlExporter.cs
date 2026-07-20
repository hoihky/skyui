using System.Text;

namespace SkyUI.FilterEditor;

/// <summary>ANSI-ish SQL using double-quoted identifiers and <c>||</c> for LIKE patterns (replace with a dialect-specific exporter as needed).</summary>
public sealed class BasicFilterSqlExporter : IFilterSqlExporter
{
    public string ToSql(FilterDocument document) => document.Root.Accept(new Visitor());

    private sealed class Visitor : IFilterNodeVisitor<string>
    {
        public string VisitGroup(FilterGroupNode node)
        {
            if (node.Children.Count == 0)
                return "(1 = 1)";

            var sep = node.LogicalKind == FilterLogicalKind.And ? " AND " : " OR ";
            var sb = new StringBuilder();
            sb.Append('(');
            var first = true;
            foreach (var child in node.Children)
            {
                var part = child.Accept(this);
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                if (!first)
                    sb.Append(sep);
                first = false;
                sb.Append(part);
            }

            if (first)
                return "(1 = 1)";

            sb.Append(')');
            return sb.ToString();
        }

        public string VisitCondition(FilterConditionNode node)
        {
            var col = QuoteIdentifier(node.FieldPath);
            return node.Operator switch
            {
                FilterCompareOperator.Equal => $"{col} = {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.NotEqual => $"{col} <> {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.LessThan => $"{col} < {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.LessOrEqual => $"{col} <= {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.GreaterThan => $"{col} > {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.GreaterOrEqual => $"{col} >= {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.Contains => $"{col} LIKE '%' || {SqlLiteral(node.ValueText)} || '%'",
                FilterCompareOperator.StartsWith => $"{col} LIKE {SqlLiteral(node.ValueText)} || '%'",
                FilterCompareOperator.EndsWith => $"{col} LIKE '%' || {SqlLiteral(node.ValueText)}",
                FilterCompareOperator.IsNull => $"{col} IS NULL",
                FilterCompareOperator.IsNotNull => $"{col} IS NOT NULL",
                _ => $"{col} = {SqlLiteral(node.ValueText)}",
            };
        }

        private static string QuoteIdentifier(string path)
        {
            var escaped = path.Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"\"{escaped}\"";
        }

        private static string SqlLiteral(string? value)
        {
            if (value is null)
                return "NULL";
            var escaped = value.Replace("'", "''", StringComparison.Ordinal);
            return $"'{escaped}'";
        }
    }
}
