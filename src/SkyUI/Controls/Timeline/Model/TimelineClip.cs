using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Clip segment on a track (start and duration in timeline seconds).</summary>
public class TimelineClip : INotifyPropertyChanged
{
    private string trackId = "";
    private double startTime;
    private double duration = 1;
    private string label = "";
    private TimelineSpriteClipMetadata? sprite;

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string TrackId
    {
        get => trackId;
        set
        {
            if (trackId == value)
                return;
            trackId = value;
            OnPropertyChanged();
        }
    }

    public double StartTime
    {
        get => startTime;
        set
        {
            if (double.IsNaN(value))
                value = 0;
            if (Math.Abs(startTime - value) < 1e-9)
                return;
            startTime = value;
            OnPropertyChanged();
        }
    }

    public double Duration
    {
        get => duration;
        set
        {
            if (double.IsNaN(value) || value < 0)
                value = 0;
            if (Math.Abs(duration - value) < 1e-9)
                return;
            duration = value;
            OnPropertyChanged();
        }
    }

    public string Label
    {
        get => label;
        set
        {
            if (label == value)
                return;
            label = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public TimelineSpriteClipMetadata? Sprite
    {
        get => sprite;
        set
        {
            if (ReferenceEquals(sprite, value))
                return;
            DetachSpriteListener(sprite);
            sprite = value;
            AttachSpriteListener(sprite);
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void AttachSpriteListener(TimelineSpriteClipMetadata? metadata)
    {
        if (metadata is null)
            return;
        metadata.PropertyChanged += OnSpriteMetadataPropertyChanged;
    }

    private void DetachSpriteListener(TimelineSpriteClipMetadata? metadata)
    {
        if (metadata is null)
            return;
        metadata.PropertyChanged -= OnSpriteMetadataPropertyChanged;
    }

    private void OnSpriteMetadataPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(nameof(Sprite));
}
