using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Cel metadata for sprite clips (atlas frame, optional hold length).</summary>
public sealed class TimelineSpriteClipMetadata : INotifyPropertyChanged
{
    private string atlasId = "";
    private string spriteName = "";
    private int frameIndex;
    private int holdFrames = 1;
    private bool holdLastCel;
    private TimelineSpriteSourceRect sourceRect;
    private bool hasSourceRect;

    public string AtlasId
    {
        get => atlasId;
        set
        {
            if (atlasId == value)
                return;
            atlasId = value ?? "";
            OnPropertyChanged();
        }
    }

    public string SpriteName
    {
        get => spriteName;
        set
        {
            if (spriteName == value)
                return;
            spriteName = value ?? "";
            OnPropertyChanged();
        }
    }

    public int FrameIndex
    {
        get => frameIndex;
        set
        {
            if (frameIndex == value)
                return;
            frameIndex = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Optional explicit hold length in timeline frames (defaults to clip duration).</summary>
    public int HoldFrames
    {
        get => holdFrames;
        set
        {
            if (value < 1)
                value = 1;
            if (holdFrames == value)
                return;
            holdFrames = value;
            OnPropertyChanged();
        }
    }

    /// <summary>When true, preview holds the last cel for the clip duration (host interprets).</summary>
    public bool HoldLastCel
    {
        get => holdLastCel;
        set
        {
            if (holdLastCel == value)
                return;
            holdLastCel = value;
            OnPropertyChanged();
        }
    }

    public bool HasSourceRect
    {
        get => hasSourceRect;
        set
        {
            if (hasSourceRect == value)
                return;
            hasSourceRect = value;
            OnPropertyChanged();
        }
    }

    public TimelineSpriteSourceRect SourceRect
    {
        get => sourceRect;
        set
        {
            if (sourceRect.Equals(value))
                return;
            sourceRect = value;
            OnPropertyChanged();
        }
    }

    public TimelineSpriteClipMetadata Clone() =>
        new()
        {
            atlasId = atlasId,
            spriteName = spriteName,
            frameIndex = frameIndex,
            holdFrames = holdFrames,
            holdLastCel = holdLastCel,
            hasSourceRect = hasSourceRect,
            sourceRect = sourceRect,
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
