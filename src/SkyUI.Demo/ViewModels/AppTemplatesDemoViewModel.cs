using System.ComponentModel;
using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.Demo.ViewModels;

public sealed class AppTemplatesDemoViewModel : INotifyPropertyChanged
{
    private string statusText = "Ready";
    private bool isDrawerOpen;

    public AppTemplatesDemoViewModel()
    {
        SaveSettingsCommand = new RelayCommand(() => SetStatus("Settings saved"));
        ResetSettingsCommand = new RelayCommand(() => SetStatus("Settings reset"));
        SubmitFormCommand = new RelayCommand(() => SetStatus("Form submitted"));
        CancelFormCommand = new RelayCommand(() => SetStatus("Form cancelled"));
        OpenDrawerCommand = new RelayCommand(() => IsDrawerOpen = true);
        NewItemCommand = new RelayCommand(() => SetStatus("New item"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText
    {
        get => statusText;
        private set
        {
            if (statusText == value)
                return;
            statusText = value;
            Notify(nameof(StatusText));
        }
    }

    public bool IsDrawerOpen
    {
        get => isDrawerOpen;
        set
        {
            if (isDrawerOpen == value)
                return;
            isDrawerOpen = value;
            Notify(nameof(IsDrawerOpen));
        }
    }

    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetSettingsCommand { get; }
    public ICommand SubmitFormCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand OpenDrawerCommand { get; }
    public ICommand NewItemCommand { get; }

    public string ProfileName { get; set; } = "Jane Cooper";
    public string ProfileEmail { get; set; } = "jane@example.com";

    public IReadOnlyList<string> Customers { get; } =
    [
        "Acme Corp",
        "Globex",
        "Initech",
    ];

    public string DetailText =>
        "SkyListDetailPage composes header, command bar, split view, status bar, and drawer.";

    private void SetStatus(string message) => StatusText = message;

    private void Notify(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
