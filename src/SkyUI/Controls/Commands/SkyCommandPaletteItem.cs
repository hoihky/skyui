using System.Windows.Input;
using Avalonia;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Executable entry displayed inside <see cref="SkyCommandPalette"/>.</summary>
public sealed class SkyCommandPaletteItem : AvaloniaObject
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, string?>(nameof(Subtitle));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, ICommand?>(nameof(Command));

    public static readonly StyledProperty<SkyIconKind?> IconKindProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, SkyIconKind?>(nameof(IconKind));

    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, bool>(nameof(IsEnabled), true);

    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, bool>(nameof(IsVisible), true);

    public static readonly StyledProperty<string?> KeywordsProperty =
        AvaloniaProperty.Register<SkyCommandPaletteItem, string?>(nameof(Keywords));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public SkyIconKind? IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>Space-separated search hints (e.g. "save disk file").</summary>
    public string? Keywords
    {
        get => GetValue(KeywordsProperty);
        set => SetValue(KeywordsProperty, value);
    }
}
