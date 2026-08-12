namespace SkyUI.Controls;

/// <summary>Outcome of <see cref="ISkyValidator.Validate"/>.</summary>
public readonly record struct SkyValidationResult
{
    public static SkyValidationResult Valid { get; } = new(true, null);

    public SkyValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public static SkyValidationResult Invalid(string errorMessage) =>
        new(false, errorMessage);
}
