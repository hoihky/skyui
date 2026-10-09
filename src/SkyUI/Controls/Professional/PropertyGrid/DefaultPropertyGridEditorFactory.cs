using Avalonia.Controls;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

public sealed class DefaultPropertyGridEditorFactory : IPropertyGridEditorFactory
{
    public Control CreateEditor(PropertyGridItem item)
    {
        if (item.IsReadOnly)
            return CreateReadOnly(item);

        if (item.ValueType == typeof(bool))
            return CreateCheckBox(item);
        if (item.ValueType == typeof(Color))
            return CreateColorEditor(item);

        return CreateTextEditor(item);
    }

    private static Control CreateReadOnly(PropertyGridItem item) =>
        new TextBlock
        {
            Text = item.Value?.ToString() ?? "",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

    private static Control CreateTextEditor(PropertyGridItem item)
    {
        var box = new TextBox { Classes = { "sky" }, Text = item.Value?.ToString() ?? "" };
        box.LostFocus += (_, _) => item.Value = CoerceValue(item.ValueType, box.Text);
        return box;
    }

    private static Control CreateCheckBox(PropertyGridItem item)
    {
        var box = new CheckBox { Classes = { "sky" }, IsChecked = item.Value is true };
        box.IsCheckedChanged += (_, _) => item.Value = box.IsChecked == true;
        return box;
    }

    private static Control CreateColorEditor(PropertyGridItem item)
    {
        var picker = new SkyColorPicker { ShowHexInput = true };
        if (item.Value is Color color)
            picker.SelectedColor = color;
        picker.SelectedColorChanged += (_, _) => item.Value = picker.SelectedColor;
        return picker;
    }

    private static object? CoerceValue(Type type, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (type == typeof(string))
            return text;
        if (type == typeof(int) && int.TryParse(text, out var i))
            return i;
        if (type == typeof(double) && double.TryParse(text, out var d))
            return d;
        if (type == typeof(bool) && bool.TryParse(text, out var b))
            return b;
        return text;
    }
}
