using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using SkyUI.Controls;
using SkyUI.Demo.Mobile.Views.Demos;
using SkyUI.Icons;

namespace SkyUI.Demo.Mobile.Views;

public partial class MobileAppView : UserControl
{
    private readonly Dictionary<string, Control> pageCache = new();

    public MobileAppView()
    {
        InitializeComponent();

        SkyActionSheet.Attach(ActionSheetHost);
        SkyMessageBox.Attach(DialogHost);

        ShellNav.Items.Add(CreateNavItem("Primitives", SkyIconKind.Layers, () => new MobilePrimitivesDemo()));
        ShellNav.Items.Add(CreateNavItem("Navigation", SkyIconKind.LayoutGrid, () => new MobileNavigationDemo()));
        ShellNav.Items.Add(CreateNavItem("Forms", SkyIconKind.Sliders, () => new MobileFormsDemo()));
        ShellNav.Items.Add(CreateNavItem("Feedback", SkyIconKind.Check, () => new MobileFeedbackDemo()));

        ShellNav.SelectionChanged += (_, _) => ShowSelectedPage();
        ShellNav.SelectedIndex = 0;
        ShowSelectedPage();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is { } topLevel)
            TopLevel.SetAutoSafeAreaPadding(topLevel, false);
    }

    public SkySheetHost SheetHost => ActionSheetHost;

    public SkySnackbarHost SnackbarHostControl => SnackbarHost;

    private static SkyNavigationViewItem CreateNavItem(string label, SkyIconKind icon, Func<Control> factory) =>
        new()
        {
            Label = label,
            IconKind = icon,
            Tag = factory,
        };

    private void ShowSelectedPage()
    {
        if (ShellNav.SelectedItem is not SkyNavigationViewItem item || item.Tag is not Func<Control> factory)
            return;

        var key = item.Label ?? string.Empty;
        if (!pageCache.TryGetValue(key, out var page))
        {
            page = factory();
            pageCache[key] = page;
        }

        ShellNav.Content = page;
    }
}
