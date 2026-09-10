using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.Samples.SettingsApp.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    public SkySnackbarHost SnackbarHost => SnackbarHostControl;
}
