using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SkyUI.FilterEditor;

/// <summary>Logical operator node linking child conditions or nested groups.</summary>
public sealed class FilterGroupNode : FilterNodeBase
{
    private readonly ObservableCollection<FilterNodeBase> _children = new();
    private FilterLogicalKind _logicalKind = FilterLogicalKind.And;

    public FilterGroupNode()
    {
        _children.CollectionChanged += OnChildrenChanged;
    }

    public override ObservableCollection<FilterNodeBase> Children => _children;

    public FilterLogicalKind LogicalKind
    {
        get => _logicalKind;
        set
        {
            if (_logicalKind == value)
                return;
            _logicalKind = value;
            OnPropertyChanged();
        }
    }

    public override T Accept<T>(IFilterNodeVisitor<T> visitor) => visitor.VisitGroup(this);

    public FilterConditionNode AddCondition(string fieldPath, FilterCompareOperator op, string? valueText = null)
    {
        var c = new FilterConditionNode(fieldPath, op, valueText);
        _children.Add(c);
        return c;
    }

    public FilterGroupNode AddGroup(FilterLogicalKind kind = FilterLogicalKind.And)
    {
        var g = new FilterGroupNode { LogicalKind = kind };
        _children.Add(g);
        return g;
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (FilterNodeBase n in e.OldItems)
            {
                if (ReferenceEquals(n.Parent, this))
                    n.Parent = null;
            }
        }

        if (e.NewItems != null)
        {
            foreach (FilterNodeBase n in e.NewItems)
                n.Parent = this;
        }
    }
}
