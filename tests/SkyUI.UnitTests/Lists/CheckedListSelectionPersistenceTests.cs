using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CheckedListSelectionPersistenceTests
{
    [Fact]
    public void RebuildAll_preserves_selection_across_rebuild()
    {
        var paris = new TestNode("Paris");
        var berlin = new TestNode("Berlin");
        var europe = new TestNode("Europe", new ObservableCollection<TestNode> { paris, berlin });
        var roots = new ObservableCollection<TestNode> { europe };
        var list = new CheckedListBox
        {
            ItemsSource = roots,
            SelectionMode = CheckedListBoxSelectionMode.Single,
        };

        list.RebuildAll();
        list.Rows[1].IsSelected = true;

        list.RebuildAll();

        var parisRow = list.Rows.First(r => ReferenceEquals(r.Item, paris));
        Assert.True(parisRow.IsSelected);
    }

    private sealed class TestNode : ICheckedListBoxItem
    {
        public TestNode(string title, ObservableCollection<TestNode>? children = null)
        {
            Title = title;
            Children = children ?? new ObservableCollection<TestNode>();
        }

        public string Title { get; set; } = string.Empty;

        public ObservableCollection<TestNode> Children { get; }

        public bool? IsChecked { get; set; }

        public bool IsExpanded { get; set; } = true;

        IEnumerable? ICheckedListBoxItem.Children => Children;

#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    }
}
