using Avalonia.Media;

namespace SkyUI.Controls.Professional;

/// <summary>Look-and-feel settings for <see cref="SkyStepper"/> nodes and connectors.</summary>
public sealed class SkyStepperChrome
{
    public double ConnectorLength { get; set; } = 72;

    public double ConnectorThickness { get; set; } = 2;

    public double NodeDiameter { get; set; } = 28;

    public double StepColumnWidth { get; set; } = 128;

    public IBrush? PendingNodeBorderBrush { get; set; }

    public IBrush? ActiveNodeBorderBrush { get; set; }

    public IBrush? CompleteNodeBorderBrush { get; set; }

    public IBrush? PendingNodeBackgroundBrush { get; set; }

    public IBrush? ActiveNodeBackgroundBrush { get; set; }

    public IBrush? CompleteNodeBackgroundBrush { get; set; }

    public IBrush? PendingConnectorBrush { get; set; }

    public IBrush? CompleteConnectorBrush { get; set; }

    public IBrush? ActiveTitleForeground { get; set; }

    public IBrush? CompleteTitleForeground { get; set; }

    public IBrush? PendingTitleForeground { get; set; }

    public IBrush? ActiveSubtitleForeground { get; set; }

    public IBrush? CompleteSubtitleForeground { get; set; }

    public IBrush? PendingSubtitleForeground { get; set; }
}
