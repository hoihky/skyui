using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.Demo.ViewModels;

public sealed class CommandSurfacesDemoViewModel : INotifyPropertyChanged
{
    private string status = "Interact with command surfaces below.";

    public CommandSurfacesDemoViewModel()
    {
        PaletteItems = new ObservableCollection<SkyCommandPaletteItem>
        {
            new()
            {
                Title = "New document",
                Subtitle = "Create a blank file",
                IconKind = SkyIconKind.Add,
                Command = new RelayCommand(() => Status = "Palette: New document"),
                Keywords = "create file",
            },
            new()
            {
                Title = "Save",
                Subtitle = "Persist changes",
                IconKind = SkyIconKind.Check,
                Command = new RelayCommand(() => Status = "Palette: Save"),
                Keywords = "disk export",
            },
        };

        ExportMenuItems =
        [
            new SkyDropDownMenuItem { Label = "Export PDF", Command = new RelayCommand(() => Status = "Exported PDF") },
            new SkyDropDownMenuItem { Label = "Export CSV", Command = new RelayCommand(() => Status = "Exported CSV") },
        ];

        OpenPaletteCommand = new RelayCommand(() => Palette?.Open());
        PushNotificationCommand = new RelayCommand(() =>
        {
            NotificationCenter?.Push("Build succeeded", "All tests passed.");
            Status = "Notification pushed";
        });
    }

    public ObservableCollection<SkyCommandPaletteItem> PaletteItems { get; }

    public ObservableCollection<SkyDropDownMenuItem> ExportMenuItems { get; }

    public ICommand OpenPaletteCommand { get; }

    public ICommand PushNotificationCommand { get; }

    public ICommand SavePrimaryCommand { get; } = new RelayCommand(() => { });

    public SkyCommandPalette? Palette { get; set; }

    public SkyNotificationCenter? NotificationCenter { get; set; }

    public string Status
    {
        get => status;
        private set
        {
            if (status == value)
                return;
            status = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand : ICommand
    {
        private readonly Action execute;

        public RelayCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
