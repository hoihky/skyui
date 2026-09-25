using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Logical track lane (clips reference <see cref="TimelineClip.TrackId"/>).</summary>
public class TimelineTrack : INotifyPropertyChanged
{
    private string name = "Track";

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

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
