using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkyUI.Controls;

namespace SkyUI.Demo.Models;

/// <summary>Sample hierarchy item for <see cref="Views.Demos.CheckedListBoxDemo"/> (implements SkyUI <see cref="ICheckedListBoxItem"/>).</summary>
public sealed class DemoCheckedListNode : ICheckedListBoxItem
{
    private bool? _isChecked;
    private bool _isExpanded = true;

    public DemoCheckedListNode(string title, ObservableCollection<DemoCheckedListNode>? children = null)
    {
        Title = title;
        Children = children;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; }

    public ObservableCollection<DemoCheckedListNode>? Children { get; }

    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
                return;
            _isChecked = value;
            OnPropertyChanged();
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    IEnumerable? ICheckedListBoxItem.Children => Children;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
