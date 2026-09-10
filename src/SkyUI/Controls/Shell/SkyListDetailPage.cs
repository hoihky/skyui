using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>List-detail app scaffold: header, command bar, split view, status bar, and filter drawer.</summary>
public class SkyListDetailPage : TemplatedControl
{
    public const string HeaderPartName = "PART_Header";
    public const string CommandBarPartName = "PART_CommandBar";
    public const string SplitViewPartName = "PART_SplitView";
    public const string StatusBarPartName = "PART_StatusBar";
    public const string DrawerPartName = "PART_Drawer";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyListDetailPage, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SkyListDetailPage, string?>(nameof(Subtitle));

    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<SkyListDetailPage, bool>(nameof(IsBackButtonVisible));

    public static readonly StyledProperty<ICommand?> BackCommandProperty =
        AvaloniaProperty.Register<SkyListDetailPage, ICommand?>(nameof(BackCommand));

    public static readonly StyledProperty<object?> HeaderActionContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(HeaderActionContent));

    public static readonly StyledProperty<string?> CommandBarTitleProperty =
        AvaloniaProperty.Register<SkyListDetailPage, string?>(nameof(CommandBarTitle));

    public static readonly DirectProperty<SkyListDetailPage, IList> PrimaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyListDetailPage, IList>(
            nameof(PrimaryCommands),
            page => page.PrimaryCommands);

    public static readonly DirectProperty<SkyListDetailPage, IList> SecondaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyListDetailPage, IList>(
            nameof(SecondaryCommands),
            page => page.SecondaryCommands);

    public static readonly DirectProperty<SkyListDetailPage, IList> OverflowCommandsProperty =
        AvaloniaProperty.RegisterDirect<SkyListDetailPage, IList>(
            nameof(OverflowCommands),
            page => page.OverflowCommands);

    public static readonly StyledProperty<object?> PaneContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(PaneContent));

    public static readonly StyledProperty<object?> DetailContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(DetailContent));

    public static readonly StyledProperty<bool> IsPaneOpenProperty =
        AvaloniaProperty.Register<SkyListDetailPage, bool>(nameof(IsPaneOpen), true);

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<SkyListDetailPage, double>(nameof(OpenPaneLength), 280);

    public static readonly StyledProperty<SkySplitViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<SkyListDetailPage, SkySplitViewDisplayMode>(
            nameof(DisplayMode),
            SkySplitViewDisplayMode.Inline);

    public static readonly StyledProperty<string?> StatusTextProperty =
        AvaloniaProperty.Register<SkyListDetailPage, string?>(nameof(StatusText));

    public static readonly StyledProperty<object?> StatusLeftContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(StatusLeftContent));

    public static readonly StyledProperty<object?> StatusRightContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(StatusRightContent));

    public static readonly StyledProperty<double?> ProgressProperty =
        AvaloniaProperty.Register<SkyListDetailPage, double?>(nameof(Progress));

    public static readonly StyledProperty<bool> IsDrawerOpenProperty =
        AvaloniaProperty.Register<SkyListDetailPage, bool>(
            nameof(IsDrawerOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> DrawerTitleProperty =
        AvaloniaProperty.Register<SkyListDetailPage, string?>(nameof(DrawerTitle));

    public static readonly StyledProperty<object?> DrawerContentProperty =
        AvaloniaProperty.Register<SkyListDetailPage, object?>(nameof(DrawerContent));

    public static readonly StyledProperty<SkyDrawerPlacement> DrawerPlacementProperty =
        AvaloniaProperty.Register<SkyListDetailPage, SkyDrawerPlacement>(
            nameof(DrawerPlacement),
            SkyDrawerPlacement.Right);

    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<SkyListDetailPage, double>(nameof(DrawerWidth), 320);

    private readonly AvaloniaList<object> primaryCommands = new();
    private readonly AvaloniaList<object> secondaryCommands = new();
    private readonly AvaloniaList<object> overflowCommands = new();
    private SkyDrawer? drawer;
    private SkyCommandBar? commandBar;
    private bool isSyncingDrawer;

    static SkyListDetailPage()
    {
        CommandBarTitleProperty.Changed.AddClassHandler<SkyListDetailPage>((page, _) => page.SyncCommandBar());
        IsDrawerOpenProperty.Changed.AddClassHandler<SkyListDetailPage>((page, _) => page.SyncDrawerOpen());
    }

    public SkyListDetailPage()
    {
        Classes.Add("sky");
        Classes.Add("sky-list-detail-page");

        primaryCommands.CollectionChanged += OnCommandCollectionChanged;
        secondaryCommands.CollectionChanged += OnCommandCollectionChanged;
        overflowCommands.CollectionChanged += OnCommandCollectionChanged;
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

    public object? HeaderActionContent
    {
        get => GetValue(HeaderActionContentProperty);
        set => SetValue(HeaderActionContentProperty, value);
    }

    public string? CommandBarTitle
    {
        get => GetValue(CommandBarTitleProperty);
        set => SetValue(CommandBarTitleProperty, value);
    }

    [Content]
    public IList PrimaryCommands => primaryCommands;

    public IList SecondaryCommands => secondaryCommands;

    public IList OverflowCommands => overflowCommands;

    public object? PaneContent
    {
        get => GetValue(PaneContentProperty);
        set => SetValue(PaneContentProperty, value);
    }

    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public bool IsPaneOpen
    {
        get => GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public SkySplitViewDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public object? StatusLeftContent
    {
        get => GetValue(StatusLeftContentProperty);
        set => SetValue(StatusLeftContentProperty, value);
    }

    public object? StatusRightContent
    {
        get => GetValue(StatusRightContentProperty);
        set => SetValue(StatusRightContentProperty, value);
    }

    public double? Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool IsDrawerOpen
    {
        get => GetValue(IsDrawerOpenProperty);
        set => SetValue(IsDrawerOpenProperty, value);
    }

    public string? DrawerTitle
    {
        get => GetValue(DrawerTitleProperty);
        set => SetValue(DrawerTitleProperty, value);
    }

    public object? DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    public SkyDrawerPlacement DrawerPlacement
    {
        get => GetValue(DrawerPlacementProperty);
        set => SetValue(DrawerPlacementProperty, value);
    }

    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (drawer is not null)
            drawer.Closed -= OnDrawerClosed;

        drawer = e.NameScope.Find(DrawerPartName) as SkyDrawer;
        commandBar = e.NameScope.Find(CommandBarPartName) as SkyCommandBar;

        if (drawer is not null)
            drawer.Closed += OnDrawerClosed;

        SyncCommandBar();
        SyncDrawerOpen();
    }

    private void OnCommandCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SyncCommandBar();

    private void SyncCommandBar()
    {
        if (commandBar is null)
            return;

        commandBar.Title = CommandBarTitle;
        CopyCommands(commandBar.PrimaryCommands, primaryCommands);
        CopyCommands(commandBar.SecondaryCommands, secondaryCommands);
        CopyCommands(commandBar.OverflowCommands, overflowCommands);
    }

    private static void CopyCommands(IList target, IList source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }

    private void SyncDrawerOpen()
    {
        if (drawer is null || isSyncingDrawer)
            return;

        var shouldOpen = IsDrawerOpen;
        if (drawer.IsOpen == shouldOpen)
            return;

        isSyncingDrawer = true;
        try
        {
            drawer.IsOpen = shouldOpen;
        }
        finally
        {
            isSyncingDrawer = false;
        }
    }

    private void OnDrawerClosed(object? sender, RoutedEventArgs e)
    {
        if (isSyncingDrawer || !IsDrawerOpen)
            return;

        isSyncingDrawer = true;
        try
        {
            IsDrawerOpen = false;
        }
        finally
        {
            isSyncingDrawer = false;
        }
    }
}
