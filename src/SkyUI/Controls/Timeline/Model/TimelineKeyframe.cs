using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Keyframe on a property lane (opacity, position, etc.).</summary>
public class TimelineKeyframe : INotifyPropertyChanged
{
    private string trackId = "";
    private string propertyName = "";
    private double time;
    private object? value;

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

    public string PropertyName
    {
        get => propertyName;
        set
        {
            if (propertyName == value)
                return;
            propertyName = value;
            OnPropertyChanged();
        }
    }

    public double Time
    {
        get => time;
        set
        {
            if (double.IsNaN(value))
                value = 0;
            if (Math.Abs(time - value) < 1e-9)
                return;
            time = value;
            OnPropertyChanged();
        }
    }

    public object? Value
    {
        get => value;
        set
        {
            if (Equals(value, this.value))
                return;
            this.value = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
