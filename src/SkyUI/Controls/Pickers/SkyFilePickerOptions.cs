using Avalonia.Platform.Storage;

namespace SkyUI.Controls;

/// <summary>Options for <see cref="SkyFilePicker"/> dialogs.</summary>
public sealed class SkyFilePickerOptions
{
    public string? Title { get; init; }

    public bool AllowMultiple { get; init; }

    public IReadOnlyList<FilePickerFileType>? FileTypes { get; init; }

    public string? SuggestedStartLocation { get; init; }

    public string? SuggestedFileName { get; init; }

    public static SkyFilePickerOptions JsonFiles(string? title = null) =>
        new()
        {
            Title = title,
            FileTypes = [FilePickerFileTypes.Json],
        };

    public static SkyFilePickerOptions TextFiles(string? title = null) =>
        new()
        {
            Title = title,
            FileTypes = [FilePickerFileTypes.TextPlain],
        };

    public static SkyFilePickerOptions AllFiles(string? title = null) =>
        new()
        {
            Title = title,
            FileTypes = [FilePickerFileTypes.All],
        };

    public static SkyFilePickerOptions CsvFiles(string? title = null) =>
        new()
        {
            Title = title,
            FileTypes =
            [
                new FilePickerFileType("CSV")
                {
                    Patterns = ["*.csv"],
                    MimeTypes = ["text/csv"],
                },
            ],
        };
}
