using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Shared paging state, slicing, and navigation commands for pagination controls.</summary>
public abstract class SkyPaginationBase : TemplatedControl
{
    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<SkyPaginationBase, int>(nameof(CurrentPage), 1);

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<SkyPaginationBase, int>(nameof(PageSize), 10);

    public static readonly StyledProperty<int> TotalCountProperty =
        AvaloniaProperty.Register<SkyPaginationBase, int>(nameof(TotalCount), 0);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyPaginationBase, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<bool> IsTotalCountAutomaticProperty =
        AvaloniaProperty.Register<SkyPaginationBase, bool>(nameof(IsTotalCountAutomatic), true);

    public static readonly StyledProperty<ItemsControl?> TargetProperty =
        AvaloniaProperty.Register<SkyPaginationBase, ItemsControl?>(nameof(Target));

    public static readonly DirectProperty<SkyPaginationBase, IEnumerable?> CurrentPageItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, IEnumerable?>(
            nameof(CurrentPageItems),
            control => control.CurrentPageItems);

    public static readonly DirectProperty<SkyPaginationBase, int> PageCountProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, int>(
            nameof(PageCount),
            control => control.PageCount);

    public static readonly DirectProperty<SkyPaginationBase, int> RangeStartProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, int>(
            nameof(RangeStart),
            control => control.RangeStart);

    public static readonly DirectProperty<SkyPaginationBase, int> RangeEndProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, int>(
            nameof(RangeEnd),
            control => control.RangeEnd);

    public static readonly DirectProperty<SkyPaginationBase, bool> CanGoToFirstProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, bool>(
            nameof(CanGoToFirst),
            control => control.CanGoToFirst);

    public static readonly DirectProperty<SkyPaginationBase, bool> CanGoToPreviousProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, bool>(
            nameof(CanGoToPrevious),
            control => control.CanGoToPrevious);

    public static readonly DirectProperty<SkyPaginationBase, bool> CanGoToNextProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, bool>(
            nameof(CanGoToNext),
            control => control.CanGoToNext);

    public static readonly DirectProperty<SkyPaginationBase, bool> CanGoToLastProperty =
        AvaloniaProperty.RegisterDirect<SkyPaginationBase, bool>(
            nameof(CanGoToLast),
            control => control.CanGoToLast);

    public static readonly RoutedEvent<RoutedEventArgs> PageChangedEvent =
        RoutedEvent.Register<SkyPaginationBase, RoutedEventArgs>(nameof(PageChanged), RoutingStrategies.Bubble);

    private readonly SkyPaginationRelayCommand firstPageCommand;
    private readonly SkyPaginationRelayCommand previousPageCommand;
    private readonly SkyPaginationRelayCommand nextPageCommand;
    private readonly SkyPaginationRelayCommand lastPageCommand;

    private IEnumerable? currentPageItems = Array.Empty<object>();
    private int pageCount;
    private int rangeStart;
    private int rangeEnd;
    private bool canGoToFirst;
    private bool canGoToPrevious;
    private bool canGoToNext;
    private bool canGoToLast;

    static SkyPaginationBase()
    {
        CurrentPageProperty.Changed.AddClassHandler<SkyPaginationBase>((control, args) =>
        {
            control.OnPagingInputChanged();
            if (args.NewValue is int newPage && args.OldValue is int oldPage && newPage != oldPage)
                control.RaiseEvent(new RoutedEventArgs(PageChangedEvent));
        });

        PageSizeProperty.Changed.AddClassHandler<SkyPaginationBase>((control, args) =>
        {
            if (args.NewValue is int newSize && args.OldValue is int oldSize && newSize != oldSize)
            {
                if (control.CurrentPage != 1)
                {
                    control.SetValue(CurrentPageProperty, 1);
                    return;
                }
            }

            control.OnPagingInputChanged();
        });

        TotalCountProperty.Changed.AddClassHandler<SkyPaginationBase>((control, _) => control.OnPagingInputChanged());
        ItemsSourceProperty.Changed.AddClassHandler<SkyPaginationBase>((control, _) => control.OnItemsSourceChanged());
        IsTotalCountAutomaticProperty.Changed.AddClassHandler<SkyPaginationBase>((control, _) => control.OnPagingInputChanged());
        TargetProperty.Changed.AddClassHandler<SkyPaginationBase>((control, _) => control.SyncTargetItemsSource());
    }

    protected SkyPaginationBase()
    {
        firstPageCommand = new SkyPaginationRelayCommand(() => GoToFirst(), () => CanGoToFirst);
        previousPageCommand = new SkyPaginationRelayCommand(() => GoToPrevious(), () => CanGoToPrevious);
        nextPageCommand = new SkyPaginationRelayCommand(() => GoToNext(), () => CanGoToNext);
        lastPageCommand = new SkyPaginationRelayCommand(() => GoToLast(), () => CanGoToLast);
    }

    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public int TotalCount
    {
        get => GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool IsTotalCountAutomatic
    {
        get => GetValue(IsTotalCountAutomaticProperty);
        set => SetValue(IsTotalCountAutomaticProperty, value);
    }

    public ItemsControl? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public IEnumerable? CurrentPageItems => currentPageItems;

    public int PageCount => pageCount;

    public int RangeStart => rangeStart;

    public int RangeEnd => rangeEnd;

    public bool CanGoToFirst => canGoToFirst;

    public bool CanGoToPrevious => canGoToPrevious;

    public bool CanGoToNext => canGoToNext;

    public bool CanGoToLast => canGoToLast;

    public ICommand FirstPageCommand => firstPageCommand;

    public ICommand PreviousPageCommand => previousPageCommand;

    public ICommand NextPageCommand => nextPageCommand;

    public ICommand LastPageCommand => lastPageCommand;

    public event EventHandler<RoutedEventArgs>? PageChanged
    {
        add => AddHandler(PageChangedEvent, value);
        remove => RemoveHandler(PageChangedEvent, value);
    }

    public void GoToFirst()
    {
        if (!CanGoToFirst)
            return;

        SetCurrentPage(1);
    }

    public void GoToPrevious()
    {
        if (!CanGoToPrevious)
            return;

        SetCurrentPage(CurrentPage - 1);
    }

    public void GoToNext()
    {
        if (!CanGoToNext)
            return;

        SetCurrentPage(CurrentPage + 1);
    }

    public void GoToLast()
    {
        if (!CanGoToLast)
            return;

        SetCurrentPage(PageCount);
    }

    private void OnItemsSourceChanged()
    {
        if (IsTotalCountAutomatic)
            SetValue(TotalCountProperty, SkyPaginationMath.CountItems(ItemsSource));

        OnPagingInputChanged();
    }

    private void OnPagingInputChanged()
    {
        var resolvedPageCount = SkyPaginationMath.ComputePageCount(TotalCount, PageSize);
        var clampedPage = SkyPaginationMath.ClampPage(CurrentPage, resolvedPageCount);

        if (clampedPage != CurrentPage)
            SetValue(CurrentPageProperty, clampedPage);

        var range = SkyPaginationMath.GetInclusiveRange(clampedPage, PageSize, TotalCount);
        var slice = SkyPaginationMath.Slice(ItemsSource, clampedPage, PageSize);

        SetAndRaise(CurrentPageItemsProperty, ref currentPageItems, slice);
        SetAndRaise(PageCountProperty, ref pageCount, resolvedPageCount);
        SetAndRaise(RangeStartProperty, ref rangeStart, range.Start);
        SetAndRaise(RangeEndProperty, ref rangeEnd, range.End);

        var hasPages = resolvedPageCount > 0;
        SetAndRaise(CanGoToFirstProperty, ref canGoToFirst, hasPages && clampedPage > 1);
        SetAndRaise(CanGoToPreviousProperty, ref canGoToPrevious, hasPages && clampedPage > 1);
        SetAndRaise(CanGoToNextProperty, ref canGoToNext, hasPages && clampedPage < resolvedPageCount);
        SetAndRaise(CanGoToLastProperty, ref canGoToLast, hasPages && clampedPage < resolvedPageCount);

        firstPageCommand.NotifyCanExecuteChanged();
        previousPageCommand.NotifyCanExecuteChanged();
        nextPageCommand.NotifyCanExecuteChanged();
        lastPageCommand.NotifyCanExecuteChanged();

        SyncTargetItemsSource();
    }

    private void SetCurrentPage(int page)
    {
        if (page == CurrentPage)
            return;

        SetValue(CurrentPageProperty, page);
    }

    private void SyncTargetItemsSource()
    {
        if (Target is null)
            return;

        Target.ItemsSource = currentPageItems;
    }
}
