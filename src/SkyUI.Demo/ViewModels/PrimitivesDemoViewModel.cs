using System.ComponentModel;
using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for tooltip, popover, loading button, and disabled states.</summary>
public sealed class PrimitivesDemoViewModel : INotifyPropertyChanged
{
    private bool _isSaving;
    private string _status = "Hover controls to see tooltips. Open the popover or run a simulated save.";

    public PrimitivesDemoViewModel()
    {
        SaveCommand = new RelayCommand(ExecuteSave, () => !IsSaving);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value)
                return;
            _isSaving = value;
            Notify(nameof(IsSaving));
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;
            _status = value;
            Notify(nameof(Status));
        }
    }

    public ICommand SaveCommand { get; }

    public void ExecuteSave()
    {
        if (IsSaving)
            return;

        IsSaving = true;
        Status = "Saving…";

        _ = CompleteSaveAsync();
    }

    private async Task CompleteSaveAsync()
    {
        await Task.Delay(1500);
        IsSaving = false;
        Status = "Save completed.";
    }

    private void Notify(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
