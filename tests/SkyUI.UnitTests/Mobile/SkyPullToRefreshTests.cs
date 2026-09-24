using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyPullToRefreshTests
{
    [Fact]
    public void Defaults_match_mobile_guidance()
    {
        var refresh = new SkyPullToRefresh();
        Assert.False(refresh.IsRefreshing);
        Assert.Equal(64, refresh.PullThreshold);
        Assert.Equal(112, refresh.MaxPullDistance);
        Assert.Equal(48, refresh.IndicatorHeight);
        Assert.True(refresh.Classes.Contains("sky-pull-to-refresh"));
    }

    [Fact]
    public void Interactor_skips_refresh_when_gate_closed()
    {
        var scroll = new ScrollViewer();
        var refreshed = false;
        using var interactor = new SkyPullToRefreshGestureInteractor(
            scroll,
            () => false,
            _ => { },
            () =>
            {
                refreshed = true;
                return true;
            });

        interactor.Attach();
        Assert.False(refreshed);
    }

    [Fact]
    public void Content_accepts_scroll_viewer_child()
    {
        var refresh = new SkyPullToRefresh
        {
            Content = new ScrollViewer { Content = new StackPanel() },
        };

        Assert.IsType<ScrollViewer>(refresh.Content);
    }
}
