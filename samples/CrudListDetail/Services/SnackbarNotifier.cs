using SkyUI.Controls;

namespace SkyUI.Samples.CrudListDetail.Services;

public sealed class SnackbarNotifier : ISnackbarNotifier
{
    private readonly ISnackbarHostAccessor hostAccessor;

    public SnackbarNotifier(ISnackbarHostAccessor hostAccessor)
    {
        this.hostAccessor = hostAccessor;
    }

    public void Show(string message, SkyFeedbackVariant variant = SkyFeedbackVariant.Neutral) =>
        hostAccessor.Host.Enqueue(message, variant);
}
