using System.Globalization;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Culture-aware date and time formatting for Sky pickers.</summary>
public static class SkyPickerFormat
{
    public static CultureInfo Resolve(CultureInfo? culture) =>
        culture ?? CultureInfo.CurrentCulture;

    public static void ApplyCulture(CalendarDatePicker picker, CultureInfo culture)
    {
        var resolved = Resolve(culture);
        picker.FirstDayOfWeek = resolved.DateTimeFormat.FirstDayOfWeek;
        picker.SelectedDateFormat = CalendarDatePickerFormat.Custom;
        picker.CustomDateFormatString = resolved.DateTimeFormat.ShortDatePattern;
    }

    public static void ApplyCulture(Avalonia.Controls.Calendar calendar, CultureInfo culture)
    {
        var resolved = Resolve(culture);
        calendar.FirstDayOfWeek = resolved.DateTimeFormat.FirstDayOfWeek;
    }

    public static void ApplyCulture(TimePicker picker, CultureInfo culture)
    {
        picker.ClockIdentifier = Uses12HourClock(Resolve(culture)) ? "12HourClock" : "24HourClock";
    }

    public static string FormatDate(DateTime? date, CultureInfo? culture = null)
    {
        if (!date.HasValue)
            return string.Empty;

        var resolved = Resolve(culture);
        return date.Value.ToString(resolved.DateTimeFormat.ShortDatePattern, resolved);
    }

    public static string FormatLongDate(DateTime? date, CultureInfo? culture = null)
    {
        if (!date.HasValue)
            return string.Empty;

        var resolved = Resolve(culture);
        return date.Value.ToString(resolved.DateTimeFormat.LongDatePattern, resolved);
    }

    public static string FormatTime(TimeSpan? time, CultureInfo? culture = null)
    {
        if (!time.HasValue)
            return string.Empty;

        var resolved = Resolve(culture);
        var sample = DateTime.Today.Add(time.Value);
        return sample.ToString(resolved.DateTimeFormat.ShortTimePattern, resolved);
    }

    public static bool TryParseDate(string? text, out DateTime? date, CultureInfo? culture = null)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var resolved = Resolve(culture);
        if (DateTime.TryParse(text, resolved, DateTimeStyles.None, out var parsed))
        {
            date = parsed.Date;
            return true;
        }

        return false;
    }

    public static bool Uses12HourClock(CultureInfo culture)
    {
        var pattern = culture.DateTimeFormat.ShortTimePattern;
        return pattern.Contains('h', StringComparison.Ordinal) ||
               pattern.Contains('t', StringComparison.OrdinalIgnoreCase);
    }
}
