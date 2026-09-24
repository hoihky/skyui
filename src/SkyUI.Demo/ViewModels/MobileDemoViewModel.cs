using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for mobile FAB and pull-to-refresh.</summary>
public sealed class MobileDemoViewModel : INotifyPropertyChanged
{
    private bool isRefreshing;
    private bool isExtendedFab;
    private string status = "Pull down to refresh or tap the FAB.";

    public MobileDemoViewModel()
    {
        Items = new ObservableCollection<string>(Enumerable.Range(1, 12).Select(i => $"Inbox item {i}"));
        RefreshCommand = new RelayCommand(ExecuteRefresh, () => !IsRefreshing);
        AddItemCommand = new RelayCommand(AddItem);
        ToggleFabCommand = new RelayCommand(() => IsExtendedFab = !IsExtendedFab);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Items { get; }

    public bool IsRefreshing
    {
        get => isRefreshing;
        set
        {
            if (isRefreshing == value)
                return;
            isRefreshing = value;
            Notify(nameof(IsRefreshing));
        }
    }

    public bool IsExtendedFab
    {
        get => isExtendedFab;
        set
        {
            if (isExtendedFab == value)
                return;
            isExtendedFab = value;
            Notify(nameof(IsExtendedFab), nameof(FabLabel));
        }
    }

    public string FabLabel => IsExtendedFab ? "Compose" : string.Empty;

    public string Status
    {
        get => status;
        private set
        {
            if (status == value)
                return;
            status = value;
            Notify(nameof(Status));
        }
    }

    public ICommand RefreshCommand { get; }

    public ICommand AddItemCommand { get; }

    public ICommand ToggleFabCommand { get; }

    private async void ExecuteRefresh()
    {
        IsRefreshing = true;
        Status = "Refreshing…";
        await Task.Delay(900);
        Items.Insert(0, $"Inbox item {DateTime.Now:HH:mm:ss}");
        IsRefreshing = false;
        Status = $"Refreshed at {DateTime.Now:HH:mm:ss}.";
    }

    private void AddItem()
    {
        Items.Add($"Draft {Items.Count + 1}");
        Status = "FAB added a draft item.";
    }

    private void Notify(params string[] names)
    {
        foreach (var name in names)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => execute();
    }
}
