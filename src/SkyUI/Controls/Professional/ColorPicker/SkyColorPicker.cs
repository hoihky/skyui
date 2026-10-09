using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
namespace SkyUI.Controls.Professional;

public class SkyColorPicker : TemplatedControl
{
    public const string SpectrumPartName = "PART_Spectrum";
    public const string PreviewPartName = "PART_Preview";
    public const string HexBoxPartName = "PART_HexBox";

    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<SkyColorPicker, Color>(nameof(SelectedColor), Colors.DodgerBlue);

    public static readonly StyledProperty<bool> ShowHexInputProperty =
        AvaloniaProperty.Register<SkyColorPicker, bool>(nameof(ShowHexInput), true);

    public static readonly StyledProperty<bool> UseDropDownStyleProperty =
        AvaloniaProperty.Register<SkyColorPicker, bool>(nameof(UseDropDownStyle), true);

    public static readonly RoutedEvent<RoutedEventArgs> SelectedColorChangedEvent =
        RoutedEvent.Register<SkyColorPicker, RoutedEventArgs>(nameof(SelectedColorChanged), RoutingStrategies.Bubble);

    private Border? spectrum;
    private Border? preview;
    private TextBox? hexBox;

    static SkyColorPicker()
    {
        SelectedColorProperty.Changed.AddClassHandler<SkyColorPicker>((p, _) => p.SyncVisuals());
        UseDropDownStyleProperty.Changed.AddClassHandler<SkyColorPicker>((p, _) => p.UpdateDropDownChrome());
    }

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public bool ShowHexInput
    {
        get => GetValue(ShowHexInputProperty);
        set => SetValue(ShowHexInputProperty, value);
    }

    /// <summary>When true, shows a swatch that opens <see cref="SkyColorSelectionDialog"/> instead of an inline spectrum.</summary>
    public bool UseDropDownStyle
    {
        get => GetValue(UseDropDownStyleProperty);
        set => SetValue(UseDropDownStyleProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? SelectedColorChanged
    {
        add => AddHandler(SelectedColorChangedEvent, value);
        remove => RemoveHandler(SelectedColorChangedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        spectrum = e.NameScope.Find<Border>(SpectrumPartName);
        preview = e.NameScope.Find<Border>(PreviewPartName);
        hexBox = e.NameScope.Find<TextBox>(HexBoxPartName);
        if (spectrum is not null)
            spectrum.PointerPressed += OnSpectrumPressed;
        if (hexBox is not null)
            hexBox.LostFocus += OnHexLostFocus;
        if (preview is not null)
            preview.PointerPressed += OnOpenPicker;
        UpdateDropDownChrome();
        SyncVisuals();
    }

    private void UpdateDropDownChrome()
    {
        if (spectrum is not null)
        {
            spectrum.IsVisible = !UseDropDownStyle;
            spectrum.Height = UseDropDownStyle ? 0 : 96;
        }
    }

    private async void OnOpenPicker(object? sender, PointerPressedEventArgs e)
    {
        if (!UseDropDownStyle)
            return;
        e.Handled = true;
        await OpenDialogAsync();
    }

    private async Task OpenDialogAsync()
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is null)
            return;
        var result = await SkyColorSelectionDialog.ShowAsync(owner, SelectedColor);
        if (result is Color color)
        {
            SelectedColor = color;
            RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent));
        }
    }

    private void OnSpectrumPressed(object? sender, PointerPressedEventArgs e)
    {
        if (spectrum is null || UseDropDownStyle)
            return;
        var pos = e.GetPosition(spectrum);
        var w = Math.Max(1, spectrum.Bounds.Width);
        var h = Math.Max(1, spectrum.Bounds.Height);
        var hue = Math.Clamp(pos.X / w, 0, 1);
        var sat = Math.Clamp(1 - pos.Y / h, 0, 1);
        SelectedColor = ColorFromHsv(hue * 360, sat, 0.85);
        RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent));
    }

    private void OnHexLostFocus(object? sender, EventArgs e)
    {
        if (hexBox is null)
            return;
        if (TryParseHex(hexBox.Text, out var color))
        {
            SelectedColor = color;
            RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent));
        }
    }

    private void SyncVisuals()
    {
        var brush = new SolidColorBrush(SelectedColor);
        if (preview is not null)
            preview.Background = brush;
        if (hexBox is not null && !hexBox.IsFocused)
            hexBox.Text = SelectedColor.ToString();
    }

    internal static Color ColorFromHsv(double h, double s, double v)
    {
        var c = v * s;
        var x = c * (1 - Math.Abs((h / 60 % 2) - 1));
        var m = v - c;
        double r, g, b;
        if (h < 60) (r, g, b) = (c, x, 0);
        else if (h < 120) (r, g, b) = (x, c, 0);
        else if (h < 180) (r, g, b) = (0, c, x);
        else if (h < 240) (r, g, b) = (0, x, c);
        else if (h < 300) (r, g, b) = (x, 0, c);
        else (r, g, b) = (c, 0, x);
        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }

    internal static bool TryParseHex(string? text, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        try
        {
            color = Color.Parse(text.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
