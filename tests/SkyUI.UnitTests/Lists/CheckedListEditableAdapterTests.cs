using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CheckedListEditableAdapterTests
{
    [Fact]
    public void Inline_adapter_commits_and_rejects_blank_values()
    {
        var adapter = new TestEditableAdapter();
        var item = new TestEditableItem("Paris");

        Assert.True(adapter.TryCommitEdit(item, "Lyon", out _));
        Assert.Equal("Lyon", item.Title);
        Assert.False(adapter.TryCommitEdit(item, " ", out var error));
        Assert.False(string.IsNullOrEmpty(error));
    }

    private sealed class TestEditableItem(string title)
    {
        public string Title { get; set; } = title;
    }

    private sealed class TestEditableAdapter : ICheckedListEditableAdapter
    {
        public bool CanEdit(object? item) => item is TestEditableItem;

        public string GetEditText(object? item) =>
            item is TestEditableItem editable ? editable.Title : string.Empty;

        public bool TryCommitEdit(object? item, string text, out string? error)
        {
            if (item is not TestEditableItem editable)
            {
                error = "Not editable";
                return false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Required";
                return false;
            }

            editable.Title = text.Trim();
            error = null;
            return true;
        }
    }
}
