using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

/// <summary>Modal color chooser hosting a full <see cref="SkyColorPicker"/> surface.</summary>
public sealed class SkyColorSelectionDialog : Window
{
    private readonly SkyColorPicker picker;

    public SkyColorSelectionDialog(Color initialColor)
    {
        Title = "Select color";
        Width = 360;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        picker = new SkyColorPicker
        {
            SelectedColor = initialColor,
            UseDropDownStyle = false,
            ShowHexInput = true,
            Margin = new Thickness(16),
        };
        var ok = new Button { Classes = { "sky", "sky-primary" }, Content = "OK", MinWidth = 88 };
        var cancel = new Button { Classes = { "sky" }, Content = "Cancel", MinWidth = 88 };
        ok.Click += OnOk;
        cancel.Click += OnCancel;
        Content = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                picker,
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 8,
                    Margin = new Thickness(16, 0, 16, 16),
                    Children = { cancel, ok },
                },
            },
        };
    }

    public Color SelectedColor => picker.SelectedColor;

    public static async Task<Color?> ShowAsync(Window owner, Color initialColor)
    {
        var dialog = new SkyColorSelectionDialog(initialColor) { Owner = owner };
        var result = await dialog.ShowDialog<Color?>(owner);
        return result;
    }

    private void OnOk(object? sender, RoutedEventArgs e) => Close(SelectedColor);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
