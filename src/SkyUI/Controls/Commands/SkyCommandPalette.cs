using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>Searchable command launcher (palette) for keyboard-driven apps.</summary>
public class SkyCommandPalette : TemplatedControl
{
    public const string OverlayPartName = "PART_Overlay";
    public const string SearchPartName = "PART_Search";
    public const string ResultsPartName = "PART_Results";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SkyCommandPalette, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<SkyCommandPalette, string?>(nameof(SearchText), string.Empty);

    public static readonly StyledProperty<ICommandPaletteFilter?> FilterProperty =
        AvaloniaProperty.Register<SkyCommandPalette, ICommandPaletteFilter?>(
            nameof(Filter),
            defaultValue: DefaultCommandPaletteFilter.Instance);

    public static readonly DirectProperty<SkyCommandPalette, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyCommandPalette, IList>(
            nameof(Items),
            p => p.Items);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyCommandPalette, IEnumerable?>(nameof(ItemsSource));

    public static readonly DirectProperty<SkyCommandPalette, IList> FilteredItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyCommandPalette, IList>(
            nameof(FilteredItems),
            p => p.FilteredItems);

    private readonly AvaloniaList<SkyCommandPaletteItem> items = new();
    private readonly AvaloniaList<SkyCommandPaletteItem> filteredItems = new();
    private Panel? overlay;
    private TextBox? searchBox;
    private ListBox? results;

    public event EventHandler<SkyCommandPaletteItem>? CommandExecuted;

    public SkyCommandPalette()
    {
        items.CollectionChanged += OnSourceItemsChanged;
        Classes.Add("sky");
        Classes.Add("sky-command-palette");
        Focusable = true;
    }

    [Content]
    public IList Items => items;

    public IList FilteredItems => filteredItems;

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public ICommandPaletteFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    static SkyCommandPalette()
    {
        IsOpenProperty.Changed.AddClassHandler<SkyCommandPalette>((p, e) => p.OnIsOpenChanged((bool)e.NewValue!));
        SearchTextProperty.Changed.AddClassHandler<SkyCommandPalette>((p, _) => p.RebuildFilteredItems());
        ItemsSourceProperty.Changed.AddClassHandler<SkyCommandPalette>((p, e) =>
            p.ApplyItemsSource(e.NewValue as IEnumerable));
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private void ApplyItemsSource(IEnumerable? source)
    {
        items.Clear();
        if (source is null)
            return;
        foreach (var item in source.OfType<SkyCommandPaletteItem>())
            items.Add(item);
        RebuildFilteredItems();
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void ExecuteSelected()
    {
        if (results?.SelectedItem is SkyCommandPaletteItem item)
            Execute(item);
    }

    /// <summary>Runs <paramref name="item"/> if enabled and closes the palette.</summary>
    public void Execute(SkyCommandPaletteItem item) => ExecuteItem(item);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        overlay = e.NameScope.Find<Panel>(OverlayPartName);
        searchBox = e.NameScope.Find<TextBox>(SearchPartName);
        results = e.NameScope.Find<ListBox>(ResultsPartName);

        if (searchBox is not null)
        {
            searchBox.Text = SearchText ?? string.Empty;
            searchBox.TextChanged += OnSearchTextChanged;
            searchBox.KeyDown += OnSearchKeyDown;
        }

        if (results is not null)
        {
            results.ItemsSource = filteredItems;
            results.DoubleTapped += OnResultDoubleTapped;
            results.KeyDown += OnResultsKeyDown;
        }

        RebuildFilteredItems();
        UpdateOverlayVisibility();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsOpen)
            return;
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void OnIsOpenChanged(bool open)
    {
        UpdateOverlayVisibility();
        if (!open)
            return;
        RebuildFilteredItems();
        searchBox?.Focus();
        if (results is not null && filteredItems.Count > 0)
            results.SelectedIndex = 0;
    }

    private void UpdateOverlayVisibility()
    {
        if (overlay is not null)
            overlay.IsVisible = IsOpen;
    }

    private void OnSourceItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildFilteredItems();

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (searchBox is null)
            return;
        SetCurrentValue(SearchTextProperty, searchBox.Text);
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && results is not null)
        {
            results.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ExecuteSelected();
            e.Handled = true;
        }
    }

    private void OnResultsKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ExecuteSelected();
            e.Handled = true;
        }
    }

    private void OnResultDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (results?.SelectedItem is SkyCommandPaletteItem item)
            ExecuteItem(item);
    }

    private void RebuildFilteredItems()
    {
        filteredItems.Clear();
        var filter = Filter ?? DefaultCommandPaletteFilter.Instance;
        foreach (var item in items)
        {
            if (filter.Matches(item, SearchText))
                filteredItems.Add(item);
        }

        if (results is not null && filteredItems.Count > 0 && results.SelectedIndex < 0)
            results.SelectedIndex = 0;
    }

    private void ExecuteItem(SkyCommandPaletteItem item)
    {
        if (!item.IsEnabled)
            return;
        if (item.Command?.CanExecute(null) == true)
            item.Command.Execute(null);
        CommandExecuted?.Invoke(this, item);
        Close();
    }
}
