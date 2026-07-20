using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.FilterEditor;

/// <summary>Visitor for filter tree (OCP: add exporters / analyzers without changing node types).</summary>
public interface IFilterNodeVisitor<out T>
{
    T VisitGroup(FilterGroupNode node);

    T VisitCondition(FilterConditionNode node);
}

/// <summary>Base node in a filter tree (MVVM: <see cref="INotifyPropertyChanged"/>).</summary>
public abstract class FilterNodeBase : INotifyPropertyChanged
{
    private FilterGroupNode? _parent;

    public FilterGroupNode? Parent
    {
        get => _parent;
        internal set
        {
            if (ReferenceEquals(_parent, value))
                return;
            _parent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Depth));
        }
    }

    /// <summary>Depth from the document root group (1 = direct child of root).</summary>
    public int Depth
    {
        get
        {
            var d = 0;
            for (var p = Parent; p != null; p = p.Parent)
                d++;
            return d;
        }
    }

    /// <summary>Child nodes for hierarchical UI (empty collection for conditions).</summary>
    public abstract ObservableCollection<FilterNodeBase> Children { get; }

    public abstract T Accept<T>(IFilterNodeVisitor<T> visitor);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Removes this node from its parent group.</summary>
    public void RemoveFromParent()
    {
        if (Parent is null)
            return;
        Parent.Children.Remove(this);
        Parent = null;
    }
}
