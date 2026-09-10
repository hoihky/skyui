using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for data presentation controls.</summary>
public sealed class DataPresentationDemoViewModel : INotifyPropertyChanged
{
    private bool isLoading;
    private int pageChangedCount;

    public DataPresentationDemoViewModel()
    {
        GridRows = new ObservableCollection<DemoGridRow>(
            Enumerable.Range(1, 47).Select(index => new DemoGridRow(index, $"Item {index}", $"Category {(index % 4) + 1}")));

        CompactItems = new ObservableCollection<string>(
            Enumerable.Range(1, 23).Select(index => $"Compact row {index}"));

        RevenueSparkline = [12, 18, 14, 22, 19, 26, 24];
        UsersSparkline = [40, 38, 42, 39, 44, 41, 45];

        SimulateLoadCommand = new RelayCommand(SimulateLoad);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DemoGridRow> GridRows { get; }

    public ObservableCollection<string> CompactItems { get; }

    public IReadOnlyList<int> RevenueSparkline { get; }

    public IReadOnlyList<int> UsersSparkline { get; }

    public bool IsLoading
    {
        get => isLoading;
        set => SetField(ref isLoading, value);
    }

    public int PageChangedCount
    {
        get => pageChangedCount;
        set => SetField(ref pageChangedCount, value);
    }

    public ICommand SimulateLoadCommand { get; }

    public void RegisterPageChange() => PageChangedCount++;

    private async void SimulateLoad()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        await Task.Delay(1500);
        IsLoading = false;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public sealed record DemoGridRow(int Id, string Name, string Category);

    private sealed class RelayCommand(Action execute) : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
