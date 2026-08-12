using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>Converts <see cref="KeyGesture"/> to a platform-formatted accelerator string for UI binding.</summary>
public sealed class SkyAcceleratorFormatConverter : IValueConverter
{
    public static readonly SkyAcceleratorFormatConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is KeyGesture gesture ? SkyAccelerator.Format(gesture) : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        SkyAccelerator.Parse(value as string);
}
