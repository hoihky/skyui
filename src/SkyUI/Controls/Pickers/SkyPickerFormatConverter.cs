using System.Globalization;
using Avalonia.Data.Converters;

namespace SkyUI.Controls;

/// <summary>Formats <see cref="DateTime"/> or <see cref="TimeSpan"/> values using <see cref="SkyPickerFormat"/>.</summary>
public sealed class SkyPickerFormatConverter : IValueConverter
{
    public static readonly SkyPickerFormatConverter DateInstance = new(SkyPickerFormatKind.Date);
    public static readonly SkyPickerFormatConverter TimeInstance = new(SkyPickerFormatKind.Time);
    public static readonly SkyPickerFormatConverter LongDateInstance = new(SkyPickerFormatKind.LongDate);

    private readonly SkyPickerFormatKind _kind;

    private SkyPickerFormatConverter(SkyPickerFormatKind kind) => _kind = kind;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var formatCulture = parameter switch
        {
            CultureInfo cultureInfo => cultureInfo,
            string name when !string.IsNullOrWhiteSpace(name) => new CultureInfo(name),
            _ => culture
        };

        return _kind switch
        {
            SkyPickerFormatKind.Date when value is DateTime dateTime =>
                SkyPickerFormat.FormatDate(dateTime, formatCulture),
            SkyPickerFormatKind.Date when value is DateTimeOffset offset =>
                SkyPickerFormat.FormatDate(offset.DateTime, formatCulture),
            SkyPickerFormatKind.LongDate when value is DateTime dateTime =>
                SkyPickerFormat.FormatLongDate(dateTime, formatCulture),
            SkyPickerFormatKind.LongDate when value is DateTimeOffset offset =>
                SkyPickerFormat.FormatLongDate(offset.DateTime, formatCulture),
            SkyPickerFormatKind.Time when value is TimeSpan time =>
                SkyPickerFormat.FormatTime(time, formatCulture),
            _ => string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private enum SkyPickerFormatKind
    {
        Date,
        LongDate,
        Time
    }
}
