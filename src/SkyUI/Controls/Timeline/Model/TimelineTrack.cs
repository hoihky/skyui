using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Logical track lane (clips reference <see cref="TimelineClip.TrackId"/>).</summary>
public class TimelineTrack : INotifyPropertyChanged
{
    private string name = "Track";
    private TimelineTrackKind kind = TimelineTrackKind.Standard;
    private bool isVisible = true;
    private bool isLocked;
    private string? accentColor;

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public TimelineTrackKind Kind
    {
        get => kind;
        set
        {
            if (kind == value)
                return;
            kind = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value)
                return;
            isVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsLocked
    {
        get => isLocked;
        set
        {
            if (isLocked == value)
                return;
            isLocked = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Optional CSS-style color (#RRGGBB or #AARRGGBB) for lane chrome.</summary>
    public string? AccentColor
    {
        get => accentColor;
        set
        {
            if (accentColor == value)
                return;
            accentColor = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => name;
        set
        {
            if (name == value)
                return;
            name = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
