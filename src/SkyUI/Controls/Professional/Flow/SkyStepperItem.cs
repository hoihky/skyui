using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Professional;

public sealed class SkyStepperItem : INotifyPropertyChanged
{
    private string title = "";
    private string? subtitle;
    private bool isComplete;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title
    {
        get => title;
        set => SetField(ref title, value);
    }

    public string? Subtitle
    {
        get => subtitle;
        set => SetField(ref subtitle, value);
    }

    public bool IsComplete
    {
        get => isComplete;
        set => SetField(ref isComplete, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
