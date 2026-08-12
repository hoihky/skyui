using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>Sky-themed inline calendar month view.</summary>
public class SkyCalendar : Avalonia.Controls.Calendar
{
    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<SkyCalendar, CultureInfo?>(nameof(Culture));

    static SkyCalendar()
    {
        CultureProperty.Changed.AddClassHandler<SkyCalendar>((calendar, e) =>
            calendar.ApplyCulture((CultureInfo?)e.NewValue));
    }

    public SkyCalendar()
    {
        Classes.Add("sky");
        Classes.Add("sky-calendar");
        DisplayMode = CalendarMode.Month;
        IsTodayHighlighted = true;
        ApplyCulture(CultureInfo.CurrentCulture);
    }

    public CultureInfo? Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e) =>
        e.Handled = true;

    private void ApplyCulture(CultureInfo? culture) =>
        SkyPickerFormat.ApplyCulture(this, SkyPickerFormat.Resolve(culture));
}
