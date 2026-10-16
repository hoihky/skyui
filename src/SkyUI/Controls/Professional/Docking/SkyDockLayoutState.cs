namespace SkyUI.Controls.Professional;

/// <summary>Serializable layout snapshot (no document payloads).</summary>
public sealed class SkyDockLayoutState
{
    public double LeftToolPaneWidth { get; init; } = 260;

    public double RightToolPaneWidth { get; init; } = 280;

    public double BottomToolPaneHeight { get; init; } = 200;

    public IReadOnlyList<SkyDockPaneLayoutEntry> ToolPanes { get; init; } = Array.Empty<SkyDockPaneLayoutEntry>();

    public IReadOnlyList<SkyDocumentLayoutEntry> DocumentShell { get; init; } = Array.Empty<SkyDocumentLayoutEntry>();
}

public sealed class SkyDocumentLayoutEntry
{
    public Guid Id { get; init; }

    public SkyDocumentShellPlacement ShellPlacement { get; init; } = SkyDocumentShellPlacement.Central;

    public SkyDocumentShellPlacement ShellDockBack { get; init; } = SkyDocumentShellPlacement.Central;

    public double ShellFloatLeft { get; init; } = double.NaN;

    public double ShellFloatTop { get; init; } = double.NaN;
}

public sealed class SkyDockPaneLayoutEntry
{
    public Guid Id { get; init; }

    public string Title { get; init; } = "";

    public SkyDockZone Zone { get; init; }

    public SkyDockPaneKind Kind { get; init; }

    public double PreferredLength { get; init; }

    public bool IsVisible { get; init; } = true;

    public int Order { get; init; }

    public SkyDockZone DockBackZone { get; init; } = SkyDockZone.Left;

    public double FloatLeft { get; init; } = double.NaN;

    public double FloatTop { get; init; } = double.NaN;

    public double FloatWidth { get; init; } = 320;

    public double FloatHeight { get; init; } = 220;
}
