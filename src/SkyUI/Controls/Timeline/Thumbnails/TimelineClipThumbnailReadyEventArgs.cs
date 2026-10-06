using Avalonia.Media;

namespace SkyUI.Controls.Timeline.Thumbnails;

public sealed class TimelineClipThumbnailReadyEventArgs : EventArgs
{
    public TimelineClipThumbnailReadyEventArgs(string clipId, string metadataFingerprint, IImage? image)
    {
        ClipId = clipId;
        MetadataFingerprint = metadataFingerprint;
        Image = image;
    }

    public string ClipId { get; }

    public string MetadataFingerprint { get; }

    public IImage? Image { get; }
}
