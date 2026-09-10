using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.Demo.Models;

public sealed class DemoCheckedListRowActionProvider : ICheckedListRowActionProvider
{
    private readonly Action<DemoCheckedListNode> onRename;
    private readonly Action<DemoCheckedListNode> onRemove;

    public DemoCheckedListRowActionProvider(
        Action<DemoCheckedListNode> onRename,
        Action<DemoCheckedListNode> onRemove)
    {
        this.onRename = onRename;
        this.onRemove = onRemove;
    }

    public IReadOnlyList<CheckedListRowAction> GetActions(object? item)
    {
        if (item is not DemoCheckedListNode node)
            return Array.Empty<CheckedListRowAction>();

        return
        [
            new CheckedListRowAction
            {
                Label = "Rename",
                Command = new RelayAction(() => onRename(node)),
            },
            new CheckedListRowAction
            {
                Label = "Remove",
                Command = new RelayAction(() => onRemove(node)),
            },
        ];
    }

    private sealed class RelayAction(Action execute) : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
