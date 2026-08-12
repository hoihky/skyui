using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Sky-themed date picker with calendar dropdown and culture-aware formatting.</summary>
public class SkyDatePicker : CalendarDatePicker
{
    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<SkyDatePicker, CultureInfo?>(nameof(Culture));

    static SkyDatePicker()
    {
        CultureProperty.Changed.AddClassHandler<SkyDatePicker>((picker, e) =>
            picker.ApplyCulture((CultureInfo?)e.NewValue));
    }

    public SkyDatePicker()
    {
        Classes.Add("sky");
        Classes.Add("sky-date-picker");
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
