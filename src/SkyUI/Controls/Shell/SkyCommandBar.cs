using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>Primary app toolbar with title, command groups, and overflow menu.</summary>
public class SkyCommandBar : TemplatedControl
{
    public const string PrimaryItemsPartName = "PART_PrimaryItems";
    public const string SecondaryItemsPartName = "PART_SecondaryItems";
    public const string OverflowItemsPartName = "PART_OverflowItems";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyCommandBar, string?>(nameof(Title));

    public static readonly DirectProperty<SkyCommandBar, IList> PrimaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyCommandBar, IList>(
            nameof(PrimaryCommands),
            bar => bar.PrimaryCommands);

    public static readonly DirectProperty<SkyCommandBar, IList> SecondaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyCommandBar, IList>(
            nameof(SecondaryCommands),
            bar => bar.SecondaryCommands);

    public static readonly DirectProperty<SkyCommandBar, IList> OverflowCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyCommandBar, IList>(
            nameof(OverflowCommands),
            bar => bar.OverflowCommands);

    private readonly AvaloniaList<object> primaryCommands = new();
    private readonly AvaloniaList<object> secondaryCommands = new();
    private readonly AvaloniaList<object> overflowCommands = new();

    public SkyCommandBar()
    {
        Classes.Add("sky");
        Classes.Add("sky-command-bar");
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [Content]
    public IList PrimaryCommands => primaryCommands;

    public IList SecondaryCommands => secondaryCommands;

    public IList OverflowCommands => overflowCommands;
}
