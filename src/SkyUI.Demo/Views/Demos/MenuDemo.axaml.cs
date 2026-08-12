using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class MenuDemo : UserControl
{
    public MenuDemo()
    {
        InitializeComponent();
        DataContext = new MenuDemoViewModel();
    }
}
