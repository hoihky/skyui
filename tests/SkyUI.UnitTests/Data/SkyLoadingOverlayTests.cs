using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyLoadingOverlayTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var overlay = new SkyLoadingOverlay();
        Assert.Contains("sky", overlay.Classes);
        Assert.Contains("sky-loading-overlay", overlay.Classes);
    }

    [Fact]
    public void IsLoading_sets_active_class()
    {
        var overlay = new SkyLoadingOverlay { IsLoading = true };
        Assert.Contains("sky-loading-overlay-active", overlay.Classes);

        overlay.IsLoading = false;
        Assert.DoesNotContain("sky-loading-overlay-active", overlay.Classes);
    }

    [Fact]
    public void Content_and_loading_text_bind()
    {
        var overlay = new SkyLoadingOverlay
        {
            Content = new TextBlock { Text = "Body" },
            LoadingText = "Loading…"
        };

        Assert.IsType<TextBlock>(overlay.Content);
        Assert.Equal("Loading…", overlay.LoadingText);
    }

    [Fact]
    public void BlocksInput_defaults_to_true()
    {
        var overlay = new SkyLoadingOverlay();
        Assert.True(overlay.BlocksInput);
    }
}
