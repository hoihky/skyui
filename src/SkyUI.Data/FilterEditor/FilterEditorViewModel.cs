using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyUI.FilterEditor;

/// <summary>MVVM surface for <see cref="FilterEditor"/>: commands, selection, SQL preview, and live wiring to the document tree.</summary>
public sealed class FilterEditorViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly FilterDocument _document;
    private IFilterSqlExporter _exporter;
    private FilterNodeBase? _selectedNode;
    private string _sqlPreview = "";
    private bool _disposed;

    public FilterEditorViewModel(FilterDocument document, IFilterSqlExporter? exporter = null)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _exporter = exporter ?? new BasicFilterSqlExporter();

        AddAndGroupCommand = new FilterEditorRelayCommand(() => AddGroup(FilterLogicalKind.And));
        AddOrGroupCommand = new FilterEditorRelayCommand(() => AddGroup(FilterLogicalKind.Or));
        AddConditionCommand = new FilterEditorRelayCommand(AddCondition);
        RemoveSelectedCommand = new FilterEditorRelayCommand(RemoveSelected, CanRemoveSelected);
        RemoveNodeCommand = new FilterEditorNodeCommand(RemoveNode);

        _document.Fields.CollectionChanged += OnFieldsCollectionChanged;
        WireFilterGroup(_document.Root);
        RefreshSql();
    }

    public FilterDocument Document => _document;

    public IReadOnlyList<FilterLogicalKind> LogicalKindOptions => FilterEditorThemeLists.LogicalKinds;

    public IReadOnlyList<FilterCompareOperator> CompareOperatorOptions => FilterEditorThemeLists.CompareOperators;

    public FilterNodeBase? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (ReferenceEquals(_selectedNode, value))
                return;
            _selectedNode = value;
            OnPropertyChanged();
        }
    }

    public string SqlPreview
    {
        get => _sqlPreview;
        private set
        {
            if (_sqlPreview == value)
                return;
            _sqlPreview = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddAndGroupCommand { get; }
    public ICommand AddOrGroupCommand { get; }
    public ICommand AddConditionCommand { get; }
    public ICommand RemoveSelectedCommand { get; }
    public ICommand RemoveNodeCommand { get; }

    public void SetSqlExporter(IFilterSqlExporter exporter)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        _exporter = exporter;
        RefreshSql();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _document.Fields.CollectionChanged -= OnFieldsCollectionChanged;
        UnwireFilterGroup(_document.Root);
    }

    private void OnFieldsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSql();

    private void WireFilterGroup(FilterGroupNode g)
    {
        g.Children.CollectionChanged += OnGroupChildrenChanged;
        foreach (var child in g.Children)
            WireNode(child);
    }

    private void WireNode(FilterNodeBase node)
    {
        node.PropertyChanged += OnNodePropertyChanged;
        if (node is FilterGroupNode childGroup)
            WireFilterGroup(childGroup);
    }

    private void OnGroupChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (FilterNodeBase n in e.NewItems)
                WireNode(n);
        }

        if (e.OldItems != null)
        {
            foreach (FilterNodeBase n in e.OldItems)
                UnwireNode(n);
        }

        RefreshSql();
    }

    private void UnwireFilterGroup(FilterGroupNode g)
    {
        g.Children.CollectionChanged -= OnGroupChildrenChanged;
        foreach (var child in g.Children.ToArray())
            UnwireNode(child);
    }

    private void UnwireNode(FilterNodeBase node)
    {
        node.PropertyChanged -= OnNodePropertyChanged;
        if (node is FilterGroupNode childGroup)
            UnwireFilterGroup(childGroup);
    }

    private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e) => RefreshSql();

    private void RefreshSql()
    {
        SqlPreview = _exporter.ToSql(_document);
        _document.RaiseStructureChanged();
    }

    private FilterGroupNode TargetGroup =>
        SelectedNode switch
        {
            null => _document.Root,
            FilterGroupNode g => g,
            FilterConditionNode c => c.Parent ?? _document.Root,
            _ => _document.Root,
        };

    private void AddGroup(FilterLogicalKind kind)
    {
        TargetGroup.AddGroup(kind);
        RefreshSql();
    }

    private void AddCondition()
    {
        var path = _document.Fields.FirstOrDefault()?.PropertyPath ?? "Field";
        TargetGroup.AddCondition(path, FilterCompareOperator.Equal, "");
        RefreshSql();
    }

    private bool CanRemoveSelected() =>
        SelectedNode is not null && !ReferenceEquals(SelectedNode, _document.Root);

    private void RemoveSelected()
    {
        if (!CanRemoveSelected())
            return;
        SelectedNode?.RemoveFromParent();
        SelectedNode = null;
        RefreshSql();
    }

    private void RemoveNode(FilterNodeBase? node)
    {
        if (node is null || ReferenceEquals(node, _document.Root))
            return;
        if (SelectedNode is not null &&
            (ReferenceEquals(SelectedNode, node) || IsUnder(SelectedNode, node)))
            SelectedNode = null;
        node.RemoveFromParent();
        RefreshSql();
    }

    private static bool IsUnder(FilterNodeBase? selected, FilterNodeBase subtreeRoot)
    {
        for (var p = selected?.Parent; p != null; p = p.Parent)
        {
            if (ReferenceEquals(p, subtreeRoot))
                return true;
        }

        return false;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
