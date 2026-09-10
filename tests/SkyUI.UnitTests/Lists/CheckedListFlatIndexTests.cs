using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CheckedListFlatIndexTests
{
    [Fact]
    public void Flatten_respects_collapsed_nodes()
    {
        var child = new TestTreeNode("Child", new ObservableCollection<TestTreeNode> { new("Grandchild") })
        {
            IsExpanded = false,
        };
        var root = new TestTreeNode("Root", new ObservableCollection<TestTreeNode> { child });

        var rows = new List<CheckedListRowModel>();
        Action requestRebuild = () => { };
        Action<CheckedListRowModel, bool?> checkCommitted = (_, _) => { };
        Action<CheckedListRowModel, PointerReleasedEventArgs> pointerPressed = (_, _) => { };
        CheckedListFlatIndex.AppendVisibleRows(
            rows,
            new[] { root },
            new DefaultCheckedListItemAdapter(),
            comparer: null,
            requestRebuild,
            checkCommitted,
            pointerPressed);

        Assert.Equal(2, rows.Count);
        Assert.DoesNotContain(rows, r => (r.Item as TestTreeNode)?.Title == "Grandchild");
    }

    [Fact]
    public void Flatten_includes_expanded_descendants()
    {
        var root = new TestTreeNode("Root", new ObservableCollection<TestTreeNode>
        {
            new("Child", new ObservableCollection<TestTreeNode> { new("Grandchild") }),
        });

        var rows = new List<CheckedListRowModel>();
        Action requestRebuild = () => { };
        Action<CheckedListRowModel, bool?> checkCommitted = (_, _) => { };
        Action<CheckedListRowModel, PointerReleasedEventArgs> pointerPressed = (_, _) => { };
        CheckedListFlatIndex.AppendVisibleRows(
            rows,
            new[] { root },
            new DefaultCheckedListItemAdapter(),
            comparer: null,
            requestRebuild,
            checkCommitted,
            pointerPressed);

        Assert.Equal(3, rows.Count);
    }

    private sealed class TestTreeNode : ICheckedListBoxItem
    {
        private bool? _isChecked;
        private bool _isExpanded = true;

        public TestTreeNode(string title, ObservableCollection<TestTreeNode>? children = null)
        {
            Title = title;
            Children = children;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title { get; }

        public ObservableCollection<TestTreeNode>? Children { get; }

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
}
