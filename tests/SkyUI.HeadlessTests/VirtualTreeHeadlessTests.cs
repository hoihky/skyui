using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class VirtualTreeHeadlessTests
{
    [Fact]
    public void SkyVirtualTreeView_exposes_rows_collection()
    {
        var tree = new SkyVirtualTreeView();
        Assert.NotNull(tree.Rows);
        Assert.Empty(tree.Rows);
    }

    [Fact]
    public void ShowCheckBoxes_defaults_differ_between_tree_and_checked_list()
    {
        Assert.False(new SkyVirtualTreeView().ShowCheckBoxes);
        Assert.True(new CheckedListBox().ShowCheckBoxes);
    }
}
