namespace SkyUI.Controls;

/// <summary>Tracks selected clips and optional active track without UI dependencies.</summary>
public sealed class TimelineSelectionModel
{
    private readonly HashSet<string> clipIds = new(StringComparer.Ordinal);
    private string? selectedTrackId;

    public event EventHandler? SelectionChanged;

    public string? SelectedTrackId
    {
        get => selectedTrackId;
        set
        {
            if (selectedTrackId == value)
                return;
            selectedTrackId = value;
            RaiseChanged();
        }
    }

    public IReadOnlyCollection<string> SelectedClipIds => clipIds;

    public bool IsClipSelected(string clipId) => clipIds.Contains(clipId);

    public void SelectSingleClip(string clipId)
    {
        clipIds.Clear();
        selectedTrackId = null;
        clipIds.Add(clipId);
        RaiseChanged();
    }

    public void ToggleClip(string clipId)
    {
        if (!clipIds.Add(clipId))
            clipIds.Remove(clipId);
        RaiseChanged();
    }

    public void SetClipSelection(IEnumerable<string> ids)
    {
        clipIds.Clear();
        foreach (var id in ids)
            clipIds.Add(id);
        RaiseChanged();
    }

    public void ClearClipSelection()
    {
        if (clipIds.Count == 0)
            return;
        clipIds.Clear();
        RaiseChanged();
    }

    public void ClearAll()
    {
        clipIds.Clear();
        selectedTrackId = null;
        RaiseChanged();
    }

    public IReadOnlyList<TimelineClipItem> ResolveClips(IEnumerable<TimelineClipItem> clips) =>
        clips.Where(c => clipIds.Contains(c.Id)).ToList();

    private void RaiseChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);
}
