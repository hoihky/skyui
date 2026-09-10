using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.UnitTests.Shell;

public class SkyCommandBarTests
{
    [Fact]
    public void Title_property_roundtrips()
    {
        var bar = new SkyCommandBar { Title = "Documents" };
        Assert.Equal("Documents", bar.Title);
    }

    [Fact]
    public void Command_collections_are_mutable()
    {
        var bar = new SkyCommandBar();
        var item = new SkyCommandBarItem { Label = "Filter", IconKind = SkyIconKind.Filter };

        bar.PrimaryCommands.Add(item);

        Assert.Single(bar.PrimaryCommands);
        Assert.Same(item, bar.PrimaryCommands[0]);
    }

    [Fact]
    public void SkyCommandBarItem_command_property_roundtrips()
    {
        var item = new SkyCommandBarItem();
        Assert.Null(item.Command);
    }
}
