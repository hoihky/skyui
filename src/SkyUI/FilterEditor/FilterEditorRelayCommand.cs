using System.Windows.Input;

namespace SkyUI.FilterEditor;

internal sealed class FilterEditorRelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public FilterEditorRelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}

internal sealed class FilterEditorNodeCommand : ICommand
{
    private readonly Action<FilterNodeBase?> _execute;

    public FilterEditorNodeCommand(Action<FilterNodeBase?> execute) => _execute = execute;

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => parameter is FilterNodeBase;

    public void Execute(object? parameter) => _execute(parameter as FilterNodeBase);
}
