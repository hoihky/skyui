using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls;

/// <summary>Logical track lane (developer may bind multiple clips to the same track id).</summary>
public sealed class TimelineTrackItem : INotifyPropertyChanged
{
    private string _name = "Track";

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            _name = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Clip segment on a track (start in seconds, duration in seconds).</summary>
public sealed class TimelineClipItem : INotifyPropertyChanged
{
    private string _trackId = "";
    private double _startTime;
    private double _duration = 1;
    private string _label = "";

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string TrackId
    {
        get => _trackId;
        set
        {
            if (_trackId == value)
                return;
            _trackId = value;
            OnPropertyChanged();
        }
    }

    public double StartTime
    {
        get => _startTime;
        set
        {
            if (double.IsNaN(value))
                value = 0;
            if (Math.Abs(_startTime - value) < 1e-9)
                return;
            _startTime = value;
            OnPropertyChanged();
        }
    }

    public double Duration
    {
        get => _duration;
        set
        {
            if (double.IsNaN(value) || value < 0)
                value = 0;
            if (Math.Abs(_duration - value) < 1e-9)
                return;
            _duration = value;
            OnPropertyChanged();
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            if (_label == value)
                return;
            _label = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Point marker on the timeline (notes, chapter points, etc.).</summary>
public sealed class TimelineMarkerItem : INotifyPropertyChanged
{
    private double _time;
    private string _label = "";

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public double Time
    {
        get => _time;
        set
        {
            if (double.IsNaN(value))
                value = 0;
            if (Math.Abs(_time - value) < 1e-9)
                return;
            _time = value;
            OnPropertyChanged();
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            if (_label == value)
                return;
            _label = value;
            OnPropertyChanged();
        }
    }

    public object? Tag { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Inclusive-exclusive style range in seconds (either edge may be greater).</summary>
public readonly struct TimelineTimeRange
{
    public TimelineTimeRange(double a, double b)
    {
        Start = a;
        End = b;
    }

    public double Start { get; }
    public double End { get; }

    public double Min => Math.Min(Start, End);
    public double Max => Math.Max(Start, End);
    public double Length => Math.Max(0, Max - Min);
}
