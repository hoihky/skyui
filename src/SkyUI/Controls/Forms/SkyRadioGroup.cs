using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>
/// Single-selection radio group with MVVM-friendly <see cref="SelectedValue"/> binding.
/// </summary>
[TemplatePart(ItemsHostPartName, typeof(Panel))]
public class SkyRadioGroup : TemplatedControl
{
    public const string ItemsHostPartName = "PART_ItemsHost";

    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<SkyRadioGroup, object?>(
            nameof(SelectedValue),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SkyRadioGroup, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly DirectProperty<SkyRadioGroup, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyRadioGroup, IList>(
            nameof(Items),
            o => o.Items);

    public static readonly RoutedEvent<RoutedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SkyRadioGroup, RoutedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    private readonly AvaloniaList<SkyRadioGroupItem> _items = new();
    private Panel? _itemsHost;
    private readonly string _groupName = $"SkyRadioGroup_{Guid.NewGuid():N}";
    private bool _syncingSelection;

    static SkyRadioGroup()
    {
        SelectedValueProperty.Changed.AddClassHandler<SkyRadioGroup>((g, e) => g.OnSelectedValueChanged(e));
    }

    public SkyRadioGroup()
    {
        _items.CollectionChanged += OnItemsCollectionChanged;
    }

    [Content]
    public IList Items => _items;

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsHost = e.NameScope.Find(ItemsHostPartName) as Panel;
        RebuildRadioButtons();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        RebuildRadioButtons();

    private void OnSelectedValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncingSelection)
            return;

        SyncRadioButtonChecks();
        RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
    }

    private void RebuildRadioButtons()
    {
        if (_itemsHost is null)
            return;

        foreach (var child in _itemsHost.Children.OfType<RadioButton>())
            child.IsCheckedChanged -= OnRadioButtonChecked;

        _itemsHost.Children.Clear();

        if (_itemsHost is StackPanel stackPanel)
            stackPanel.Orientation = Orientation;

        foreach (var item in _items.OfType<SkyRadioGroupItem>())
        {
            var radio = new RadioButton
            {
                Classes = { "sky" },
                Content = item.Label,
                GroupName = _groupName,
                Tag = item,
                IsEnabled = item.IsEnabled,
                IsChecked = ValuesEqual(item.Value, SelectedValue)
            };
            radio.IsCheckedChanged += OnRadioButtonChecked;
            _itemsHost.Children.Add(radio);
        }
    }

    private void OnRadioButtonChecked(object? sender, RoutedEventArgs e)
    {
        if (_syncingSelection || sender is not RadioButton { IsChecked: true, Tag: SkyRadioGroupItem item })
            return;

        SetCurrentValue(SelectedValueProperty, item.Value);
    }

    private void SyncRadioButtonChecks()
    {
        if (_itemsHost is null)
            return;

        _syncingSelection = true;
        try
        {
            foreach (var radio in _itemsHost.Children.OfType<RadioButton>())
            {
                if (radio.Tag is SkyRadioGroupItem item)
                    radio.IsChecked = ValuesEqual(item.Value, SelectedValue);
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private static bool ValuesEqual(object? left, object? right) =>
        Equals(left, right);
}
