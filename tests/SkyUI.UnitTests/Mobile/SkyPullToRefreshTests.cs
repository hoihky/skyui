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
    public void Interactor_attaches_when_scroll_viewer_has_visual_children()
    {
        var scroll = new ScrollViewer
        {
            Content = new Border { Height = 120, Width = 200 },
        };

        using var interactor = new SkyPullToRefreshGestureInteractor(
            scroll,
            () => true,
            _ => { },
            () => false);

        var exception = Record.Exception(() => interactor.Attach());
        Assert.Null(exception);
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

    [Fact]
    public void Clearing_IsRefreshing_resets_pull_offset()
    {
        var refresh = new SkyPullToRefresh
        {
            Content = new ScrollViewer { Content = new StackPanel() },
        };

        refresh.IsRefreshing = true;
        refresh.IsRefreshing = false;

        Assert.False(refresh.IsRefreshing);
        Assert.Equal(0, refresh.PullOffset);
    }

    [Fact]
    public void Refresh_command_can_execute_before_refreshing_flag_is_set()
    {
        var refresh = new SkyPullToRefresh();
        var executed = false;
        var command = new RelayCommand(
            () => executed = true,
            () => !refresh.IsRefreshing);

        refresh.RefreshCommand = command;

        Assert.True(command.CanExecute(null));
        command.Execute(null);
        Assert.True(executed);

        refresh.IsRefreshing = true;
        Assert.False(command.CanExecute(null));
    }

    private sealed class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => execute();
    }
}
