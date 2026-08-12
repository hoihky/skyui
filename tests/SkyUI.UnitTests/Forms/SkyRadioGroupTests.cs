using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyRadioGroupTests
{
    [Fact]
    public void SelectedValue_defaults_to_null()
    {
        var group = new SkyRadioGroup();
        Assert.Null(group.SelectedValue);
    }

    [Fact]
    public void Items_collection_accepts_group_items()
    {
        var group = new SkyRadioGroup();
        group.Items.Add(new SkyRadioGroupItem { Label = "A", Value = "a" });
        group.Items.Add(new SkyRadioGroupItem { Label = "B", Value = "b" });

        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public void SelectedValue_can_be_set_to_item_value()
    {
        var group = new SkyRadioGroup
        {
            SelectedValue = "light"
        };

        Assert.Equal("light", group.SelectedValue);
    }

    [Fact]
    public void Orientation_defaults_to_vertical()
    {
        var group = new SkyRadioGroup();
        Assert.Equal(Avalonia.Layout.Orientation.Vertical, group.Orientation);
    }
}
