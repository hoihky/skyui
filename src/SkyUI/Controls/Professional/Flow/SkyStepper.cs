using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public class SkyStepper : TemplatedControl
{
    public const string StepsHostPartName = "PART_StepsHost";

    public static readonly StyledProperty<IEnumerable?> StepsProperty =
        AvaloniaProperty.Register<SkyStepper, IEnumerable?>(nameof(Steps));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SkyStepper, int>(nameof(SelectedIndex));

    public static readonly StyledProperty<SkyStepperChrome> ChromeProperty =
        AvaloniaProperty.Register<SkyStepper, SkyStepperChrome>(nameof(Chrome), new SkyStepperChrome());

    private Panel? stepsHost;
    private INotifyCollectionChanged? subscribed;

    public IEnumerable? Steps
    {
        get => GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public SkyStepperChrome Chrome
    {
        get => GetValue(ChromeProperty);
        set => SetValue(ChromeProperty, value);
    }

    static SkyStepper()
    {
        StepsProperty.Changed.AddClassHandler<SkyStepper>((s, _) => s.Rebuild());
        SelectedIndexProperty.Changed.AddClassHandler<SkyStepper>((s, _) => s.Rebuild());
        ChromeProperty.Changed.AddClassHandler<SkyStepper>((s, _) => s.Rebuild());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        stepsHost = e.NameScope.Find<Panel>(StepsHostPartName);
        Rebuild();
    }

    private void Rebuild()
    {
        if (stepsHost is null)
            return;
        Unsubscribe();
        stepsHost.Children.Clear();
        var chrome = Chrome ?? new SkyStepperChrome();
        var steps = Steps?.OfType<SkyStepperItem>().ToList() ?? [];
        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var index = i;
            var isActive = index == SelectedIndex;
            var isComplete = step.IsComplete || index < SelectedIndex;
            stepsHost.Children.Add(CreateStepVisual(this, chrome, step, index + 1, isActive, isComplete, () =>
            {
                SetCurrentValue(SelectedIndexProperty, index);
            }));
            if (i < steps.Count - 1)
                stepsHost.Children.Add(CreateConnector(chrome, isComplete));
        }

        if (Steps is INotifyCollectionChanged notify)
        {
            subscribed = notify;
            subscribed.CollectionChanged += OnStepsChanged;
        }
    }

    private void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Unsubscribe()
    {
        if (subscribed is null)
            return;
        subscribed.CollectionChanged -= OnStepsChanged;
        subscribed = null;
    }

    private static Control CreateConnector(SkyStepperChrome chrome, bool complete)
    {
        var brush = complete
            ? chrome.CompleteConnectorBrush ?? new SolidColorBrush(Color.Parse("#38BDF8"))
            : chrome.PendingConnectorBrush ?? new SolidColorBrush(Color.Parse("#4B5563"));
        var y = chrome.NodeDiameter / 2 - chrome.ConnectorThickness / 2;
        return new Border
        {
            Width = chrome.ConnectorLength,
            Height = chrome.ConnectorThickness,
            Margin = new Thickness(4, y, 4, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = brush,
        };
    }

    private IBrush ThemeBrush(string key, IBrush fallback) =>
        TryGetResource(key, ActualThemeVariant, out var resource) && resource is IBrush brush
            ? brush
            : fallback;

    private bool ShouldUseDarkStepLabelText()
    {
        if (TryGetThemeColor("SkyPaletteCard", out var card))
            return GetRelativeLuminance(card) > 0.55;
        if (TryGetThemeColor("SkyPaletteSurface", out var surface))
            return GetRelativeLuminance(surface) > 0.55;
        if (TryGetThemeBrushColor("SkyCardBrush", out var cardBrush))
            return GetRelativeLuminance(cardBrush) > 0.55;
        if (TryGetThemeBrushColor("SkyBackgroundBrush", out var background))
            return GetRelativeLuminance(background) > 0.55;
        if (TryGetThemeBrushColor("SkyTextPrimaryBrush", out var text))
            return GetRelativeLuminance(text) < 0.45;
        return true;
    }

    private bool TryGetThemeColor(string key, out Color color)
    {
        if (TryGetResource(key, ActualThemeVariant, out var resource) && resource is Color c)
        {
            color = c;
            return true;
        }

        color = default;
        return false;
    }

    private bool TryGetThemeBrushColor(string key, out Color color)
    {
        if (!TryGetResource(key, ActualThemeVariant, out var resource))
        {
            color = default;
            return false;
        }

        if (resource is ISolidColorBrush solid)
        {
            color = solid.Color;
            return true;
        }

        if (resource is SolidColorBrush brush)
        {
            color = brush.Color;
            return true;
        }

        color = default;
        return false;
    }

    private static double GetRelativeLuminance(Color color)
    {
        static double Channel(byte c) => c / 255d;
        var r = Channel(color.R);
        var g = Channel(color.G);
        var b = Channel(color.B);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private (IBrush Title, IBrush SubtitleActive, IBrush TitleMuted, IBrush SubtitleInactive) GetStepLabelBrushes()
    {
        if (ShouldUseDarkStepLabelText())
        {
            return (
                new SolidColorBrush(Color.Parse("#130F26")),
                new SolidColorBrush(Color.Parse("#5B5475")),
                new SolidColorBrush(Color.Parse("#3F3958")),
                new SolidColorBrush(Color.Parse("#2F2A45")));
        }

        return (
            ThemeBrush("SkyTextPrimaryBrush", new SolidColorBrush(Color.Parse("#FAFAFF"))),
            ThemeBrush("SkyTextSecondaryBrightBrush", new SolidColorBrush(Color.Parse("#BEB8D4"))),
            ThemeBrush("SkyTextSecondaryBrightBrush", new SolidColorBrush(Color.Parse("#BEB8D4"))),
            ThemeBrush("SkyTextSecondaryBrightBrush", new SolidColorBrush(Color.Parse("#D4D0E4"))));
    }

    private Control CreateStepVisual(
        SkyStepper owner,
        SkyStepperChrome chrome,
        SkyStepperItem step,
        int number,
        bool isActive,
        bool isComplete,
        Action onSelect)
    {
        var accent = Color.Parse("#38BDF8");
        var borderBrush = isActive
            ? chrome.ActiveNodeBorderBrush ?? new SolidColorBrush(accent)
            : isComplete
                ? chrome.CompleteNodeBorderBrush ?? new SolidColorBrush(accent)
                : chrome.PendingNodeBorderBrush ?? new SolidColorBrush(Color.Parse("#6B7280"));
        var fill = isActive
            ? chrome.ActiveNodeBackgroundBrush ?? new SolidColorBrush(accent)
            : isComplete
                ? chrome.CompleteNodeBackgroundBrush ?? new SolidColorBrush(Color.Parse("#0E7490"))
                : chrome.PendingNodeBackgroundBrush ?? Brushes.Transparent;
        IBrush nodeForeground = isActive || isComplete
            ? Brushes.White
            : new SolidColorBrush(Color.Parse("#9CA3AF"));
        var (titlePrimary, subtitleActive, titleMuted, subtitleInactive) = owner.GetStepLabelBrushes();
        var accentText = owner.ThemeBrush("SkyAccentBrush", new SolidColorBrush(accent));
        var titleBrush = isActive
            ? chrome.ActiveTitleForeground ?? accentText
            : isComplete
                ? chrome.CompleteTitleForeground ?? titlePrimary
                : chrome.PendingTitleForeground ?? titleMuted;
        var subtitleBrush = isActive
            ? chrome.ActiveSubtitleForeground ?? subtitleActive
            : isComplete
                ? chrome.CompleteSubtitleForeground ?? subtitleInactive
                : chrome.PendingSubtitleForeground ?? subtitleInactive;

        var circle = new Border
        {
            Width = chrome.NodeDiameter,
            Height = chrome.NodeDiameter,
            CornerRadius = new CornerRadius(chrome.NodeDiameter / 2),
            BorderThickness = new Thickness(2),
            BorderBrush = borderBrush,
            Background = fill,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new TextBlock
            {
                Text = isComplete ? "✓" : number.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = isComplete ? 13 : 12,
                FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal,
                Foreground = nodeForeground,
            },
        };
        var title = new TextBlock
        {
            Text = step.Title,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = titleBrush,
            FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal,
            FontSize = 13,
        };
        var subtitle = new TextBlock
        {
            Text = step.Subtitle ?? (isComplete ? "Completed" : "Pending"),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Foreground = subtitleBrush,
        };
        var labels = new StackPanel { Spacing = 2 };
        labels.Children.Add(title);
        labels.Children.Add(subtitle);
        var root = new StackPanel
        {
            Spacing = 8,
            Width = chrome.StepColumnWidth,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };
        root.Children.Add(circle);
        root.Children.Add(labels);
        root.PointerPressed += (_, e) =>
        {
            onSelect();
            e.Handled = true;
        };
        return root;
    }
}
