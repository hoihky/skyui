using Avalonia.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class ProfessionalDemo : UserControl
{
    public ProfessionalDemo()
    {
        InitializeComponent();
        var vm = new ProfessionalDemoViewModel();
        DataContext = vm;
        Loaded += (_, _) => vm.RichTextTarget = RichEditor;
    }
}
