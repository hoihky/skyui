using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkyUI.Controls;

namespace SkyUI.Demo.Models;

/// <summary>Lazy tree node used with <see cref="LazyDemoTreeDataSource"/>.</summary>
public sealed class LazyDemoTreeNode : ICheckedListBoxItem
{
    private bool? isChecked;
    private bool isExpanded;

    public LazyDemoTreeNode(string title, bool hasLazyChildren = false)
    {
        Title = title;
        HasLazyChildren = hasLazyChildren;
        Children = new ObservableCollection<LazyDemoTreeNode>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; set; }

    public bool HasLazyChildren { get; }

    public ObservableCollection<LazyDemoTreeNode> Children { get; }

    public bool ChildrenLoaded { get; set; }

    public bool? IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value)
                return;
            isChecked = value;
            OnPropertyChanged();
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
                return;
            isExpanded = value;
            OnPropertyChanged();
        }
    }

    IEnumerable? ICheckedListBoxItem.Children => Children;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
