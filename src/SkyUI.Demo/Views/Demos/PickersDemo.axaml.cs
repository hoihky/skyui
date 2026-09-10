using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class PickersDemo : UserControl
{
    public PickersDemo()
    {
        InitializeComponent();
        DataContext = new PickersDemoViewModel();
    }

    private async void OnOpenFileClick(object? sender, RoutedEventArgs e)
    {
        var file = await SkyFilePicker.OpenFileAsync(this, SkyFilePickerOptions.AllFiles("Select a file"));
        FilePickerResultText.Text = file is null ? "Open file cancelled." : $"Selected: {file.Path}";
    }

    private async void OnSaveFileClick(object? sender, RoutedEventArgs e)
    {
        var file = await SkyFilePicker.SaveFileAsync(
            this,
            new SkyFilePickerOptions
            {
                Title = "Save export",
                SuggestedFileName = "export.csv",
                FileTypes = SkyFilePickerOptions.CsvFiles().FileTypes,
            });

        FilePickerResultText.Text = file is null ? "Save file cancelled." : $"Save to: {file.Path}";
    }

    private async void OnOpenFolderClick(object? sender, RoutedEventArgs e)
    {
        var folder = await SkyFilePicker.OpenFolderAsync(this, new SkyFilePickerOptions { Title = "Select folder" });
        FilePickerResultText.Text = folder is null ? "Open folder cancelled." : $"Folder: {folder.Path}";
    }
}
