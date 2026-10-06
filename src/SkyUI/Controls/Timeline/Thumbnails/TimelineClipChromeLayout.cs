using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.Controls.Timeline.Thumbnails;

/// <summary>Visual parts of a clip body used for thumbnails and labels.</summary>
public sealed class TimelineClipChromeLayout
{
    public TimelineClipChromeLayout(Image thumbnail, TextBlock label)
    {
        Thumbnail = thumbnail;
        Label = label;
        Root = BuildRoot(thumbnail, label);
    }

    public Image Thumbnail { get; }

    public TextBlock Label { get; }

    public Grid Root { get; }

    private static Grid BuildRoot(Image thumbnail, TextBlock label)
    {
        thumbnail.Width = TimelineRenderMetrics.ClipThumbnailWidth;
        thumbnail.Height = TimelineRenderMetrics.ClipThumbnailHeight;
        thumbnail.Stretch = Stretch.UniformToFill;
        thumbnail.IsHitTestVisible = false;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
        };
        Grid.SetColumn(thumbnail, 0);
        Grid.SetColumn(label, 1);
        grid.Children.Add(thumbnail);
        grid.Children.Add(label);
        return grid;
    }
}
