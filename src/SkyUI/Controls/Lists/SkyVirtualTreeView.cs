namespace SkyUI.Controls;

/// <summary>
/// Virtualized hierarchical list with optional checkboxes. Reuses <see cref="CheckedListBox"/> flattening,
/// adapters, and <see cref="VirtualizingStackPanel"/> row recycling.
/// </summary>
public class SkyVirtualTreeView : CheckedListBox
{
    public SkyVirtualTreeView()
    {
        Classes.Add("sky");
        Classes.Add("sky-virtual-tree");
        ShowCheckBoxes = false;
        SelectionMode = CheckedListBoxSelectionMode.Single;
        Indent = 20;
    }
}
