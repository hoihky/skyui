using System.Windows.Input;

namespace SkyUI.Controls;

/// <summary>Action shown in a row overflow menu.</summary>
public sealed class CheckedListRowAction
{
    public required string Label { get; init; }

    public required ICommand Command { get; init; }

    public object? CommandParameter { get; init; }
}
