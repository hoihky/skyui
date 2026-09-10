using SkyUI.Samples.SettingsApp.Models;

namespace SkyUI.Samples.SettingsApp.Services;

/// <summary>Persists the settings document (repository abstraction).</summary>
public interface ISettingsStore
{
    Task<SettingsDocument> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(SettingsDocument document, CancellationToken cancellationToken = default);
}
