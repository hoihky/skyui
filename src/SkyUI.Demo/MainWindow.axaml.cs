using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Demo.Models;
using SkyUI.Demo.Views.Demos;
using SkyUI.Icons;
using SkyUI.Themes.Sky;

namespace SkyUI.Demo;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _detailCache = new();

    public MainWindow()
    {
        InitializeComponent();

        ThemeCombo.SelectionChanged += (_, _) => ApplyThemeVariant();
        DensityCombo.SelectionChanged += (_, _) => ApplyDensity();
        AccentHexBox.LostFocus += (_, _) => TryApplyAccentFromHex();
        AccentHexBox.KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                TryApplyAccentFromHex();
        };

        if (Application.Current is { } app)
        {
            SyncThemeComboFromApp(app);
            SyncDensityFromApp(app);
            SyncAccentFromApp(app);
        }

        NavList.ItemsSource = new DemoItem[]
        {
            new("Overview", () => new OverviewDemo(), SkyIconKind.Home),
            new("Typography", () => new TypographyDemo(), SkyIconKind.List),
            new("Icons", () => new IconsDemo(), SkyIconKind.Layers),
            new("Buttons", () => new ButtonsDemo(), SkyIconKind.Add),
            new("Avatar", () => new AvatarDemo(), SkyIconKind.Image),
            new("Chip", () => new ChipDemo(), SkyIconKind.Sliders),
            new("Badge", () => new BadgeDemo(), SkyIconKind.Check),
            new("Feedback", () => new FeedbackDemo(), SkyIconKind.Layers),
            new("Navigation", () => new NavigationDemo(), SkyIconKind.LayoutGrid),
            new("Forms", () => new FormsDemo(), SkyIconKind.Sliders),
            new("Menus", () => new MenuDemo(), SkyIconKind.List),
            new("Pickers", () => new PickersDemo(), SkyIconKind.LayoutGrid),
            new("Layout", () => new LayoutDemo(), SkyIconKind.LayoutGrid),
            new("Primitives", () => new PrimitivesDemo(), SkyIconKind.Layers),
            new("Text field", () => new TextFieldDemo(), SkyIconKind.Search),
            new("Checkbox & Switch", () => new CheckboxSwitchDemo(), SkyIconKind.Check),
            new("Select", () => new SelectDemo(), SkyIconKind.ChevronDown),
            new("List", () => new ListDemo(), SkyIconKind.List),
            new("Accordion", () => new AccordionDemo(), SkyIconKind.Layers),
            new("Placeholder", () => new PlaceholderDemo(), SkyIconKind.LayoutGrid),
            new("Diagram", () => new DiagramDemo(), SkyIconKind.Layers),
            new("CheckedListBox", () => new CheckedListBoxDemo(), SkyIconKind.Check),
            new("Filter editor", () => new FilterEditorDemo(), SkyIconKind.Filter),
            new("Virtual DataGrid", () => new VirtualDataGridDemo(), SkyIconKind.Table),
            new("Video timeline", () => new VideoTimelineDemo(), SkyIconKind.Video),
        };

        NavList.SelectionChanged += (_, _) => ApplySelection();
        NavList.SelectedIndex = 0;
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (NavList.SelectedItem is not DemoItem item)
            return;

        if (!_detailCache.TryGetValue(item.Title, out var view))
        {
            view = item.CreateView();
            _detailCache[item.Title] = view;
        }

        DetailHost.Content = view;
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
