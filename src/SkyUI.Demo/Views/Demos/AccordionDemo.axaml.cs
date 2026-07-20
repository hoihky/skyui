using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.Demo.Views.Demos;

public partial class AccordionDemo : UserControl
{
    public AccordionDemo()
    {
        InitializeComponent();
        ModeCombo.ItemsSource = new[]
        {
            SkyAccordionSelectionMode.Single,
            SkyAccordionSelectionMode.Multiple,
        };
        ModeCombo.SelectedItem = SkyAccordionSelectionMode.Multiple;
        ModeCombo.SelectionChanged += (_, _) =>
        {
            if (ModeCombo.SelectedItem is SkyAccordionSelectionMode m)
                Accordion.SelectionMode = m;
        };
    }
}
