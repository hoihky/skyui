using System.Collections;
using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary><see cref="SkyFormField"/> with an integrated styled <see cref="ComboBox"/>.</summary>
public class SkyComboBoxField : SkyFormField
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyComboBoxField, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SkyComboBoxField, object?>(
            nameof(SelectedItem),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SkyComboBoxField, string?>(nameof(PlaceholderText));

    private readonly ComboBox comboBox = new() { Classes = { "sky" } };

    static SkyComboBoxField()
    {
        ItemsSourceProperty.Changed.AddClassHandler<SkyComboBoxField>((field, e) =>
            field.comboBox.ItemsSource = e.NewValue as IEnumerable);
        SelectedItemProperty.Changed.AddClassHandler<SkyComboBoxField>((field, e) =>
            field.comboBox.SelectedItem = e.NewValue);
        PlaceholderTextProperty.Changed.AddClassHandler<SkyComboBoxField>((field, e) =>
            field.comboBox.PlaceholderText = e.NewValue as string);
    }

    public SkyComboBoxField()
    {
        Content = comboBox;
        comboBox.SelectionChanged += OnComboSelectionChanged;
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public ComboBox ComboBox => comboBox;

    private void OnComboSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        SelectedItem = comboBox.SelectedItem;
}
