using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>Shared flyout menu wiring for drop-down command buttons.</summary>
public abstract class SkyDropDownButtonBase : TemplatedControl
{
    public static readonly DirectProperty<SkyDropDownButtonBase, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyDropDownButtonBase, IList>(
            nameof(Items),
            b => b.Items);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyDropDownButtonBase, IEnumerable?>(nameof(ItemsSource));

    private readonly AvaloniaList<SkyDropDownMenuItem> items = new();
    private SkyMenuFlyout? flyout;

    protected SkyDropDownButtonBase()
    {
        items.CollectionChanged += OnItemsCollectionChanged;
        Classes.Add("sky");
    }

    static SkyDropDownButtonBase()
    {
        ItemsSourceProperty.Changed.AddClassHandler<SkyDropDownButtonBase>((b, e) =>
            b.ApplyItemsSource(e.NewValue as IEnumerable));
    }

    [Content]
    public IList Items => items;

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
        foreach (var entry in source.OfType<SkyDropDownMenuItem>())
            items.Add(entry);
    }

    protected void ShowDropDown(Control anchor)
    {
        flyout ??= CreateFlyout();
        SyncFlyoutItems();
        FlyoutBase.SetAttachedFlyout(anchor, flyout);
        FlyoutBase.ShowAttachedFlyout(anchor);
    }

    private SkyMenuFlyout CreateFlyout()
    {
        var menu = new SkyMenuFlyout();
        menu.Closed += (_, _) => OnDropDownClosed();
        return menu;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SyncFlyoutItems();

    private void SyncFlyoutItems()
    {
        if (flyout is null)
            return;
        flyout.Items.Clear();
        foreach (var entry in items)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = entry.Label,
                Command = entry.Command,
                IsEnabled = entry.IsEnabled,
            });
        }
    }

    protected virtual void OnDropDownClosed()
    {
    }
}
