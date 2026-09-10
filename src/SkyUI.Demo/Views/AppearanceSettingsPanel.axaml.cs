using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Themes.Sky;

namespace SkyUI.Demo.Views;

public partial class AppearanceSettingsPanel : UserControl
{
    public AppearanceSettingsPanel()
    {
        InitializeComponent();

        ThemeCombo.SelectionChanged += (_, _) => ApplyThemeVariant();
        DensityCombo.SelectionChanged += (_, _) => ApplyDensity();
        AccentHexBox.LostFocus += (_, _) => TryApplyAccentFromHex();
        AccentHexBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
                TryApplyAccentFromHex();
        };

        if (Application.Current is { } app)
        {
            SyncThemeComboFromApp(app);
            SyncDensityFromApp(app);
            SyncAccentFromApp(app);
        }
    }

    private void ApplyThemeVariant()
    {
        if (ThemeCombo.SelectedItem is not ComboBoxItem { Tag: string tag })
            return;
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = tag switch
        {
            "Light" => ThemeVariant.Light,
            "HighContrast" => SkyThemeVariants.HighContrast,
            _ => ThemeVariant.Dark,
        };
    }

    private void SyncThemeComboFromApp(Application app)
    {
        var index = app.RequestedThemeVariant switch
        {
            { } v when v == ThemeVariant.Light => 1,
            { } v when v == SkyThemeVariants.HighContrast => 2,
            _ => 0,
        };
        ThemeCombo.SelectedIndex = index;
    }

    private void ApplyDensity()
    {
        if (DensityCombo.SelectedItem is not ComboBoxItem { Tag: string tag })
            return;
        if (Application.Current is not Application app)
            return;

        var density = tag == "Compact" ? SkyDensity.Compact : SkyDensity.Comfortable;
        SkyThemeProperties.SetDensity(app, density);
    }

    private void SyncDensityFromApp(Application app)
    {
        DensityCombo.SelectedIndex = SkyThemeProperties.GetDensity(app) == SkyDensity.Compact ? 1 : 0;
    }

    private void SyncAccentFromApp(Application app)
    {
        if (SkyThemeProperties.GetAccentOverride(app) is { } accent)
            SetAccentUi(accent, applyOverride: false);
        else
            ClearAccentOverrideUi();
    }

    private void TryApplyAccentFromHex()
    {
        var text = AccentHexBox.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            ClearAccentOverrideUi();
            return;
        }

        if (!TryParseHexColor(text, out var color))
            return;

        SetAccentUi(color, applyOverride: true);
    }

    private void OnAccentSwatchClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string hex })
            return;
        if (!TryParseHexColor(hex, out var color))
            return;

        SetAccentUi(color, applyOverride: true);
    }

    private void OnAccentResetClick(object? sender, RoutedEventArgs e) =>
        ClearAccentOverrideUi();

    private void SetAccentUi(Color color, bool applyOverride)
    {
        AccentHexBox.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        AccentPreview.Background = new SolidColorBrush(color);

        if (applyOverride && Application.Current is Application app)
            SkyThemeProperties.SetAccentOverride(app, color);
    }

    private void ClearAccentOverrideUi()
    {
        if (Application.Current is Application app)
            SkyThemeProperties.SetAccentOverride(app, null);

        AccentHexBox.Text = string.Empty;
        if (Application.Current?.TryGetResource("SkyAccentBrush", out var brush) == true && brush is IBrush b)
            AccentPreview.Background = b;
        else
            AccentPreview.ClearValue(Border.BackgroundProperty);
    }

    private static bool TryParseHexColor(string text, out Color color)
    {
        color = default;
        var s = text.Trim();
        if (!s.StartsWith('#'))
            s = "#" + s;

        try
        {
            color = Color.Parse(s);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
