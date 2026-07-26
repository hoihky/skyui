using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Icons;

/// <summary>Attach icons to buttons and list items (composes icon + label content).</summary>
public static class SkyIconProperties
{
    public static readonly AttachedProperty<SkyIconKind?> IconProperty =
        AvaloniaProperty.RegisterAttached<Control, SkyIconKind?>("Icon", typeof(SkyIconProperties));

    public static readonly AttachedProperty<SkyIconSize?> IconSizeProperty =
        AvaloniaProperty.RegisterAttached<Control, SkyIconSize?>("IconSize", typeof(SkyIconProperties));

    public static readonly AttachedProperty<SkyIconChrome> IconChromeProperty =
        AvaloniaProperty.RegisterAttached<Control, SkyIconChrome>(
            "IconChrome",
            typeof(SkyIconProperties),
            defaultValue: SkyIconChrome.Button);

    private static readonly AttachedProperty<object?> IconLabelProperty =
        AvaloniaProperty.RegisterAttached<Control, object?>("IconLabel", typeof(SkyIconProperties));

    static SkyIconProperties()
    {
        IconProperty.Changed.AddClassHandler<Control>(OnIconChanged);
        IconSizeProperty.Changed.AddClassHandler<Control>(OnIconLayoutChanged);
        IconChromeProperty.Changed.AddClassHandler<Control>(OnIconLayoutChanged);
    }

    public static SkyIconKind? GetIcon(Control element) => element.GetValue(IconProperty);

    public static void SetIcon(Control element, SkyIconKind? value) => element.SetValue(IconProperty, value);

    public static SkyIconSize? GetIconSize(Control element) => element.GetValue(IconSizeProperty);

    public static void SetIconSize(Control element, SkyIconSize? value) => element.SetValue(IconSizeProperty, value);

    public static SkyIconChrome GetIconChrome(Control element) => element.GetValue(IconChromeProperty);

    public static void SetIconChrome(Control element, SkyIconChrome value) => element.SetValue(IconChromeProperty, value);

    private static void OnIconChanged(Control control, AvaloniaPropertyChangedEventArgs e) =>
        RebuildContent(control);

    private static void OnIconLayoutChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetIcon(control) is not null)
            RebuildContent(control);
    }

    private static void RebuildContent(Control control)
    {
        var icon = GetIcon(control);
        if (icon is null or SkyIconKind.None)
        {
            if (control.GetValue(IconLabelProperty) is { } saved)
            {
                control.SetValue(IconLabelProperty, null);
                if (control is ContentControl cc)
                    cc.Content = saved;
            }

            control.Classes.Remove("sky-has-icon");
            return;
        }

        control.Classes.Add("sky-has-icon");

        if (control is ContentControl contentControl)
        {
            if (contentControl.GetValue(IconLabelProperty) is null
                && contentControl.Content is not StackPanel)
                contentControl.SetValue(IconLabelProperty, contentControl.Content);

            var label = contentControl.GetValue(IconLabelProperty);
            var size = GetIconSize(contentControl) ?? GetIconChrome(contentControl).ToIconSize();

            contentControl.Content = CreateRow(icon.Value, size, label, contentControl.Foreground);
        }
    }

    internal static StackPanel CreateRow(SkyIconKind kind, SkyIconSize size, object? label, IBrush? foreground)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
        };

        row.Children.Add(new SkyIcon
        {
            Kind = kind,
            Size = size,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = foreground,
        });

        if (label is string text)
        {
            row.Children.Add(new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = foreground,
            });
        }
        else if (label is Control child)
            row.Children.Add(child);
        else if (label is not null)
            row.Children.Add(new ContentControl { Content = label, VerticalAlignment = VerticalAlignment.Center });

        return row;
    }
}
