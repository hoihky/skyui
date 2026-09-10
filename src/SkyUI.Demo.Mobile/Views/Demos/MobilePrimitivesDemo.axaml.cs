using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using SkyUI.Controls;
using SkyUI.Demo.Mobile.Views;

namespace SkyUI.Demo.Mobile.Views.Demos;

public partial class MobilePrimitivesDemo : UserControl
{
    public MobilePrimitivesDemo()
    {
        InitializeComponent();

        SkyTouchTarget.SetEnsureTouchTarget(TouchButton, true);
        ActionSheetButton.Click += OnActionSheetClick;
        SheetButton.Click += OnSheetClick;
    }

    private MobileAppView? FindShell() =>
        this.GetVisualAncestors().OfType<MobileAppView>().FirstOrDefault();

    private async void OnActionSheetClick(object? sender, RoutedEventArgs e)
    {
        var shell = FindShell();
        if (shell is null)
            return;

        var index = await SkyActionSheet.ShowAsync([
            new SkyActionSheetItem { Title = "Share" },
            new SkyActionSheetItem { Title = "Rename" },
            new SkyActionSheetItem { Title = "Delete", IsDestructive = true },
        ], title: "Item actions", sheetHost: shell.SheetHost);

        ActionSheetResult.Text = index switch
        {
            null => "Cancelled",
            0 => "Share selected",
            1 => "Rename selected",
            2 => "Delete selected",
            _ => $"Selected index {index}",
        };
    }

    private void OnSheetClick(object? sender, RoutedEventArgs e)
    {
        var shell = FindShell();
        if (shell is null)
            return;

        shell.SheetHost.Title = "Filters";
        shell.SheetHost.SheetContent = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = "Status", FontWeight = FontWeight.SemiBold },
                new ComboBox
                {
                    Classes = { "sky" },
                    ItemsSource = new[] { "All", "Active", "Archived" },
                    SelectedIndex = 0,
                },
                new Button
                {
                    Classes = { "sky", "sky-primary" },
                    Content = "Apply",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                },
            },
        };
        shell.SheetHost.Show();
    }
}
