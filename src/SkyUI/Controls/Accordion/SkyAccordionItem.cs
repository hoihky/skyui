using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

[PseudoClasses("expanded")]
public class SkyAccordionItem : ContentControl
{
    public const string PartHeaderToggle = "PART_HeaderToggle";

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SkyAccordionItem, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<SkyAccordionItem, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<SkyAccordionItem, bool>(nameof(IsExpanded));

    public static readonly RoutedEvent<RoutedEventArgs> ExpandedEvent =
        RoutedEvent.Register<SkyAccordionItem, RoutedEventArgs>(nameof(Expanded), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CollapsedEvent =
        RoutedEvent.Register<SkyAccordionItem, RoutedEventArgs>(nameof(Collapsed), RoutingStrategies.Bubble);

    private ToggleButton? _headerToggle;
    private IDisposable? _headerToggleBinding;

    static SkyAccordionItem()
    {
        IsExpandedProperty.Changed.AddClassHandler<SkyAccordionItem>(OnIsExpandedChanged);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerToggleBinding?.Dispose();
        _headerToggleBinding = null;

        _headerToggle = e.NameScope.Find<ToggleButton>(PartHeaderToggle);
        if (_headerToggle is not null)
        {
            _headerToggleBinding = _headerToggle.Bind(
                ToggleButton.IsCheckedProperty,
                new Binding(nameof(IsExpanded)) { Source = this, Mode = BindingMode.TwoWay });
        }
    }

    private static void OnIsExpandedChanged(SkyAccordionItem item, AvaloniaPropertyChangedEventArgs e)
    {
        var expanded = e.GetNewValue<bool>();
        item.PseudoClasses.Set(":expanded", expanded);

        if (expanded)
            item.RaiseEvent(new RoutedEventArgs(ExpandedEvent));
        else
            item.RaiseEvent(new RoutedEventArgs(CollapsedEvent));
    }
}
