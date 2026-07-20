namespace SkyUI.FilterEditor;

/// <summary>Comparison between a bound field and a literal / null.</summary>
public enum FilterCompareOperator
{
    Equal,
    NotEqual,
    LessThan,
    LessOrEqual,
    GreaterThan,
    GreaterOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
}
