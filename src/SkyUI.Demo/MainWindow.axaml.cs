using System.Collections.Generic;
using Avalonia.Controls;
using SkyUI.Demo.Models;
using SkyUI.Demo.Views.Demos;

namespace SkyUI.Demo;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _detailCache = new();

    public MainWindow()
    {
        InitializeComponent();

        NavList.ItemsSource = new DemoItem[]
        {
            new("Overview", () => new OverviewDemo()),
            new("Buttons", () => new ButtonsDemo()),
            new("Avatar", () => new AvatarDemo()),
            new("Chip", () => new ChipDemo()),
            new("Badge", () => new BadgeDemo()),
            new("Text field", () => new TextFieldDemo()),
            new("Checkbox & Switch", () => new CheckboxSwitchDemo()),
            new("Select", () => new SelectDemo()),
            new("List", () => new ListDemo()),
            new("Accordion", () => new AccordionDemo()),
            new("Placeholder", () => new PlaceholderDemo()),
            new("Diagram", () => new DiagramDemo()),
            new("CheckedListBox", () => new CheckedListBoxDemo()),
            new("Filter editor", () => new FilterEditorDemo()),
            new("Virtual DataGrid", () => new VirtualDataGridDemo()),
            new("Video timeline", () => new VideoTimelineDemo()),
        };

        NavList.SelectionChanged += (_, _) => ApplySelection();
        NavList.SelectedIndex = 0;
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (NavList.SelectedItem is not DemoItem item)
            return;

        if (!_detailCache.TryGetValue(item.Title, out var view))
        {
            view = item.CreateView();
            _detailCache[item.Title] = view;
        }

        DetailHost.Content = view;
    }
}
