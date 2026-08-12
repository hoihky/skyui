namespace SkyUI.Controls;

/// <summary>
/// Strategy for validating a single form value. Compose with <see cref="CompositeSkyValidator"/>
/// or use <see cref="SkyValidators"/> factories.
/// </summary>
public interface ISkyValidator
{
    /// <summary>Validates <paramref name="value"/> from a form input.</summary>
    SkyValidationResult Validate(object? value);
}
