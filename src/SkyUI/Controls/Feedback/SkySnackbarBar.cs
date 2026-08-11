using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Single snackbar surface (used by <see cref="SkySnackbarHost"/> item template).</summary>
public class SkySnackbarBar : TemplatedControl
{
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<SkySnackbarBar, string?>(nameof(Message));

    public static readonly StyledProperty<SkyFeedbackVariant> VariantProperty =
        AvaloniaProperty.Register<SkySnackbarBar, SkyFeedbackVariant>(nameof(Variant), SkyFeedbackVariant.Neutral);

    static SkySnackbarBar()
    {
        VariantProperty.Changed.AddClassHandler<SkySnackbarBar>((b, _) => b.SyncVariantClass());
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public SkyFeedbackVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncVariantClass();
    }

    private void SyncVariantClass()
    {
        foreach (var name in Enum.GetNames<SkyFeedbackVariant>())
            Classes.Remove($"sky-feedback-{name.ToLowerInvariant()}");

        Classes.Add($"sky-feedback-{Variant.ToString().ToLowerInvariant()}");
    }
}
