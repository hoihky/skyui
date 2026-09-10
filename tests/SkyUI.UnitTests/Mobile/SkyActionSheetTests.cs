using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyActionSheetTests
{
    [Fact]
    public async Task ShowAsync_returns_selected_index()
    {
        var host = new SkySheetHost();
        var task = SkyActionSheet.ShowAsync([
            new SkyActionSheetItem { Title = "Share" },
            new SkyActionSheetItem { Title = "Delete", IsDestructive = true },
        ], title: "Actions", sheetHost: host);

        Assert.True(host.IsOpen);
        var root = Assert.IsType<StackPanel>(host.SheetContent);
        var share = root.Children.OfType<Button>().First(b => b.Content?.ToString() == "Share");
        share.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var index = await task;
        Assert.Equal(0, index);
        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task ShowAsync_cancel_returns_null()
    {
        var host = new SkySheetHost();
        var task = SkyActionSheet.ShowAsync([
            new SkyActionSheetItem { Title = "Share" },
        ], sheetHost: host);

        var root = Assert.IsType<StackPanel>(host.SheetContent);
        var cancel = root.Children.OfType<Button>().First(b => b.Content?.ToString() == "Cancel");
        cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var index = await task;
        Assert.Null(index);
    }
}
