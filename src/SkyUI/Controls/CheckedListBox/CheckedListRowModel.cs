using Avalonia.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyUI.Controls;

/// <summary>
/// One visible row in the flattened tree (MVVM row state; virtualization recycles the presenter, not this model).
/// </summary>
public sealed class CheckedListRowModel : INotifyPropertyChanged
{
    private readonly ICheckedListItemAdapter _adapter;
    private readonly Action _requestStructureRebuild;
    private readonly Action<CheckedListRowModel, bool?> _checkCommitted;
    private readonly Action<CheckedListRowModel, PointerPressedEventArgs> _pointerPressed;
    private bool _isSelected;
    private ICommand? _toggleExpandCommand;

    internal CheckedListRowModel(
        object? item,
        int depth,
        bool hasChildren,
        ICheckedListItemAdapter adapter,
        Action requestStructureRebuild,
        Action<CheckedListRowModel, bool?> checkCommitted,
        Action<CheckedListRowModel, PointerPressedEventArgs> pointerPressed)
    {
        Item = item;
        Depth = depth;
        HasChildren = hasChildren;
        _adapter = adapter;
        _requestStructureRebuild = requestStructureRebuild;
        _checkCommitted = checkCommitted;
        _pointerPressed = pointerPressed;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public object? Item { get; }

    public int Depth { get; }

    public bool HasChildren { get; }

    public bool IsExpanded
    {
        get => _adapter.GetIsExpanded(Item);
        set
        {
            if (IsExpanded == value)
                return;
            _adapter.SetIsExpanded(Item, value);
            OnPropertyChanged(nameof(IsExpanded));
            _requestStructureRebuild();
        }
    }

    public bool? IsChecked
    {
        get => _adapter.GetIsChecked(Item);
        set
        {
            var cur = _adapter.GetIsChecked(Item);
            if (cur == value)
                return;
            _adapter.SetIsChecked(Item, value);
            OnPropertyChanged(nameof(IsChecked));
            _checkCommitted(this, value);
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleExpandCommand =>
        _toggleExpandCommand ??= new CheckedListRelayCommand(
            () =>
            {
                if (!HasChildren)
                    return;
                IsExpanded = !IsExpanded;
            },
            () => HasChildren);

    internal void NotifyCheckFromAdapter()
    {
        OnPropertyChanged(nameof(IsChecked));
    }

    internal void NotifyExpandFromAdapter()
    {
        OnPropertyChanged(nameof(IsExpanded));
    }

    internal void HandleRowPointerPressed(PointerPressedEventArgs e) => _pointerPressed(this, e);

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
