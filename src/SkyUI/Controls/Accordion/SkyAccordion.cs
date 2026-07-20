using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

/// <summary>
/// Vertical stack of <see cref="SkyAccordionItem"/> entries. Supports XAML children, <see cref="ItemsControl.ItemsSource"/>,
/// and optional <see cref="ItemsControl.ItemTemplate"/> for non-<see cref="SkyAccordionItem"/> data (containers are created automatically).
/// </summary>
public class SkyAccordion : ItemsControl
{
    public static readonly StyledProperty<SkyAccordionSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<SkyAccordion, SkyAccordionSelectionMode>(
            nameof(SelectionMode),
            SkyAccordionSelectionMode.Multiple);

    static SkyAccordion()
    {
        SelectionModeProperty.Changed.AddClassHandler<SkyAccordion>((o, _) => o.OnSelectionModeChanged());
    }

    public SkyAccordion()
    {
        AddHandler(SkyAccordionItem.ExpandedEvent, OnChildExpanded, RoutingStrategies.Bubble);
    }

    public SkyAccordionSelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    private void OnSelectionModeChanged()
    {
        if (SelectionMode != SkyAccordionSelectionMode.Single)
            return;
        SkyAccordionItem? first = null;
        foreach (var ac in this.GetVisualDescendants().OfType<SkyAccordionItem>())
        {
            if (!ac.IsExpanded)
                continue;
            if (first is null)
            {
                first = ac;
                continue;
            }

            ac.IsExpanded = false;
        }
    }

    private void OnChildExpanded(object? sender, RoutedEventArgs e)
    {
        if (SelectionMode != SkyAccordionSelectionMode.Single)
            return;
        if (e.Source is not SkyAccordionItem opened)
            return;
        foreach (var ac in this.GetVisualDescendants().OfType<SkyAccordionItem>())
        {
            if (!ReferenceEquals(ac, opened))
                ac.IsExpanded = false;
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not SkyAccordionItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new SkyAccordionItem();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is not SkyAccordionItem ac || item is SkyAccordionItem)
            return;
        if (ac.Header is null && item is not null)
            ac.Header = item.ToString();
    }
}
