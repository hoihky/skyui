using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class FormsDemo : UserControl
{
    public FormsDemo()
    {
        InitializeComponent();
        DataContext = new FormsDemoViewModel();
    }
}
