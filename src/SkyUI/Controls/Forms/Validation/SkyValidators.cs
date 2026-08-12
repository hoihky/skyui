using System.Globalization;
using System.Text.RegularExpressions;

namespace SkyUI.Controls;

/// <summary>Factory methods for common <see cref="ISkyValidator"/> implementations.</summary>
public static class SkyValidators
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Rejects null, empty, or whitespace-only values.</summary>
    public static ISkyValidator Required(string errorMessage = "This field is required.") =>
        new DelegateSkyValidator(value =>
            value switch
            {
                null => SkyValidationResult.Invalid(errorMessage),
                string text when string.IsNullOrWhiteSpace(text) => SkyValidationResult.Invalid(errorMessage),
                _ => SkyValidationResult.Valid
            });

    /// <summary>Requires a string at least <paramref name="minLength"/> characters.</summary>
    public static ISkyValidator MinLength(int minLength, string? errorMessage = null) =>
        new DelegateSkyValidator(value =>
        {
            var text = value?.ToString() ?? string.Empty;
            return text.Length >= minLength
                ? SkyValidationResult.Valid
                : SkyValidationResult.Invalid(
                    errorMessage ?? $"Enter at least {minLength} characters.");
        });

    /// <summary>Requires a string at most <paramref name="maxLength"/> characters.</summary>
    public static ISkyValidator MaxLength(int maxLength, string? errorMessage = null) =>
        new DelegateSkyValidator(value =>
        {
            var text = value?.ToString() ?? string.Empty;
            return text.Length <= maxLength
                ? SkyValidationResult.Valid
                : SkyValidationResult.Invalid(
                    errorMessage ?? $"Enter at most {maxLength} characters.");
        });

    /// <summary>Requires a string matching a basic email pattern.</summary>
    public static ISkyValidator Email(string errorMessage = "Enter a valid email address.") =>
        new DelegateSkyValidator(value =>
        {
            var text = value?.ToString() ?? string.Empty;
            return EmailPattern.IsMatch(text)
                ? SkyValidationResult.Valid
                : SkyValidationResult.Invalid(errorMessage);
        });

    /// <summary>Requires a numeric value within an inclusive range.</summary>
    public static ISkyValidator Range(double minimum, double maximum, string? errorMessage = null) =>
        new DelegateSkyValidator(value =>
        {
            if (!TryToDouble(value, out var number))
                return SkyValidationResult.Invalid(errorMessage ?? "Enter a number.");

            return number >= minimum && number <= maximum
                ? SkyValidationResult.Valid
                : SkyValidationResult.Invalid(
                    errorMessage ?? $"Enter a value between {minimum} and {maximum}.");
        });

    /// <summary>Wraps a custom validation delegate.</summary>
    public static ISkyValidator Create(Func<object?, SkyValidationResult> validate) =>
        new DelegateSkyValidator(validate);

    private static bool TryToDouble(object? value, out double number)
    {
        switch (value)
        {
            case double d:
                number = d;
                return true;
            case float f:
                number = f;
                return true;
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case decimal m:
                number = (double)m;
                return true;
            case string text when double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsed):
                number = parsed;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private sealed class DelegateSkyValidator(Func<object?, SkyValidationResult> validate) : ISkyValidator
    {
        public SkyValidationResult Validate(object? value) => validate(value);
    }
}
