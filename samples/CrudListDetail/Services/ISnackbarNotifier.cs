using SkyUI.Controls;

namespace SkyUI.Samples.CrudListDetail.Services;

public interface ISnackbarNotifier
{
    void Show(string message, SkyFeedbackVariant variant = SkyFeedbackVariant.Neutral);
}
