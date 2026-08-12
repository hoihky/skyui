using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class PrimitivesDemo : UserControl
{
    public PrimitivesDemo()
    {
        InitializeComponent();
        DataContext = new PrimitivesDemoViewModel();
    }
}
