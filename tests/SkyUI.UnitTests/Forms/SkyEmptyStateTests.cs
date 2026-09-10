using System.Windows.Input;
using Avalonia.Controls;
using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.UnitTests;

public class SkyEmptyStateTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var state = new SkyEmptyState();
        Assert.Contains("sky", state.Classes);
        Assert.Contains("sky-empty-state", state.Classes);
    }

    [Fact]
    public void Title_and_description_bind()
    {
        var state = new SkyEmptyState
        {
            Title = "No results",
            Description = "Try a different search term."
        };

        Assert.Equal("No results", state.Title);
        Assert.Equal("Try a different search term.", state.Description);
    }

    [Fact]
    public void IconKind_defaults_to_search()
    {
        var state = new SkyEmptyState();
        Assert.Equal(SkyIconKind.Search, state.IconKind);
    }

    [Fact]
    public void ActionText_and_command_bind()
    {
        var command = new TestCommand();
        var state = new SkyEmptyState
        {
            ActionText = "Create item",
            ActionCommand = command
        };

        Assert.Equal("Create item", state.ActionText);
        Assert.Same(command, state.ActionCommand);
    }

    [Fact]
    public void ActionContent_can_host_custom_control()
    {
        var button = new Button { Content = "Custom" };
        var state = new SkyEmptyState { ActionContent = button };
        Assert.Same(button, state.ActionContent);
    }

    private sealed class TestCommand : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}
