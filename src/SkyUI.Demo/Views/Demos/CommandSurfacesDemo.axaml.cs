using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class CommandSurfacesDemo : UserControl
{
    public CommandSurfacesDemo()
    {
        InitializeComponent();
        var vm = new CommandSurfacesDemoViewModel();
        DataContext = vm;
        Loaded += (_, _) =>
        {
            vm.Palette = Palette;
            vm.NotificationCenter = NotificationHost;
        };
    }
}
