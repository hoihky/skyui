using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Semi-transparent blocker shown over content during async work.</summary>
public class SkyLoadingOverlay : ContentControl
{
    public const string OverlayPartName = "PART_Overlay";

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<SkyLoadingOverlay, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string?> LoadingTextProperty =
        AvaloniaProperty.Register<SkyLoadingOverlay, string?>(nameof(LoadingText));

    public static readonly StyledProperty<bool> BlocksInputProperty =
        AvaloniaProperty.Register<SkyLoadingOverlay, bool>(nameof(BlocksInput), true);

    private Panel? overlay;

    static SkyLoadingOverlay()
    {
        IsLoadingProperty.Changed.AddClassHandler<SkyLoadingOverlay>((control, _) => control.SyncLoadingState());
        BlocksInputProperty.Changed.AddClassHandler<SkyLoadingOverlay>((control, _) => control.SyncLoadingState());
    }

    public SkyLoadingOverlay()
    {
        Classes.Add("sky");
        Classes.Add("sky-loading-overlay");
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? LoadingText
    {
        get => GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public bool BlocksInput
    {
        get => GetValue(BlocksInputProperty);
        set => SetValue(BlocksInputProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        overlay = e.NameScope.Find(OverlayPartName) as Panel;
        SyncLoadingState();
    }

    private void SyncLoadingState()
    {
        Classes.Set("sky-loading-overlay-active", IsLoading);

        if (overlay is null)
            return;

        overlay.IsVisible = IsLoading;
        overlay.IsHitTestVisible = IsLoading && BlocksInput;
    }
}
