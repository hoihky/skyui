using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Input;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for menu commands and accelerators.</summary>
public sealed class MenuDemoViewModel : INotifyPropertyChanged
{
    private string _status = "Use the menu bar, context menu, or flyout button.";

    public MenuDemoViewModel()
    {
        NewCommand = new RelayCommand(() => SetStatus("New"));
        OpenCommand = new RelayCommand(() => SetStatus("Open"));
        SaveCommand = new RelayCommand(() => SetStatus("Save"));
        CopyCommand = new RelayCommand(() => SetStatus("Copy"));
        PasteCommand = new RelayCommand(() => SetStatus("Paste"));
        DeleteCommand = new RelayCommand(() => SetStatus("Delete"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;
            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand DeleteCommand { get; }

    public KeyGesture NewGesture => SkyMenuGestures.New;
    public KeyGesture OpenGesture => SkyMenuGestures.Open;
    public KeyGesture SaveGesture => SkyMenuGestures.Save;
    public KeyGesture CopyGesture => SkyMenuGestures.Copy;
    public KeyGesture PasteGesture => SkyMenuGestures.Paste;
    public KeyGesture DeleteGesture => SkyMenuGestures.Delete;

    private void SetStatus(string action) =>
        Status = $"{action} ({SkyAccelerator.Format(GetGesture(action))})";

    private KeyGesture? GetGesture(string action) => action switch
    {
        "New" => NewGesture,
        "Open" => OpenGesture,
        "Save" => SaveGesture,
        "Copy" => CopyGesture,
        "Paste" => PasteGesture,
        "Delete" => DeleteGesture,
        _ => null
    };

    private sealed class RelayCommand(Action execute) : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
