using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.VisualTree;
using SkyUI.Controls.Professional;

namespace SkyUI.HeadlessTests;

public class ProfessionalThemedHeadlessTests
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.GetOrStartForAssembly(typeof(ProfessionalThemedHeadlessTests).Assembly);

    [Fact]
    public void SkyPropertyGrid_applies_theme_template_and_renders_categories()
    {
        Session.Dispatch(() =>
        {
            var grid = new SkyPropertyGrid
            {
                Width = 480,
                Height = 320,
                Items = new ObservableCollection<PropertyGridItem>
                {
                    new() { Name = "Title", Category = "General", Value = "Demo", ValueType = typeof(string) },
                    new() { Name = "Enabled", Category = "General", Value = true, ValueType = typeof(bool) },
                },
            };

            var window = Show(grid);
            var titles = grid.GetVisualDescendants().OfType<TextBlock>()
                .Select(t => t.Text)
                .Where(t => t is "General" or "Title" or "Enabled")
                .ToList();

            Assert.Contains("General", titles);
            Assert.Contains("Title", titles);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyStepper_applies_theme_template_with_steps_host()
    {
        Session.Dispatch(() =>
        {
            var stepper = new SkyStepper
            {
                Width = 640,
                Height = 160,
                Steps = new ObservableCollection<SkyStepperItem>
                {
                    new() { Title = "One", Subtitle = "First" },
                    new() { Title = "Two", Subtitle = "Second" },
                },
            };

            var window = Show(stepper);
            var labels = stepper.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
            Assert.Contains("One", labels);
            Assert.Contains("Two", labels);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyRichTextBox_live_preview_renders_formatted_inlines()
    {
        Session.Dispatch(() =>
        {
            var editor = new SkyRichTextBox
            {
                Width = 420,
                Height = 360,
                ShowLivePreview = true,
                Text = "Hello **World**",
            };

            var window = Show(editor);
            var preview = editor.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(t => t.Name == SkyRichTextBox.PreviewPartName);
            Assert.NotNull(preview);
            Assert.Equal(2, preview!.Inlines.Count);

            var bold = Assert.IsType<Run>(preview.Inlines[1]);
            Assert.Equal("World", bold.Text);
            Assert.Equal(FontWeight.SemiBold, bold.FontWeight);

            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyTreeView_applies_theme_and_lists_nodes()
    {
        Session.Dispatch(() =>
        {
            var tree = new SkyTreeView
            {
                Width = 320,
                Height = 200,
                Items = new ObservableCollection<SkyTreeNodeItem>
                {
                    new()
                    {
                        Header = "Root",
                        IsExpanded = true,
                        Children = { new SkyTreeNodeItem { Header = "Child" } },
                    },
                },
            };

            var window = Show(tree);
            var labels = tree.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
            Assert.Contains("Root", labels);
            Assert.Contains("Child", labels);
            window.Close();
        }, CancellationToken.None);
    }

    private static Window Show(Control content)
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = content,
        };
        window.Show();
        content.Measure(new Size(content.Width, content.Height));
        content.Arrange(new Rect(0, 0, content.Width, content.Height));
        return window;
    }
}
