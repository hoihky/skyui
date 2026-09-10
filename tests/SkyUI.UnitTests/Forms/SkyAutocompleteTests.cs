using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyAutocompleteTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var control = new SkyAutocomplete();
        Assert.Contains("sky", control.Classes);
        Assert.Contains("sky-autocomplete", control.Classes);
    }

    [Fact]
    public void IsFreeTextAllowed_defaults_to_true()
    {
        var control = new SkyAutocomplete();
        Assert.True(control.IsFreeTextAllowed);
    }

    [Fact]
    public void MinimumQueryLength_defaults_to_one()
    {
        var control = new SkyAutocomplete();
        Assert.Equal(1, control.MinimumQueryLength);
    }

    [Fact]
    public void Text_and_selected_item_bind()
    {
        var control = new SkyAutocomplete
        {
            Text = "Alpha",
            SelectedItem = "a"
        };

        Assert.Equal("Alpha", control.Text);
        Assert.Equal("a", control.SelectedItem);
    }

    [Fact]
    public void ItemsSource_accepts_string_collection()
    {
        var control = new SkyAutocomplete
        {
            ItemsSource = new[] { "Apple", "Apricot", "Banana" }
        };

        Assert.NotNull(control.ItemsSource);
    }

    [Fact]
    public void SkyAutocompleteItem_stores_text_and_value()
    {
        var item = new SkyAutocompleteItem("Label", 42);
        Assert.Equal("Label", item.Text);
        Assert.Equal(42, item.Value);
    }

    private sealed class TestProvider : ISkyAutocompleteProvider
    {
        public Task<IReadOnlyList<SkyAutocompleteItem>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SkyAutocompleteItem> results =
                [new SkyAutocompleteItem(query, query)];
            return Task.FromResult(results);
        }
    }

    [Fact]
    public async Task Provider_search_returns_results()
    {
        var provider = new TestProvider();
        var results = await provider.SearchAsync("test");
        Assert.Single(results);
        Assert.Equal("test", results[0].Text);
    }
}
