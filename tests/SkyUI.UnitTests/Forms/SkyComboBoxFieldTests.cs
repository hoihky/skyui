using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyComboBoxFieldTests
{
    [Fact]
    public void Extends_sky_form_field()
    {
        var field = new SkyComboBoxField();
        Assert.IsAssignableFrom<SkyFormField>(field);
    }

    [Fact]
    public void Hosts_styled_combo_box()
    {
        var field = new SkyComboBoxField();
        Assert.IsType<ComboBox>(field.Content);
        Assert.Contains("sky", field.ComboBox.Classes);
    }

    [Fact]
    public void SelectedItem_syncs_with_combo_box()
    {
        var items = new[] { "One", "Two", "Three" };
        var field = new SkyComboBoxField
        {
            ItemsSource = items,
            SelectedItem = "Two"
        };

        Assert.Equal("Two", field.ComboBox.SelectedItem);
        Assert.Equal("Two", field.SelectedItem);
    }

    [Fact]
    public void GetInputValue_returns_selected_item()
    {
        var field = new SkyComboBoxField
        {
            ItemsSource = new[] { "A", "B" },
            SelectedItem = "B"
        };

        Assert.Equal("B", field.GetInputValue());
    }

    [Fact]
    public void PlaceholderText_forwards_to_combo_box()
    {
        var field = new SkyComboBoxField { PlaceholderText = "Choose one" };
        Assert.Equal("Choose one", field.ComboBox.PlaceholderText);
    }
}
