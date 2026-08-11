using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class FeedbackHeadlessTests
{
    [Fact]
    public void SkySnackbarHost_enqueues_message()
    {
        var host = new SkySnackbarHost();
        host.Enqueue("Saved", SkyFeedbackVariant.Success);

        Assert.Single(host.Messages);
        Assert.Equal("Saved", host.Messages[0].Message);
    }

    [Fact]
    public void SkyDialogHost_open_and_close()
    {
        var dialog = new SkyDialogHost();
        dialog.Show();
        Assert.True(dialog.IsOpen);

        dialog.Close();
        Assert.False(dialog.IsOpen);
    }

    [Fact]
    public void SkyBanner_close_via_handler_sets_is_open_false()
    {
        var banner = new SkyBanner { IsOpen = true, IsCloseable = true };
        banner.CloseRequested += (_, _) => banner.IsOpen = false;
        banner.RaiseEvent(new RoutedEventArgs(SkyBanner.CloseRequestedEvent));
        Assert.False(banner.IsOpen);
    }

    [Fact]
    public void Feedback_controls_instantiate() =>
        Assert.NotNull(new SkyAlert { Message = "Hi" });
}
