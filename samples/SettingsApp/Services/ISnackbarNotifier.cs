using SkyUI.Controls;

namespace SkyUI.Samples.SettingsApp.Services;

/// <summary>Facade over <see cref="SkySnackbarHost"/> for view models.</summary>
public interface ISnackbarNotifier
{
    void Show(string message, SkyFeedbackVariant variant = SkyFeedbackVariant.Neutral);
}
