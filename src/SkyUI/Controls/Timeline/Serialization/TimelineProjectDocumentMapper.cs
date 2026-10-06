using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Serialization;

public sealed class TimelineProjectDocumentMapper
{
    public TimelineProjectDocument ToDocument(TimelineProject project)
    {
        var doc = new TimelineProjectDocument
        {
            Duration = project.Duration,
            Fps = project.Fps,
            TimeUnit = project.TimeUnit,
        };

        foreach (var track in project.Tracks)
        {
            doc.Tracks.Add(new TimelineTrackDocument
            {
                Id = track.Id,
                Name = track.Name,
                Kind = track.Kind,
                IsVisible = track.IsVisible,
                IsLocked = track.IsLocked,
                AccentColor = track.AccentColor,
            });
        }

        foreach (var clip in project.Clips)
        {
            var clipDoc = new TimelineClipDocument
            {
                Id = clip.Id,
                TrackId = clip.TrackId,
                StartTime = clip.StartTime,
                Duration = clip.Duration,
                Label = clip.Label,
            };
            if (clip.Sprite is not null)
                clipDoc.Sprite = ToSpriteDocument(clip.Sprite);
            doc.Clips.Add(clipDoc);
        }

        foreach (var marker in project.Markers)
        {
            doc.Markers.Add(new TimelineMarkerDocument
            {
                Id = marker.Id,
                Time = marker.Time,
                Label = marker.Label,
            });
        }

        foreach (var keyframe in project.Keyframes)
        {
            var keyDoc = new TimelineKeyframeDocument
            {
                Id = keyframe.Id,
                TrackId = keyframe.TrackId,
                PropertyName = keyframe.PropertyName,
                Time = keyframe.Time,
            };
            if (keyframe.Value is double d)
                keyDoc.NumericValue = d;
            else if (keyframe.Value is not null)
                keyDoc.TextValue = keyframe.Value.ToString();
            doc.Keyframes.Add(keyDoc);
        }

        return doc;
    }

    public void ApplyToProject(TimelineProjectDocument document, TimelineProject project)
    {
        project.Duration = document.Duration;
        project.Fps = document.Fps;
        project.TimeUnit = document.TimeUnit;

        project.Tracks.Clear();
        foreach (var trackDoc in document.Tracks)
        {
            project.Tracks.Add(new TimelineTrackItem
            {
                Id = string.IsNullOrEmpty(trackDoc.Id) ? Guid.NewGuid().ToString("N") : trackDoc.Id,
                Name = trackDoc.Name,
                Kind = trackDoc.Kind,
                IsVisible = trackDoc.IsVisible,
                IsLocked = trackDoc.IsLocked,
                AccentColor = trackDoc.AccentColor,
            });
        }

        project.Clips.Clear();
        foreach (var clipDoc in document.Clips)
        {
            var clip = new TimelineClipItem
            {
                Id = string.IsNullOrEmpty(clipDoc.Id) ? Guid.NewGuid().ToString("N") : clipDoc.Id,
                TrackId = clipDoc.TrackId,
                StartTime = clipDoc.StartTime,
                Duration = clipDoc.Duration,
                Label = clipDoc.Label,
            };
            if (clipDoc.Sprite is not null)
                clip.Sprite = FromSpriteDocument(clipDoc.Sprite);
            project.Clips.Add(clip);
        }

        project.Markers.Clear();
        foreach (var markerDoc in document.Markers)
        {
            project.Markers.Add(new TimelineMarkerItem
            {
                Id = string.IsNullOrEmpty(markerDoc.Id) ? Guid.NewGuid().ToString("N") : markerDoc.Id,
                Time = markerDoc.Time,
                Label = markerDoc.Label,
            });
        }

        project.Keyframes.Clear();
        foreach (var keyDoc in document.Keyframes)
        {
            object? value = keyDoc.NumericValue.HasValue
                ? keyDoc.NumericValue.Value
                : keyDoc.TextValue;
            project.Keyframes.Add(new TimelineKeyframeItem
            {
                Id = string.IsNullOrEmpty(keyDoc.Id) ? Guid.NewGuid().ToString("N") : keyDoc.Id,
                TrackId = keyDoc.TrackId,
                PropertyName = keyDoc.PropertyName,
                Time = keyDoc.Time,
                Value = value,
            });
        }
    }

    private static TimelineSpriteClipDocument ToSpriteDocument(TimelineSpriteClipMetadata sprite) =>
        new()
        {
            AtlasId = sprite.AtlasId,
            SpriteName = sprite.SpriteName,
            FrameIndex = sprite.FrameIndex,
            HoldFrames = sprite.HoldFrames,
            HoldLastCel = sprite.HoldLastCel,
            HasSourceRect = sprite.HasSourceRect,
            SourceX = sprite.SourceRect.X,
            SourceY = sprite.SourceRect.Y,
            SourceWidth = sprite.SourceRect.Width,
            SourceHeight = sprite.SourceRect.Height,
        };

    private static TimelineSpriteClipMetadata FromSpriteDocument(TimelineSpriteClipDocument sprite) =>
        new()
        {
            AtlasId = sprite.AtlasId,
            SpriteName = sprite.SpriteName,
            FrameIndex = sprite.FrameIndex,
            HoldFrames = sprite.HoldFrames,
            HoldLastCel = sprite.HoldLastCel,
            HasSourceRect = sprite.HasSourceRect,
            SourceRect = new TimelineSpriteSourceRect(
                sprite.SourceX,
                sprite.SourceY,
                sprite.SourceWidth,
                sprite.SourceHeight),
        };
}
