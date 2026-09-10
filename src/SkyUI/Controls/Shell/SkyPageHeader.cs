using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Page title row with optional back navigation and action slots.</summary>
public class SkyPageHeader : TemplatedControl
{
    public const string BackButtonPartName = "PART_BackButton";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyPageHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SkyPageHeader, string?>(nameof(Subtitle));

    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<SkyPageHeader, bool>(nameof(IsBackButtonVisible));

    public static readonly StyledProperty<ICommand?> BackCommandProperty =
        AvaloniaProperty.Register<SkyPageHeader, ICommand?>(nameof(BackCommand));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<SkyPageHeader, object?>(nameof(ActionContent));

    public static readonly RoutedEvent<RoutedEventArgs> BackRequestedEvent =
        RoutedEvent.Register<SkyPageHeader, RoutedEventArgs>(nameof(BackRequested), RoutingStrategies.Bubble);

    private Button? backButton;

    public SkyPageHeader()
    {
        Classes.Add("sky");
        Classes.Add("sky-page-header");
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool IsBackButtonVisible
    {
        get => GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public ICommand? BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? BackRequested
    {
        add => AddHandler(BackRequestedEvent, value);
        remove => RemoveHandler(BackRequestedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (backButton is not null)
            backButton.Click -= OnBackClick;

        backButton = e.NameScope.Find(BackButtonPartName) as Button;
        if (backButton is not null)
            backButton.Click += OnBackClick;
    }

    private void OnBackClick(object? sender, RoutedEventArgs e)
    {
        if (BackCommand?.CanExecute(null) == true)
            BackCommand.Execute(null);

        RaiseEvent(new RoutedEventArgs(BackRequestedEvent));
    }
}
