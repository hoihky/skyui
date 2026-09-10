using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

/// <summary>Increases bottom padding when the on-screen keyboard occludes content.</summary>
public class SkyKeyboardInset : ContentControl
{
    public static readonly StyledProperty<double> ExtraBottomPaddingProperty =
        AvaloniaProperty.Register<SkyKeyboardInset, double>(nameof(ExtraBottomPadding), 8);

    private IInputPane? inputPane;
    private Thickness basePadding;

    public SkyKeyboardInset()
    {
        Classes.Add("sky");
        Classes.Add("sky-keyboard-inset");
    }

    public double ExtraBottomPadding
    {
        get => GetValue(ExtraBottomPaddingProperty);
        set => SetValue(ExtraBottomPaddingProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        basePadding = Padding;
        DetachInputPane();
        inputPane = TopLevel.GetTopLevel(this)?.InputPane;
        if (inputPane is null)
            return;

        inputPane.StateChanged += OnInputPaneStateChanged;
        ApplyInputPaneInset(inputPane);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachInputPane();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachInputPane()
    {
        if (inputPane is null)
            return;

        inputPane.StateChanged -= OnInputPaneStateChanged;
        inputPane = null;
    }

    private void OnInputPaneStateChanged(object? sender, InputPaneStateEventArgs e) =>
        ApplyInputPaneInset(inputPane);

    private void ApplyInputPaneInset(IInputPane? pane)
    {
        var keyboardInset = 0d;
        if (pane is { State: InputPaneState.Open })
            keyboardInset = Math.Max(0, pane.OccludedRect.Height);

        Padding = new Thickness(
            basePadding.Left,
            basePadding.Top,
            basePadding.Right,
            basePadding.Bottom + keyboardInset + ExtraBottomPadding);
    }
}
