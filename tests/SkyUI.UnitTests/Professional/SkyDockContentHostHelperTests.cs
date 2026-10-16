using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDockContentHostHelperTests
{
    [Fact]
    public void Assign_moves_control_between_presenters()
    {
        var text = new TextBlock { Text = "Selection properties" };
        var first = new ContentPresenter();
        var second = new ContentPresenter();

        SkyDockContentHostHelper.Assign(first, text);
        Assert.Same(text, first.Content);

        SkyDockContentHostHelper.Assign(second, text);
        Assert.Null(first.Content);
        Assert.Same(text, second.Content);
    }
}
