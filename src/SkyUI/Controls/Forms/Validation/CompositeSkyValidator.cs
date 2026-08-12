namespace SkyUI.Controls;

/// <summary>Runs validators in order and returns the first failure.</summary>
public sealed class CompositeSkyValidator : ISkyValidator
{
    private readonly IReadOnlyList<ISkyValidator> _validators;

    public CompositeSkyValidator(params ISkyValidator[] validators) =>
        _validators = validators;

    public CompositeSkyValidator(IEnumerable<ISkyValidator> validators) =>
        _validators = validators.ToList();

    public SkyValidationResult Validate(object? value)
    {
        foreach (var validator in _validators)
        {
            var result = validator.Validate(value);
            if (!result.IsValid)
                return result;
        }

        return SkyValidationResult.Valid;
    }
}
