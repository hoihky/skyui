namespace SkyUI.Controls;

/// <summary>Single form-level validation error for <see cref="SkyValidationSummary"/>.</summary>
public sealed class SkyValidationSummaryItem
{
    public SkyValidationSummaryItem()
    {
    }

    public SkyValidationSummaryItem(string fieldKey, string message)
    {
        FieldKey = fieldKey;
        Message = message;
    }

    public string FieldKey { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
