using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Bottom status strip for connection state, selection counts, or progress.</summary>
public class SkyStatusBar : TemplatedControl
{
    public const string LeftContentPartName = "PART_LeftContent";
    public const string RightContentPartName = "PART_RightContent";
    public const string ProgressPartName = "PART_Progress";

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SkyStatusBar, string?>(nameof(Text));

    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<SkyStatusBar, object?>(nameof(LeftContent));

    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<SkyStatusBar, object?>(nameof(RightContent));

    public static readonly StyledProperty<double?> ProgressProperty =
        AvaloniaProperty.Register<SkyStatusBar, double?>(nameof(Progress));

    public SkyStatusBar()
    {
        Classes.Add("sky");
        Classes.Add("sky-status-bar");
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>Optional progress value from 0 to 100.</summary>
    public double? Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}
