using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace SkyUI.Controls;

/// <summary>Consistent wrapper over Avalonia storage pickers.</summary>
public static class SkyFilePicker
{
    public static async Task<IReadOnlyList<IStorageFile>> OpenFilesAsync(
        Visual? owner = null,
        SkyFilePickerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var storage = ResolveStorageProvider(owner);
        if (storage is null || !storage.CanOpen)
            return Array.Empty<IStorageFile>();

        var resolved = options ?? new SkyFilePickerOptions();
        var pickerOptions = new FilePickerOpenOptions
        {
            Title = resolved.Title,
            AllowMultiple = resolved.AllowMultiple,
            FileTypeFilter = resolved.FileTypes,
            SuggestedStartLocation = await ResolveFolderAsync(storage, resolved.SuggestedStartLocation, cancellationToken)
                .ConfigureAwait(false),
        };

        return await storage.OpenFilePickerAsync(pickerOptions).ConfigureAwait(false);
    }

    public static async Task<IStorageFile?> OpenFileAsync(
        Visual? owner = null,
        SkyFilePickerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var files = await OpenFilesAsync(
            owner,
            options is null
                ? new SkyFilePickerOptions()
                : new SkyFilePickerOptions
                {
                    Title = options.Title,
                    AllowMultiple = false,
                    FileTypes = options.FileTypes,
                    SuggestedStartLocation = options.SuggestedStartLocation,
                    SuggestedFileName = options.SuggestedFileName,
                },
            cancellationToken).ConfigureAwait(false);

        return files.Count > 0 ? files[0] : null;
    }

    public static async Task<IStorageFile?> SaveFileAsync(
        Visual? owner = null,
        SkyFilePickerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var storage = ResolveStorageProvider(owner);
        if (storage is null || !storage.CanSave)
            return null;

        var resolved = options ?? new SkyFilePickerOptions();
        var pickerOptions = new FilePickerSaveOptions
        {
            Title = resolved.Title,
            SuggestedFileName = resolved.SuggestedFileName,
            FileTypeChoices = resolved.FileTypes,
            SuggestedStartLocation = await ResolveFolderAsync(storage, resolved.SuggestedStartLocation, cancellationToken)
                .ConfigureAwait(false),
        };

        return await storage.SaveFilePickerAsync(pickerOptions).ConfigureAwait(false);
    }

    public static async Task<IStorageFolder?> OpenFolderAsync(
        Visual? owner = null,
        SkyFilePickerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var storage = ResolveStorageProvider(owner);
        if (storage is null || !storage.CanPickFolder)
            return null;

        var resolved = options ?? new SkyFilePickerOptions();
        var pickerOptions = new FolderPickerOpenOptions
        {
            Title = resolved.Title,
            AllowMultiple = false,
            SuggestedStartLocation = await ResolveFolderAsync(storage, resolved.SuggestedStartLocation, cancellationToken)
                .ConfigureAwait(false),
        };

        return await storage.OpenFolderPickerAsync(pickerOptions).ConfigureAwait(false) is { Count: > 0 } folders
            ? folders[0]
            : null;
    }

    private static IStorageProvider? ResolveStorageProvider(Visual? owner) =>
        SkyTopLevelResolver.Resolve(owner)?.StorageProvider;

    private static async Task<IStorageFolder?> ResolveFolderAsync(
        IStorageProvider storage,
        string? path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            return await storage.TryGetFolderFromPathAsync(path).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }
}
