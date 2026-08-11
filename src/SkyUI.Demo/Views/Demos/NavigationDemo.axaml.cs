using Avalonia.Controls;

namespace SkyUI.Demo.Views.Demos;

public partial class NavigationDemo : UserControl
{
    public NavigationDemo()
    {
        InitializeComponent();
        PreviewWidthSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Slider.ValueProperty)
                PreviewHost.Width = PreviewWidthSlider.Value;
        };
        PreviewHost.Width = PreviewWidthSlider.Value;
    }
}
