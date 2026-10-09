using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls.Professional;

public class SkyWizard : TemplatedControl
{
    public const string PageHostPartName = "PART_PageHost";
    public const string BackButtonPartName = "PART_Back";
    public const string NextButtonPartName = "PART_Next";

    public static readonly StyledProperty<IEnumerable?> PagesProperty =
        AvaloniaProperty.Register<SkyWizard, IEnumerable?>(nameof(Pages));

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<SkyWizard, int>(nameof(CurrentIndex));

    public static readonly RoutedEvent<RoutedEventArgs> CompletedEvent =
        RoutedEvent.Register<SkyWizard, RoutedEventArgs>(nameof(Completed), RoutingStrategies.Bubble);

    private ContentPresenter? pageHost;
    private Button? backButton;
    private Button? nextButton;
    private INotifyCollectionChanged? subscribed;

    public IEnumerable? Pages
    {
        get => GetValue(PagesProperty);
        set => SetValue(PagesProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    static SkyWizard()
    {
        PagesProperty.Changed.AddClassHandler<SkyWizard>((w, _) => w.SyncPage());
        CurrentIndexProperty.Changed.AddClassHandler<SkyWizard>((w, _) => w.SyncPage());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        pageHost = e.NameScope.Find<ContentPresenter>(PageHostPartName);
        backButton = e.NameScope.Find<Button>(BackButtonPartName);
        nextButton = e.NameScope.Find<Button>(NextButtonPartName);
        if (backButton is not null)
            backButton.Click += (_, _) => GoBack();
        if (nextButton is not null)
            nextButton.Click += (_, _) => GoNext();
        if (Pages is INotifyCollectionChanged notify)
        {
            subscribed = notify;
            subscribed.CollectionChanged += (_, _) => SyncPage();
        }

        SyncPage();
    }

    public void GoBack()
    {
        if (CurrentIndex > 0)
            CurrentIndex--;
    }

    public void GoNext()
    {
        var pages = Pages?.OfType<SkyWizardPage>().ToList() ?? [];
        if (pages.Count == 0)
            return;
        var current = pages[CurrentIndex];
        if (!current.CanProceed)
            return;
        if (CurrentIndex >= pages.Count - 1)
            RaiseEvent(new RoutedEventArgs(CompletedEvent));
        else
            CurrentIndex++;
    }

    private void SyncPage()
    {
        var pages = Pages?.OfType<SkyWizardPage>().ToList() ?? [];
        if (pageHost is not null)
            pageHost.Content = CurrentIndex >= 0 && CurrentIndex < pages.Count
                ? pages[CurrentIndex].Content
                : null;
        if (backButton is not null)
            backButton.IsEnabled = CurrentIndex > 0;
        if (nextButton is not null)
            nextButton.Content = CurrentIndex >= pages.Count - 1 ? "Finish" : "Next";
    }
}
