using System.Collections.ObjectModel;
using SkyUI.Demo.Models;

namespace SkyUI.Demo.Models;

/// <summary>Shared sample hierarchy for list/tree demos.</summary>
public static class DemoTreeData
{
    public static ObservableCollection<DemoCheckedListNode> CreateFileExplorerTree() =>
    [
        new("Documents", new ObservableCollection<DemoCheckedListNode>
        {
            new("Reports", new ObservableCollection<DemoCheckedListNode>
            {
                new("Q1-summary.pdf"),
                new("Q2-summary.pdf"),
            }),
            new("Notes", new ObservableCollection<DemoCheckedListNode>
            {
                new("meeting-notes.md"),
            }),
        }),
        new("Media", new ObservableCollection<DemoCheckedListNode>
        {
            new("Images", new ObservableCollection<DemoCheckedListNode>
            {
                new("cover.png"),
                new("avatar.jpg"),
            }),
            new("Videos", new ObservableCollection<DemoCheckedListNode>
            {
                new("intro.mp4"),
            }),
        }),
        new("Archive"),
    ];
}
