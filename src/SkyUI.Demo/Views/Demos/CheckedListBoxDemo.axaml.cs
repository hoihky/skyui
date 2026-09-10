using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.Models;

namespace SkyUI.Demo.Views.Demos;

public partial class CheckedListBoxDemo : UserControl
{
    private readonly ObservableCollection<DemoCheckedListNode> roots = new();
    private readonly TitleComparer titleComparer = new();
    private readonly DemoCheckedListEditableAdapter editableAdapter = new();
    private DemoCheckedListRowActionProvider? rowActionProvider;

    public CheckedListBoxDemo()
    {
        InitializeComponent();
        SelectionModeCombo.ItemsSource = new[]
        {
            CheckedListBoxSelectionMode.None,
            CheckedListBoxSelectionMode.Single,
            CheckedListBoxSelectionMode.Multiple,
        };
        SelectionModeCombo.SelectedItem = CheckedListBoxSelectionMode.Multiple;

        foreach (var n in BuildSampleTree())
            roots.Add(n);

        TreeList.ItemsSource = roots;
        TreeList.ItemComparer = SortToggle.IsChecked == true ? titleComparer : null;
        TreeList.EditableAdapter = editableAdapter;
        TreeList.CheckedChanged += OnCheckedChanged;
        TreeList.SelectionChanged += OnSelectionChanged;
        TreeList.ReorderRequested += OnReorderRequested;
        TreeList.EditCommitted += (_, e) => Log($"Renamed → {e.Text}");
        TreeList.EditCancelled += (_, e) => Log($"Edit cancelled ({e.OriginalText})");

        rowActionProvider = new DemoCheckedListRowActionProvider(
            node => TreeList.BeginEditForItem(node),
            RemoveNode);
        TreeList.RowActionProvider = rowActionProvider;

        SelectionModeCombo.SelectionChanged += OnSelectionModeChanged;
        CascadeToggle.IsCheckedChanged += (_, _) => TreeList.CascadeToChildren = CascadeToggle.IsChecked == true;
        ThreeStateToggle.IsCheckedChanged += (_, _) => TreeList.UseThreeStateForParents = ThreeStateToggle.IsChecked == true;
        SortToggle.IsCheckedChanged += OnSortToggleChanged;
        ReorderToggle.IsCheckedChanged += (_, _) => TreeList.AllowReorder = ReorderToggle.IsChecked == true;
        InlineEditToggle.IsCheckedChanged += (_, _) => TreeList.AllowInlineEdit = InlineEditToggle.IsChecked == true;
        RowActionsToggle.IsCheckedChanged += (_, _) =>
            TreeList.RowActionProvider = RowActionsToggle.IsChecked == true ? rowActionProvider : null;
        AddRootButton.Click += OnAddRootClick;
        ClearLogButton.Click += (_, _) => EventLog.Text = string.Empty;

        Log("Ready. Current selection mode: Multiple.");
    }

    private static IEnumerable<DemoCheckedListNode> BuildSampleTree()
    {
        var europe = new DemoCheckedListNode("Europe", new ObservableCollection<DemoCheckedListNode>
        {
            new("Paris"),
            new("Berlin"),
            new("Amsterdam"),
        });
        var asia = new DemoCheckedListNode("Asia", new ObservableCollection<DemoCheckedListNode>
        {
            new("Tokyo"),
            new("Seoul"),
            new("Singapore"),
        });
        var americas = new DemoCheckedListNode("Americas", new ObservableCollection<DemoCheckedListNode>
        {
            new("Toronto"),
            new("São Paulo"),
            new("Mexico City"),
        });
        return new[] { europe, asia, americas };
    }

    private void OnSelectionModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectionModeCombo.SelectedItem is CheckedListBoxSelectionMode mode)
        {
            TreeList.SelectionMode = mode;
            Log($"SelectionMode → {mode}");
        }
    }

    private void OnSortToggleChanged(object? sender, RoutedEventArgs e)
    {
        TreeList.ItemComparer = SortToggle.IsChecked == true ? titleComparer : null;
        Log(SortToggle.IsChecked == true ? "Sorting: on (A–Z per sibling group)." : "Sorting: off.");
    }

    private void OnAddRootClick(object? sender, RoutedEventArgs e)
    {
        var i = roots.Count + 1;
        roots.Add(new DemoCheckedListNode($"New region {i}", new ObservableCollection<DemoCheckedListNode>
        {
            new($"City A-{i}"),
            new($"City B-{i}"),
        }));
        Log($"Added root 'New region {i}' (live INotifyCollectionChanged on roots).");
    }

    private void OnCheckedChanged(object? sender, CheckedListBoxCheckedChangedEventArgs e) =>
        Log($"Checked: item={e.Item} value={e.NewValue?.ToString() ?? "null"}");

    private void OnSelectionChanged(object? sender, CheckedListBoxSelectionChangedEventArgs e) =>
        Log($"Selection: item={e.Item} selected={e.IsSelected}");

    private void OnReorderRequested(object? sender, CheckedListReorderEventArgs e)
    {
        var adapter = TreeList.ItemAdapter ?? new DefaultCheckedListItemAdapter();
        if (CheckedListReorderHelper.TryReorderSibling(roots, e.SourceItem, e.TargetItem, e.Position, e.SharedParent, adapter))
        {
            if (SortToggle.IsChecked == true)
            {
                SortToggle.IsChecked = false;
                TreeList.ItemComparer = null;
                Log("Sorting turned off so manual order is preserved.");
            }

            Log($"Reordered {((DemoCheckedListNode?)e.SourceItem)?.Title} {e.Position} {((DemoCheckedListNode?)e.TargetItem)?.Title}");
        }
        else
            Log("Reorder failed (siblings only).");
    }

    private void RemoveNode(DemoCheckedListNode node)
    {
        if (!TryRemove(roots, node))
            Log($"Could not remove {node.Title}");
        else
            Log($"Removed {node.Title}");
    }

    private static bool TryRemove(IList<DemoCheckedListNode> collection, DemoCheckedListNode node)
    {
        if (collection.Remove(node))
            return true;

        foreach (var item in collection)
        {
            if (item.Children is null)
                continue;
            if (TryRemove(item.Children, node))
                return true;
        }

        return false;
    }

    private void Log(string line) =>
        EventLog.Text = string.IsNullOrEmpty(EventLog.Text) ? line : EventLog.Text + "\n" + line;

    private sealed class TitleComparer : IComparer<object?>
    {
        public int Compare(object? x, object? y)
        {
            var a = (x as DemoCheckedListNode)?.Title ?? string.Empty;
            var b = (y as DemoCheckedListNode)?.Title ?? string.Empty;
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
