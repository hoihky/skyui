using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>Text input with phone, credit-card, or custom display masks.</summary>
public class SkyMaskedTextBox : TextBox
{
    public static readonly StyledProperty<SkyInputMaskKind> MaskKindProperty =
        AvaloniaProperty.Register<SkyMaskedTextBox, SkyInputMaskKind>(nameof(MaskKind));

    public static readonly StyledProperty<string?> MaskProperty =
        AvaloniaProperty.Register<SkyMaskedTextBox, string?>(nameof(Mask));

    public static readonly StyledProperty<string> RawTextProperty =
        AvaloniaProperty.Register<SkyMaskedTextBox, string>(
            nameof(RawText),
            string.Empty,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    private bool isInternalUpdate;

    static SkyMaskedTextBox()
    {
        MaskKindProperty.Changed.AddClassHandler<SkyMaskedTextBox>((box, _) => box.ApplyMask());
        MaskProperty.Changed.AddClassHandler<SkyMaskedTextBox>((box, _) => box.ApplyMask());
        RawTextProperty.Changed.AddClassHandler<SkyMaskedTextBox>((box, e) =>
        {
            if (!box.isInternalUpdate)
                box.UpdateDisplayFromRaw(e.GetNewValue<string>());
        });
    }

    public SkyMaskedTextBox()
    {
        Classes.Add("sky");
        Classes.Add("sky-masked-text");
        TextChanged += OnTextChanged;
    }

    public SkyInputMaskKind MaskKind
    {
        get => GetValue(MaskKindProperty);
        set => SetValue(MaskKindProperty, value);
    }

    public string? Mask
    {
        get => GetValue(MaskProperty);
        set => SetValue(MaskProperty, value);
    }

    public string RawText
    {
        get => GetValue(RawTextProperty);
        set => SetValue(RawTextProperty, value);
    }

    public bool IsMaskComplete =>
        SkyInputMask.IsComplete(ResolvedMask, RawText);

    private string ResolvedMask =>
        SkyInputMask.ResolveMask(MaskKind, Mask);

    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(ResolvedMask))
        {
            base.OnTextInput(e);
            return;
        }

        if (string.IsNullOrEmpty(e.Text))
        {
            base.OnTextInput(e);
            return;
        }

        foreach (var character in e.Text)
        {
            if (!char.IsDigit(character) && MaskKind != SkyInputMaskKind.Custom)
                continue;

            if (MaskKind == SkyInputMaskKind.Custom &&
                !SkyInputMask.MatchesPlaceholder(SkyInputMask.AnyPlaceholder, character) &&
                !char.IsLetterOrDigit(character))
                continue;

            AppendRaw(character.ToString());
        }

        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Back && !string.IsNullOrEmpty(RawText))
        {
            SetRawText(RawText[..^1]);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete && !string.IsNullOrEmpty(RawText))
        {
            SetRawText(RawText[..^1]);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (isInternalUpdate || string.IsNullOrEmpty(ResolvedMask))
            return;

        var extracted = SkyInputMask.ExtractRaw(ResolvedMask, Text);
        if (extracted != RawText)
            SetRawText(extracted);
    }

    private void ApplyMask() => UpdateDisplayFromRaw(RawText);

    private void AppendRaw(string characters)
    {
        var mask = ResolvedMask;
        var next = new System.Text.StringBuilder(RawText);
        foreach (var character in characters)
        {
            if (next.Length >= mask.Count(SkyInputMask.IsPlaceholder))
                break;

            if (MaskKind == SkyInputMaskKind.Custom)
            {
                var placeholder = mask.Skip(next.Length).FirstOrDefault(SkyInputMask.IsPlaceholder);
                if (placeholder != default && SkyInputMask.MatchesPlaceholder(placeholder, character))
                    next.Append(character);
            }
            else if (char.IsDigit(character))
            {
                next.Append(character);
            }
        }

        SetRawText(next.ToString());
    }

    private void SetRawText(string value)
    {
        isInternalUpdate = true;
        try
        {
            RawText = value;
            UpdateDisplayFromRaw(value);
        }
        finally
        {
            isInternalUpdate = false;
        }
    }

    private void UpdateDisplayFromRaw(string rawValue)
    {
        isInternalUpdate = true;
        try
        {
            Text = SkyInputMask.Format(ResolvedMask, rawValue);
        }
        finally
        {
            isInternalUpdate = false;
        }
    }
}
