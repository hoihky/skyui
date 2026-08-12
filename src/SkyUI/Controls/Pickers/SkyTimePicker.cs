using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed time picker with culture-aware 12/24-hour clock selection.</summary>
public class SkyTimePicker : TimePicker
{
    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<SkyTimePicker, CultureInfo?>(nameof(Culture));

    static SkyTimePicker()
    {
        CultureProperty.Changed.AddClassHandler<SkyTimePicker>((picker, e) =>
            picker.ApplyCulture((CultureInfo?)e.NewValue));
    }

    public SkyTimePicker()
    {
        Classes.Add("sky");
        Classes.Add("sky-time-picker");
        MinuteIncrement = 5;
        ApplyCulture(CultureInfo.CurrentCulture);
    }

    public CultureInfo? Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    private void ApplyCulture(CultureInfo? culture) =>
        SkyPickerFormat.ApplyCulture(this, SkyPickerFormat.Resolve(culture));
}
