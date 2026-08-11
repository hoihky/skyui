namespace SkyUI.Controls;

/// <summary>Transient snackbar message shown by <see cref="SkySnackbarHost"/>.</summary>
public sealed class SkySnackbarMessage
{
    public SkySnackbarMessage(string message, SkyFeedbackVariant variant, int durationMs)
    {
        Message = message;
        Variant = variant;
        DurationMs = durationMs;
    }

    public string Message { get; }

    public SkyFeedbackVariant Variant { get; }

    public int DurationMs { get; }
}
