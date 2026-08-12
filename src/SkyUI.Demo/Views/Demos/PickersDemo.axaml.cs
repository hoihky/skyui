using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class PickersDemo : UserControl
{
    public PickersDemo()
    {
        InitializeComponent();
        DataContext = new PickersDemoViewModel();
    }
}
