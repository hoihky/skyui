using Avalonia.Media;

namespace SkyUI.Controls.Timeline.Thumbnails;

public sealed class TimelineClipThumbnailCache
{
    private readonly Dictionary<TimelineClipThumbnailCacheKey, IImage> entries = new();

    public bool TryGet(TimelineClipThumbnailCacheKey key, out IImage? image)
    {
        if (entries.TryGetValue(key, out image))
            return true;
        image = null;
        return false;
    }

    public void Store(TimelineClipThumbnailCacheKey key, IImage image)
    {
        if (entries.TryGetValue(key, out var previous))
            Release(previous);
        entries[key] = image;
    }

    public void RemoveClip(string clipId)
    {
        foreach (var key in entries.Keys.Where(k => k.ClipId == clipId).ToList())
        {
            if (entries.Remove(key, out var image))
                Release(image);
        }
    }

    public void Clear()
    {
        foreach (var image in entries.Values)
            Release(image);
        entries.Clear();
    }

    private static void Release(IImage? image)
    {
        if (image is IDisposable disposable)
            disposable.Dispose();
    }
}
