namespace SkyUI.Controls;

/// <summary>One row in a <see cref="SkyActionSheet"/>.</summary>
public sealed class SkyActionSheetItem
{
    public required string Title { get; init; }

    public bool IsDestructive { get; init; }

    public bool IsCancel { get; init; }
}
