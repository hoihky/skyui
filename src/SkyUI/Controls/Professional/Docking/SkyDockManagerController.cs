using System.Collections;
using System.Linq;
using Avalonia;

namespace SkyUI.Controls.Professional;

/// <summary>Dock layout rules decoupled from the visual tree (testable).</summary>
public sealed class SkyDockManagerController
{
    private IEnumerable? toolPanes;

    public IEnumerable? ToolPanes
    {
        get => toolPanes;
        set => toolPanes = value;
    }

    public IReadOnlyList<SkyDockPane> GetPanesInZone(SkyDockZone zone) =>
        Snapshot()
            .Where(p => p.Zone == zone && p.IsVisible)
            .OrderBy(p => p.Order)
            .ThenBy(p => p.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public IReadOnlyList<SkyDockPane> GetFloatingPanes() => GetPanesInZone(SkyDockZone.Float);

    public bool HasVisibleToolsInZone(SkyDockZone zone) =>
        zone != SkyDockZone.Document && GetPanesInZone(zone).Count > 0;

    public SkyDockPane? ResolveSelectedPane(SkyDockZone zone)
    {
        var panes = GetPanesInZone(zone);
        if (panes.Count == 0)
            return null;
        return panes.FirstOrDefault(p => p.IsSelected) ?? panes[0];
    }

    public void EnsureSelection(SkyDockZone zone)
    {
        var panes = GetPanesInZone(zone);
        if (panes.Count == 0)
            return;
        if (panes.Any(p => p.IsSelected))
            return;
        panes[0].IsSelected = true;
    }

    public void SelectPane(SkyDockPane pane)
    {
        ArgumentNullException.ThrowIfNull(pane);
        foreach (var candidate in Snapshot().Where(p => p.Zone == pane.Zone))
            candidate.IsSelected = ReferenceEquals(candidate, pane);
    }

    public bool TryAddPane(SkyDockPane pane, IList? mutableList)
    {
        ArgumentNullException.ThrowIfNull(pane);
        if (mutableList is null)
            return false;
        if (Snapshot().Any(p => p.Id == pane.Id))
            return false;
        mutableList.Add(pane);
        return true;
    }

    public bool TryRemovePane(SkyDockPane pane, IList? mutableList)
    {
        ArgumentNullException.ThrowIfNull(pane);
        if (mutableList is null)
            return false;
        for (var i = 0; i < mutableList.Count; i++)
        {
            if (mutableList[i] is SkyDockPane candidate && candidate.Id == pane.Id)
            {
                mutableList.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public void FloatPane(SkyDockPane pane, Point? floatPosition = null)
    {
        ArgumentNullException.ThrowIfNull(pane);
        if (pane.Zone != SkyDockZone.Float)
            pane.DockBackZone = pane.Zone is SkyDockZone.Document or SkyDockZone.Float
                ? SkyDockZone.Left
                : pane.Zone;
        pane.Zone = SkyDockZone.Float;
        pane.Kind = SkyDockPaneKind.Float;
        pane.IsSelected = true;
        if (floatPosition is not null)
        {
            pane.FloatLeft = floatPosition.Value.X;
            pane.FloatTop = floatPosition.Value.Y;
        }
    }

    public void DockPane(SkyDockPane pane, SkyDockZone targetZone)
    {
        ArgumentNullException.ThrowIfNull(pane);
        if (targetZone == SkyDockZone.Document)
            throw new ArgumentException("Document zone is reserved for SkyDocumentWell.", nameof(targetZone));
        if (targetZone == SkyDockZone.Float)
        {
            FloatPane(pane);
            return;
        }

        pane.Zone = targetZone;
        pane.Kind = SkyDockPaneKind.Tool;
        pane.DockBackZone = targetZone;
        SelectPane(pane);
    }

    public void ApplyDrop(SkyDockPane pane, SkyDockZone targetZone, Point dropPointInManager)
    {
        ArgumentNullException.ThrowIfNull(pane);
        if (targetZone == SkyDockZone.Float)
            FloatPane(pane, dropPointInManager);
        else if (pane.Zone != targetZone)
            DockPane(pane, targetZone);
        else
            SelectPane(pane);
    }

    public void DockBack(SkyDockPane pane) => DockPane(pane, pane.DockBackZone);

    public SkyDockLayoutState CaptureLayout(double leftWidth, double rightWidth, double bottomHeight)
    {
        var entries = Snapshot()
            .Where(p => p.Zone != SkyDockZone.Document)
            .OrderBy(p => p.Zone)
            .ThenBy(p => p.Order)
            .Select(p => new SkyDockPaneLayoutEntry
            {
                Id = p.Id,
                Title = p.Title,
                Zone = p.Zone,
                Kind = p.Kind,
                PreferredLength = p.PreferredLength,
                IsVisible = p.IsVisible,
                Order = p.Order,
                DockBackZone = p.DockBackZone,
                FloatLeft = p.FloatLeft,
                FloatTop = p.FloatTop,
                FloatWidth = p.FloatWidth,
                FloatHeight = p.FloatHeight,
            })
            .ToList();

        return new SkyDockLayoutState
        {
            LeftToolPaneWidth = leftWidth,
            RightToolPaneWidth = rightWidth,
            BottomToolPaneHeight = bottomHeight,
            ToolPanes = entries,
        };
    }

    public void ApplyPaneMetadata(SkyDockLayoutState state, IList? mutableList)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (mutableList is null)
            return;

        var existing = Snapshot().ToDictionary(p => p.Id);
        foreach (var entry in state.ToolPanes)
        {
            if (existing.TryGetValue(entry.Id, out var pane))
            {
                pane.Title = entry.Title;
                pane.Zone = entry.Zone;
                pane.Kind = entry.Kind;
                pane.PreferredLength = entry.PreferredLength;
                pane.IsVisible = entry.IsVisible;
                pane.Order = entry.Order;
                pane.DockBackZone = entry.DockBackZone;
                pane.FloatLeft = entry.FloatLeft;
                pane.FloatTop = entry.FloatTop;
                pane.FloatWidth = entry.FloatWidth;
                pane.FloatHeight = entry.FloatHeight;
            }
            else
            {
                mutableList.Add(new SkyDockPane(entry.Id, entry.Title, null, entry.Zone)
                {
                    Kind = entry.Kind,
                    PreferredLength = entry.PreferredLength,
                    IsVisible = entry.IsVisible,
                    Order = entry.Order,
                    DockBackZone = entry.DockBackZone,
                    FloatLeft = entry.FloatLeft,
                    FloatTop = entry.FloatTop,
                    FloatWidth = entry.FloatWidth,
                    FloatHeight = entry.FloatHeight,
                });
            }
        }
    }

    private IEnumerable<SkyDockPane> Snapshot()
    {
        if (toolPanes is null)
            yield break;
        foreach (var item in toolPanes)
        {
            if (item is SkyDockPane pane)
                yield return pane;
        }
    }
}
