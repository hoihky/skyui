namespace SkyUI.Controls.Timeline.Model;

/// <summary>Track lookup helpers for editing policies and composition.</summary>
public sealed class TimelineTrackCatalog
{
    private readonly TimelineProject project;

    public TimelineTrackCatalog(TimelineProject project) => this.project = project;

    public TimelineTrack? FindById(string? trackId)
    {
        if (string.IsNullOrEmpty(trackId))
            return null;
        foreach (var track in project.Tracks)
        {
            if (track.Id == trackId)
                return track;
        }

        return null;
    }

    public bool IsTrackLocked(string? trackId) => FindById(trackId)?.IsLocked == true;

    public bool IsTrackVisible(string? trackId) => FindById(trackId)?.IsVisible != false;

    public IReadOnlyList<TimelineTrack> SpriteTracksInOrder()
    {
        var list = new List<TimelineTrack>();
        foreach (var track in project.Tracks)
        {
            if (track.Kind == TimelineTrackKind.Sprite)
                list.Add(track);
        }

        return list;
    }
}
