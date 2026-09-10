using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyMessageBoxTests
{
    [Fact]
    public async Task Confirm_primary_returns_primary_result()
    {
        var host = new SkyDialogHost();
        var task = SkyMessageBox.ConfirmAsync("Delete item?", dialogHost: host);
        host.RaiseEvent(new RoutedEventArgs(SkyDialogHost.PrimaryActionEvent));

        var result = await task;
        Assert.Equal(SkyMessageBoxResult.Primary, result);
    }

    [Fact]
    public async Task Confirm_secondary_returns_secondary_result()
    {
        var host = new SkyDialogHost();
        var task = SkyMessageBox.ConfirmAsync("Delete item?", dialogHost: host);

        host.RaiseEvent(new RoutedEventArgs(SkyDialogHost.SecondaryActionEvent));

        var result = await task;
        Assert.Equal(SkyMessageBoxResult.Secondary, result);
    }

    [Fact]
    public async Task Info_configures_alert_content()
    {
        var host = new SkyDialogHost();
        var task = SkyMessageBox.ShowInfoAsync("Saved.", dialogHost: host);

        Assert.IsType<SkyAlert>(host.DialogContent);
        var alert = (SkyAlert)host.DialogContent!;
        Assert.Equal("Saved.", alert.Message);
        Assert.Equal(SkyFeedbackVariant.Info, alert.Variant);

        host.RaiseEvent(new RoutedEventArgs(SkyDialogHost.PrimaryActionEvent));
        await task;
    }

    [Fact]
    public async Task Error_maps_to_danger_variant()
    {
        var host = new SkyDialogHost();
        var task = SkyMessageBox.ShowErrorAsync("Failed.", dialogHost: host);

        var alert = (SkyAlert)host.DialogContent!;
        Assert.Equal(SkyFeedbackVariant.Danger, alert.Variant);

        host.RaiseEvent(new RoutedEventArgs(SkyDialogHost.PrimaryActionEvent));
        await task;
    }

    [Fact]
    public async Task ShowAsync_without_host_throws()
    {
        SkyMessageBox.ResetForTests();
        await Assert.ThrowsAsync<InvalidOperationException>(() => SkyMessageBox.ShowInfoAsync("Hi"));
    }
}
