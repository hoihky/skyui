using Avalonia;
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
    private readonly Action<CheckedListRowModel, PointerReleasedEventArgs> _pointerPressed;
    private readonly Action<CheckedListRowModel>? _expandRequested;
    private bool isSelected;
    private bool isEditing;
    private bool isLoadingChildren;
    private string editText = string.Empty;
    private Rect rowBounds;
    private ICommand? toggleExpandCommand;

    internal CheckedListRowModel(
        object? item,
        int depth,
        bool hasChildren,
        ICheckedListItemAdapter adapter,
        Action requestStructureRebuild,
        Action<CheckedListRowModel, bool?> checkCommitted,
        Action<CheckedListRowModel, PointerReleasedEventArgs> pointerPressed,
        Action<CheckedListRowModel>? expandRequested = null)
    {
        Item = item;
        Depth = depth;
        HasChildren = hasChildren;
        _adapter = adapter;
        _requestStructureRebuild = requestStructureRebuild;
        _checkCommitted = checkCommitted;
        _pointerPressed = pointerPressed;
        _expandRequested = expandRequested;
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
            if (value)
                _expandRequested?.Invoke(this);
            _requestStructureRebuild();
        }
    }

    public bool IsLoadingChildren
    {
        get => isLoadingChildren;
        internal set
        {
            if (isLoadingChildren == value)
                return;
            isLoadingChildren = value;
            OnPropertyChanged();
        }
    }

    public bool IsEditing
    {
        get => isEditing;
        internal set
        {
            if (isEditing == value)
                return;
            isEditing = value;
            OnPropertyChanged();
        }
    }

    public string EditText
    {
        get => editText;
        internal set
        {
            if (editText == value)
                return;
            editText = value;
            OnPropertyChanged();
        }
    }

    public Rect RowBounds
    {
        get => rowBounds;
        internal set
        {
            if (rowBounds == value)
                return;
            rowBounds = value;
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
        get => isSelected;
        set
        {
            if (isSelected == value)
                return;
            isSelected = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleExpandCommand =>
        toggleExpandCommand ??= new CheckedListRelayCommand(
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

    internal void HandleRowPointerPressed(PointerReleasedEventArgs e) => _pointerPressed(this, e);

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
