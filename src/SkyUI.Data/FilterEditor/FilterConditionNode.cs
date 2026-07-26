using System.Collections.ObjectModel;

namespace SkyUI.FilterEditor;

/// <summary>Leaf node: field + operator + optional literal.</summary>
public sealed class FilterConditionNode : FilterNodeBase
{
    private readonly ObservableCollection<FilterNodeBase> _emptyChildren = new();
    private string _fieldPath;
    private FilterCompareOperator _operator;
    private string? _valueText;

    public FilterConditionNode(string fieldPath, FilterCompareOperator op, string? valueText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldPath);
        _fieldPath = fieldPath;
        _operator = op;
        _valueText = valueText;
    }

    public override ObservableCollection<FilterNodeBase> Children => _emptyChildren;

    public string FieldPath
    {
        get => _fieldPath;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (_fieldPath == value)
                return;
            _fieldPath = value;
            OnPropertyChanged();
        }
    }

    public FilterCompareOperator Operator
    {
        get => _operator;
        set
        {
            if (_operator == value)
                return;
            _operator = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Serialized / display literal (null for <see cref="FilterCompareOperator.IsNull"/> / <see cref="FilterCompareOperator.IsNotNull"/>).</summary>
    public string? ValueText
    {
        get => _valueText;
        set
        {
            if (_valueText == value)
                return;
            _valueText = value;
            OnPropertyChanged();
        }
    }

    public override T Accept<T>(IFilterNodeVisitor<T> visitor) => visitor.VisitCondition(this);
}
