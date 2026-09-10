using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class VirtualTreeControlsTests
{
    [Fact]
    public void SkyVirtualTreeView_instantiates() =>
        Assert.NotNull(new SkyVirtualTreeView());

    [Fact]
    public void SkyVirtualTreeView_has_sky_classes()
    {
        var tree = new SkyVirtualTreeView();
        Assert.Contains("sky", tree.Classes);
        Assert.Contains("sky-virtual-tree", tree.Classes);
    }

    [Fact]
    public void SkyVirtualTreeView_defaults_hide_checkboxes()
    {
        var tree = new SkyVirtualTreeView();
        Assert.False(tree.ShowCheckBoxes);
        Assert.Equal(CheckedListBoxSelectionMode.Single, tree.SelectionMode);
    }

    [Fact]
    public void CheckedListBox_defaults_show_checkboxes() =>
        Assert.True(new CheckedListBox().ShowCheckBoxes);

    [Fact]
    public void ShowCheckBoxes_can_be_toggled_on_virtual_tree()
    {
        var tree = new SkyVirtualTreeView { ShowCheckBoxes = true };
        Assert.True(tree.ShowCheckBoxes);
    }

    [Fact]
    public void CheckedListBox_exposes_enhancement_properties()
    {
        var list = new CheckedListBox
        {
            AllowReorder = true,
            AllowInlineEdit = true,
        };

        Assert.True(list.AllowReorder);
        Assert.True(list.AllowInlineEdit);
    }
}
