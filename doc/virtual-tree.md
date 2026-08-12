# Virtual tree view

`SkyVirtualTreeView` is a themed hierarchical list with UI virtualization and optional checkboxes. It reuses the same flattening, adapter, and selection infrastructure as `CheckedListBox`.

## Controls

| Control | Use for |
|---------|---------|
| `SkyVirtualTreeView` | Explorer-style tree (checkboxes off by default) |
| `CheckedListBox` | Hierarchical checklist (checkboxes always on) |

Both use `VirtualizingStackPanel` to recycle row visuals for expanded nodes.

## MVVM

Implement `ICheckedListBoxItem` on your node type, or supply a custom `ICheckedListItemAdapter`:

```xml
<sky:SkyVirtualTreeView ItemsSource="{Binding Folders}"
                       SelectionMode="Single"
                       ShowCheckBoxes="{Binding AllowChecks}"
                       Indent="20">
  <sky:SkyVirtualTreeView.ItemTemplate>
    <DataTemplate x:DataType="vm:FolderNode">
      <TextBlock Text="{Binding Name}" />
    </DataTemplate>
  </sky:SkyVirtualTreeView.ItemTemplate>
</sky:SkyVirtualTreeView>
```

### Node contract (`ICheckedListBoxItem`)

| Property | Purpose |
|----------|---------|
| `IsExpanded` | Show/hide children |
| `IsChecked` | Tri-state checkbox state (when `ShowCheckBoxes` is true) |
| `Children` | Child collection |

## Selection

`CheckedListBoxSelectionMode`: `None`, `Single`, `Multiple` (Ctrl+click for multi-select).

## Checkboxes

Set `ShowCheckBoxes="True"` on `SkyVirtualTreeView` to enable tri-state parent aggregation via `CascadeToChildren` and `UseThreeStateForParents` (inherited from `CheckedListBox`).

## Related

- [Development roadmap](./development-roadmap.md) — Lists sprint
- `CheckedListBox` demo — full checkbox + sort + cascade examples
