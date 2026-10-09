using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkyUI.Controls.Professional;

namespace SkyUI.Demo.ViewModels;

public sealed class ProfessionalDemoViewModel : INotifyPropertyChanged
{
    public ProfessionalDemoViewModel()
    {
        PropertyRows = new ObservableCollection<PropertyGridItem>
        {
            new() { Name = "Title", Category = "General", Value = "SkyUI demo", ValueType = typeof(string) },
            new() { Name = "Enabled", Category = "General", Value = true, ValueType = typeof(bool) },
            new() { Name = "Opacity", Category = "Appearance", Value = 0.9, ValueType = typeof(double) },
            new() { Name = "Accent", Category = "Appearance", Value = Color.Parse("#4FC3F7"), ValueType = typeof(Color) },
            new() { Name = "Version", Category = "About", Value = "0.1", ValueType = typeof(string), IsReadOnly = true },
        };
        Tags = new ObservableCollection<string> { "design", "avalonia", "mvvm" };
        TreeRoots = new ObservableCollection<SkyTreeNodeItem>
        {
            new()
            {
                Header = "Solution",
                IsExpanded = true,
                Children =
                {
                    new SkyTreeNodeItem { Header = "SkyUI.csproj" },
                    new SkyTreeNodeItem
                    {
                        Header = "Controls",
                        IsExpanded = true,
                        Children =
                        {
                            new SkyTreeNodeItem { Header = "Professional" },
                            new SkyTreeNodeItem { Header = "Forms" },
                        },
                    },
                },
            },
        };
        Steps = new ObservableCollection<SkyStepperItem>
        {
            new() { Title = "Account", Subtitle = "Sign in", IsComplete = true },
            new() { Title = "Profile", Subtitle = "Details" },
            new() { Title = "Confirm", Subtitle = "Review" },
        };
        WizardPages = new ObservableCollection<SkyWizardPage>
        {
            new() { Title = "Welcome", Content = "Step 1: Welcome to Sky wizard." },
            new() { Title = "Options", Content = "Step 2: Choose your preferences." },
            new() { Title = "Done", Content = "Step 3: Finish setup." },
        };
        PreviewImage = CreatePreviewImage();
        BoldCommand = new RelayCommand(() => RichTextTarget?.ApplyFormat(SkyRichTextFormat.Bold));
        ItalicCommand = new RelayCommand(() => RichTextTarget?.ApplyFormat(SkyRichTextFormat.Italic));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PropertyGridItem> PropertyRows { get; }

    public ObservableCollection<string> Tags { get; }

    public ObservableCollection<SkyTreeNodeItem> TreeRoots { get; }

    public ObservableCollection<SkyStepperItem> Steps { get; }

    public ObservableCollection<SkyWizardPage> WizardPages { get; }

    public IImage PreviewImage { get; }

    private Color pickerColor = Color.Parse("#81C784");
    private double rangeStart = 25;
    private double rangeEnd = 75;
    private string richText = "Select text and use **bold** from the toolbar.";
    private int stepperIndex;
    private int wizardIndex;

    public Color PickerColor
    {
        get => pickerColor;
        set => SetField(ref pickerColor, value);
    }

    public double RangeStart
    {
        get => rangeStart;
        set
        {
            if (SetField(ref rangeStart, value))
                OnPropertyChanged(nameof(RangeSummary));
        }
    }

    public double RangeEnd
    {
        get => rangeEnd;
        set
        {
            if (SetField(ref rangeEnd, value))
                OnPropertyChanged(nameof(RangeSummary));
        }
    }

    public string RangeSummary => $"Range: {RangeStart:0} – {RangeEnd:0}";

    public string RichText
    {
        get => richText;
        set => SetField(ref richText, value);
    }

    public int StepperIndex
    {
        get => stepperIndex;
        set => SetField(ref stepperIndex, value);
    }

    public int WizardIndex
    {
        get => wizardIndex;
        set => SetField(ref wizardIndex, value);
    }

    public SkyRichTextBox? RichTextTarget { get; set; }

    public ICommand BoldCommand { get; }

    public ICommand ItalicCommand { get; }

    private static DrawingImage CreatePreviewImage()
    {
        return new DrawingImage(new GeometryDrawing
        {
            Brush = new SolidColorBrush(Color.Parse("#3949AB")),
            Geometry = new RectangleGeometry(new Rect(0, 0, 160, 100)),
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
