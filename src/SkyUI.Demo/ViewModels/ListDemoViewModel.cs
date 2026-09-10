using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Demo.Models;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for virtual tree view with optional checkboxes.</summary>
public sealed class ListDemoViewModel : INotifyPropertyChanged
{
    private bool _showCheckBoxes;
    private CheckedListBoxSelectionMode _selectionMode = CheckedListBoxSelectionMode.Single;
    private string _status = "Select a row or toggle checkboxes when enabled.";

    public ListDemoViewModel()
    {
        Items = DemoTreeData.CreateFileExplorerTree();
        LazyItems =
        [
            new LazyDemoTreeNode("Projects", hasLazyChildren: true),
            new LazyDemoTreeNode("Shared drives", hasLazyChildren: true),
        ];
        LazyDataSource = new LazyDemoTreeDataSource();
        LazyItemAdapter = new AsyncCheckedListItemAdapter(new DefaultCheckedListItemAdapter(), LazyDataSource);
        LazyEditableAdapter = new LazyDemoTreeEditableAdapter();
        ToggleCheckBoxesCommand = new RelayCommand(() => ShowCheckBoxes = !ShowCheckBoxes);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DemoCheckedListNode> Items { get; }

    public ObservableCollection<LazyDemoTreeNode> LazyItems { get; }

    public LazyDemoTreeDataSource LazyDataSource { get; }

    public AsyncCheckedListItemAdapter LazyItemAdapter { get; }

    public LazyDemoTreeEditableAdapter LazyEditableAdapter { get; }

    public bool ShowCheckBoxes
    {
        get => _showCheckBoxes;
        set
        {
            if (_showCheckBoxes == value)
                return;
            _showCheckBoxes = value;
            Notify(nameof(ShowCheckBoxes), nameof(CheckBoxesLabel));
        }
    }

    public string CheckBoxesLabel => ShowCheckBoxes ? "Hide checkboxes" : "Show checkboxes";

    public CheckedListBoxSelectionMode SelectionMode
    {
        get => _selectionMode;
        set
        {
            if (_selectionMode == value)
                return;
            _selectionMode = value;
            Notify(nameof(SelectionMode));
        }
    }

    public IReadOnlyList<CheckedListBoxSelectionMode> SelectionModes { get; } =
    [
        CheckedListBoxSelectionMode.None,
        CheckedListBoxSelectionMode.Single,
        CheckedListBoxSelectionMode.Multiple,
    ];

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

    public ICommand ToggleCheckBoxesCommand { get; }

    public void OnSelectionChanged(object? item, bool isSelected) =>
        Status = isSelected
            ? $"Selected: {(item as DemoCheckedListNode)?.Title ?? item?.ToString() ?? "(null)"}"
            : "Selection cleared";

    public void OnCheckedChanged(object? item, bool? value) =>
        Status = $"Checked {(item as DemoCheckedListNode)?.Title ?? item?.ToString() ?? "(null)"}: {FormatCheck(value)}";

    private static string FormatCheck(bool? value) =>
        value switch
        {
            true => "checked",
            false => "unchecked",
            _ => "indeterminate"
        };

    private void Notify(params string[] names)
    {
        foreach (var name in names)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
