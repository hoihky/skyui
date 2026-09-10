using System.ComponentModel;
using System.Windows.Input;
using SkyUI.Icons;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for app chrome: command bar, page header, split view, drawer, and status bar.</summary>
public sealed class AppChromeDemoViewModel : INotifyPropertyChanged
{
    private string statusText = "Ready";
    private string pageSubtitle = "Browse records and open the filter drawer.";
    private bool isPaneOpen = true;
    private bool isDrawerOpen;
    private double? progress;

    public AppChromeDemoViewModel()
    {
        NewCommand = new RelayCommand(() => SetStatus("New item"));
        SaveCommand = new RelayCommand(() => SetStatus("Saved"));
        RefreshCommand = new RelayCommand(() => SetStatus("Refreshed"));
        ExportCommand = new RelayCommand(() => SetStatus("Exported"));
        BackCommand = new RelayCommand(() => SetStatus("Back navigation requested"));
        OpenDrawerCommand = new RelayCommand(() => IsDrawerOpen = true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string PageTitle => "Customers";

    public string PageSubtitle
    {
        get => pageSubtitle;
        set
        {
            if (pageSubtitle == value)
                return;
            pageSubtitle = value;
            Notify(nameof(PageSubtitle));
        }
    }

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

    public double? Progress
    {
        get => progress;
        private set
        {
            if (progress == value)
                return;
            progress = value;
            Notify(nameof(Progress));
        }
    }

    public bool IsPaneOpen
    {
        get => isPaneOpen;
        set
        {
            if (isPaneOpen == value)
                return;
            isPaneOpen = value;
            Notify(nameof(IsPaneOpen));
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

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand OpenDrawerCommand { get; }

    public IReadOnlyList<string> PaneItems { get; } =
    [
        "Acme Corp",
        "Globex",
        "Initech",
        "Umbrella",
        "Wayne Enterprises",
    ];

    public string DetailText =>
        "Select a customer from the list pane. Use overlay mode on narrow widths and the drawer for filters.";

    private void SetStatus(string message)
    {
        StatusText = message;
        Progress = message == "Saved" ? 100 : null;
    }

    private void Notify(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
