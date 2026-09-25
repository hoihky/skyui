namespace SkyUI.Controls;

/// <summary>Configuration for magnetic snapping while editing clips.</summary>
public sealed class TimelineSnapSettings
{
    public bool IsEnabled { get; set; } = true;

    public bool SnapToPlayhead { get; set; } = true;

    public bool SnapToMarkers { get; set; } = true;

    public bool SnapToClipEdges { get; set; } = true;

    public bool SnapToGrid { get; set; } = true;

    /// <summary>Grid interval in seconds when <see cref="SnapToGrid"/> is true.</summary>
    public double GridIntervalSeconds { get; set; } = 0.25;

    /// <summary>Maximum distance (seconds) at which snap engages.</summary>
    public double SnapThresholdSeconds { get; set; } = 0.12;
}
