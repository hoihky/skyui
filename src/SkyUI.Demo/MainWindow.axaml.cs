using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Demo.Models;
using SkyUI.Demo.Views.Demos;
using SkyUI.Icons;

namespace SkyUI.Demo;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> detailCache = new();

    public MainWindow()
    {
        InitializeComponent();

        NavList.ItemsSource = new DemoItem[]
        {
            new("Overview", () => new OverviewDemo(), SkyIconKind.Home),
            new("Typography", () => new TypographyDemo(), SkyIconKind.List),
            new("Icons", () => new IconsDemo(), SkyIconKind.Layers),
            new("Buttons", () => new ButtonsDemo(), SkyIconKind.Add),
            new("Avatar", () => new AvatarDemo(), SkyIconKind.Image),
            new("Chip", () => new ChipDemo(), SkyIconKind.Sliders),
            new("Badge", () => new BadgeDemo(), SkyIconKind.Check),
            new("Feedback", () => new FeedbackDemo(), SkyIconKind.Layers),
            new("Navigation", () => new NavigationDemo(), SkyIconKind.LayoutGrid),
            new("App chrome", () => new AppChromeDemo(), SkyIconKind.LayoutGrid),
            new("App templates", () => new AppTemplatesDemo(), SkyIconKind.LayoutGrid),
            new("Forms", () => new FormsDemo(), SkyIconKind.Sliders),
            new("Menus", () => new MenuDemo(), SkyIconKind.List),
            new("Pickers", () => new PickersDemo(), SkyIconKind.LayoutGrid),
            new("Layout", () => new LayoutDemo(), SkyIconKind.LayoutGrid),
            new("Primitives", () => new PrimitivesDemo(), SkyIconKind.Layers),
            new("Text field", () => new TextFieldDemo(), SkyIconKind.Search),
            new("Checkbox & Switch", () => new CheckboxSwitchDemo(), SkyIconKind.Check),
            new("Select", () => new SelectDemo(), SkyIconKind.ChevronDown),
            new("List", () => new ListDemo(), SkyIconKind.List),
            new("Accordion", () => new AccordionDemo(), SkyIconKind.Layers),
            new("Placeholder", () => new PlaceholderDemo(), SkyIconKind.LayoutGrid),
            new("Diagram", () => new DiagramDemo(), SkyIconKind.Layers),
            new("CheckedListBox", () => new CheckedListBoxDemo(), SkyIconKind.Check),
            new("Filter editor", () => new FilterEditorDemo(), SkyIconKind.Filter),
            new("Virtual DataGrid", () => new VirtualDataGridDemo(), SkyIconKind.Table),
            new("Video timeline", () => new VideoTimelineDemo(), SkyIconKind.Video),
        };

        NavList.SelectionChanged += (_, _) => ApplySelection();
        NavList.SelectedIndex = 0;
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (NavList.SelectedItem is not DemoItem item)
            return;

        if (!detailCache.TryGetValue(item.Title, out var view))
        {
            view = item.CreateView();
            detailCache[item.Title] = view;
        }

        DetailHost.Content = view;
    }

    private void OnOpenSettingsDrawer(object? sender, RoutedEventArgs e) =>
        SettingsDrawer.Show();
}
