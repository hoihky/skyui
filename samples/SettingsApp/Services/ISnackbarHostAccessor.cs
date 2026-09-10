using SkyUI.Controls;

namespace SkyUI.Samples.SettingsApp.Services;

/// <summary>Provides the snackbar host after the shell window is created.</summary>
public interface ISnackbarHostAccessor
{
    SkySnackbarHost Host { get; }

    void Attach(SkySnackbarHost host);
}
