using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Rendering;

internal sealed class TimelineTrackHeaderChromeBuilder
{
    private readonly TimelineTrackHeaderFormatter nameFormatter = new();

    public Border Build(TimelineInteractionContext ctx, TimelineTrack track)
    {
        var name = new TextBlock
        {
            Text = nameFormatter.FormatNameOnly(track),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = ctx.Control.Foreground,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        var visibility = CreateAffordance(
            ctx,
            TimelineTrackHeaderTags.Visibility(track.Id),
            track.IsVisible ? "Vis" : "Hide",
            track.IsVisible ? 1 : 0.45);
        var locked = CreateAffordance(
            ctx,
            TimelineTrackHeaderTags.Lock(track.Id),
            track.IsLocked ? "Lock" : "Edit",
            track.IsLocked ? 1 : 0.55);
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        Grid.SetColumn(name, 0);
        Grid.SetColumn(visibility, 1);
        Grid.SetColumn(locked, 2);
        grid.Children.Add(name);
        grid.Children.Add(visibility);
        grid.Children.Add(locked);
        var border = new Border
        {
            Height = TimelineRenderMetrics.TrackHeight,
            Padding = new Thickness(8, 0, 4, 0),
            Background = ctx.Control.ClipLaneBrush,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = grid,
            Tag = track.Id,
        };
        return border;
    }

    private static Border CreateAffordance(
        TimelineInteractionContext ctx,
        string tag,
        string text,
        double opacity)
    {
        return new Border
        {
            Padding = new Thickness(4, 2, 4, 2),
            Margin = new Thickness(2, 0, 0, 0),
            CornerRadius = new CornerRadius(3),
            Background = Brushes.Transparent,
            Opacity = opacity,
            Child = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = ctx.Control.Foreground,
            },
            Tag = tag,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
    }
}

internal static class TimelineTrackHeaderTags
{
    public static string Visibility(string trackId) => $"vis:{trackId}";

    public static string Lock(string trackId) => $"lock:{trackId}";

    public static bool TryParse(string tag, out string kind, out string trackId)
    {
        var parts = tag.Split(':', 2);
        if (parts.Length != 2)
        {
            kind = "";
            trackId = "";
            return false;
        }

        kind = parts[0];
        trackId = parts[1];
        return true;
    }
}
