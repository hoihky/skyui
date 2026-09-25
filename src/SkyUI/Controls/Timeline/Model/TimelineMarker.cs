using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Point marker on the timeline ruler.</summary>
public class TimelineMarker : INotifyPropertyChanged
{
    private double time;
    private string label = "";

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
