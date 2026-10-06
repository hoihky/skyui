using Avalonia.Media;
using Avalonia.Threading;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Integration;

namespace SkyUI.Controls.Timeline.Thumbnails;

/// <summary>Loads clip thumbnails through a provider with metadata-aware caching.</summary>
public sealed class TimelineClipThumbnailCoordinator : IDisposable
{
    private readonly TimelineClipThumbnailCache cache = new();
    private readonly TimelineSpriteMetadataFingerprint fingerprint = new();
    private readonly Dictionary<string, int> requestGeneration = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CancellationTokenSource> activeLoads = new(StringComparer.Ordinal);

    public event EventHandler<TimelineClipThumbnailReadyEventArgs>? ThumbnailReady;

    public void InvalidateClip(string clipId)
    {
        cache.RemoveClip(clipId);
        requestGeneration[clipId] = requestGeneration.GetValueOrDefault(clipId) + 1;
        CancelLoad(clipId);
    }

    public void Clear()
    {
        foreach (var clipId in activeLoads.Keys.ToList())
            CancelLoad(clipId);
        cache.Clear();
        requestGeneration.Clear();
    }

    public void Dispose() => Clear();

    public void Request(TimelineClipItem clip, ITimelineClipThumbnailProvider? provider)
    {
        var metadataFingerprint = fingerprint.Compute(clip);
        if (provider is null)
        {
            RaiseReady(clip.Id, metadataFingerprint, null);
            return;
        }

        var key = new TimelineClipThumbnailCacheKey(clip.Id, metadataFingerprint);
        if (cache.TryGet(key, out var cached) && cached is not null)
        {
            RaiseReady(clip.Id, metadataFingerprint, cached);
            return;
        }

        var generation = requestGeneration.GetValueOrDefault(clip.Id);
        CancelLoad(clip.Id);
        var cts = new CancellationTokenSource();
        activeLoads[clip.Id] = cts;
        _ = LoadAsync(clip, provider, key, metadataFingerprint, generation, cts.Token);
    }

    private async Task LoadAsync(
        TimelineClipItem clip,
        ITimelineClipThumbnailProvider provider,
        TimelineClipThumbnailCacheKey key,
        string metadataFingerprint,
        int generation,
        CancellationToken cancellationToken)
    {
        IImage? image = null;
        try
        {
            image = await provider.GetThumbnailAsync(clip, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch
        {
            image = null;
        }

        if (cancellationToken.IsCancellationRequested
            || requestGeneration.GetValueOrDefault(clip.Id) != generation)
        {
            ReleaseIfDisposable(image);
            return;
        }

        if (image is not null)
            cache.Store(key, image);

        var clipId = clip.Id;
        Dispatcher.UIThread.Post(() =>
        {
            if (requestGeneration.GetValueOrDefault(clipId) != generation)
            {
                ReleaseIfDisposable(image);
                return;
            }

            RaiseReady(clipId, metadataFingerprint, image);
        });
    }

    private void RaiseReady(string clipId, string metadataFingerprint, IImage? image) =>
        ThumbnailReady?.Invoke(this, new TimelineClipThumbnailReadyEventArgs(clipId, metadataFingerprint, image));

    private void CancelLoad(string clipId)
    {
        if (!activeLoads.TryGetValue(clipId, out var cts))
            return;
        activeLoads.Remove(clipId);
        try
        {
            cts.Cancel();
        }
        finally
        {
            cts.Dispose();
        }
    }

    private static void ReleaseIfDisposable(IImage? image)
    {
        if (image is IDisposable disposable)
            disposable.Dispose();
    }
}
