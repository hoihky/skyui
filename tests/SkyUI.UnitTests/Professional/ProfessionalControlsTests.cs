using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class ProfessionalControlsTests
{
    [Fact]
    public void PropertyGridItem_notifies_value_changes()
    {
        var item = new PropertyGridItem { Name = "Width", Value = 10, ValueType = typeof(int) };
        var changes = 0;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PropertyGridItem.Value))
                changes++;
        };
        item.Value = 20;
        Assert.Equal(1, changes);
    }

    [Fact]
    public void SkyColorPicker_parses_hex_colors()
    {
        Assert.True(SkyColorPicker.TryParseHex("#FF8040", out var color));
        Assert.Equal(255, color.R);
    }

    [Fact]
    public void SkyColorPicker_hsv_conversion_is_stable()
    {
        var color = SkyColorPicker.ColorFromHsv(120, 1, 1);
        Assert.True(color.G > color.R);
    }

    [Fact]
    public void SkyDualRangeSlider_exposes_range_endpoints()
    {
        var slider = new SkyDualRangeSlider { RangeStart = 30, RangeEnd = 70 };
        Assert.Equal(30, slider.RangeStart);
        Assert.Equal(70, slider.RangeEnd);
    }

    [Fact]
    public void SkyTreeNodeItem_expansion_flag_toggles()
    {
        var node = new SkyTreeNodeItem { Header = "Root" };
        node.IsExpanded = true;
        Assert.True(node.IsExpanded);
    }

    [Fact]
    public void SkyWizardPage_can_block_progress()
    {
        var page = new SkyWizardPage { CanProceed = false };
        Assert.False(page.CanProceed);
    }

    [Fact]
    public void SkyImageViewer_zoom_is_clamped()
    {
        var viewer = new SkyImageViewer();
        viewer.Zoom = 100;
        Assert.True(viewer.Zoom <= 8);
    }

    [Fact]
    public void SkyTagEditor_tags_collection_accepts_strings()
    {
        var tags = new ObservableCollection<string> { "alpha" };
        var editor = new SkyTagEditor { Tags = tags };
        Assert.Equal("alpha", Assert.Single(tags));
        Assert.NotNull(editor.Tags);
    }

    [Fact]
    public void SkyStepper_selected_index_defaults_to_zero()
    {
        var stepper = new SkyStepper();
        Assert.Equal(0, stepper.SelectedIndex);
    }

    [Fact]
    public void SkyRichTextBox_text_property_round_trips()
    {
        var box = new SkyRichTextBox { Text = "Hello" };
        Assert.Equal("Hello", box.Text);
    }

    [Fact]
    public void DefaultPropertyGridEditorFactory_creates_checkbox_for_bool()
    {
        var factory = new DefaultPropertyGridEditorFactory();
        var editor = factory.CreateEditor(new PropertyGridItem { ValueType = typeof(bool), Value = true });
        Assert.IsType<Avalonia.Controls.CheckBox>(editor);
    }

    [Fact]
    public void SkyPropertyGrid_accepts_item_collection()
    {
        var grid = new SkyPropertyGrid
        {
            Items = new ObservableCollection<PropertyGridItem>
            {
                new() { Name = "Title", Value = "Demo", Category = "General", ValueType = typeof(string) },
            },
        };
        Assert.Single(grid.Items!.Cast<PropertyGridItem>());
    }

    [Fact]
    public void SkyPropertyGrid_show_grid_lines_defaults_true()
    {
        var grid = new SkyPropertyGrid();
        Assert.True(grid.ShowGridLines);
        grid.ShowGridLines = false;
        Assert.False(grid.ShowGridLines);
    }

    [Fact]
    public void SkyPropertyGrid_category_expansion_api()
    {
        var grid = new SkyPropertyGrid { CategoriesExpandedByDefault = true };
        Assert.True(grid.IsCategoryExpanded("General"));
        grid.SetCategoryExpanded("General", false);
        Assert.False(grid.IsCategoryExpanded("General"));
    }

    [Fact]
    public void SkyStepper_chrome_connector_length_defaults()
    {
        var chrome = new SkyStepperChrome();
        Assert.Equal(72, chrome.ConnectorLength);
        Assert.Equal(28, chrome.NodeDiameter);
    }
}
