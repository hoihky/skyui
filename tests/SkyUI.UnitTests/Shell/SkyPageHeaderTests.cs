using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.UnitTests.Shell;

public class SkyPageHeaderTests
{
    [Fact]
    public void Back_requested_event_can_be_handled()
    {
        var header = new SkyPageHeader { IsBackButtonVisible = true };
        var raised = false;
        header.BackRequested += (_, _) => raised = true;

        header.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(SkyPageHeader.BackRequestedEvent));

        Assert.True(raised);
    }

    [Fact]
    public void Back_command_can_be_assigned()
    {
        var command = new TestCommand();
        var header = new SkyPageHeader
        {
            IsBackButtonVisible = true,
            BackCommand = command,
        };

        Assert.True(command.CanExecute(null));
        Assert.Equal(command, header.BackCommand);
    }

    [Fact]
    public void Action_content_accepts_arbitrary_content()
    {
        var header = new SkyPageHeader { ActionContent = "Actions" };
        Assert.Equal("Actions", header.ActionContent);
    }

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}
