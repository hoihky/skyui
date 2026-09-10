using System.Text.Json;
using SkyUI.Samples.SettingsApp.Models;

namespace SkyUI.Samples.SettingsApp.Services;

/// <summary>File-based settings store for the desktop sample.</summary>
public sealed class JsonFileSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string filePath;

    public JsonFileSettingsStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SkyUI",
            "SettingsApp");
        Directory.CreateDirectory(folder);
        filePath = Path.Combine(folder, "settings.json");
    }

    public async Task<SettingsDocument> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return new SettingsDocument();

        await using var stream = File.OpenRead(filePath);
        var document = await JsonSerializer.DeserializeAsync<SettingsDocument>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return document ?? new SettingsDocument();
    }

    public async Task SaveAsync(SettingsDocument document, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
