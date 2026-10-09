using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Professional;

/// <summary>MVVM-friendly row model for <see cref="SkyPropertyGrid"/>.</summary>
public sealed class PropertyGridItem : INotifyPropertyChanged
{
    private object? value;
    private string name = "";
    private string category = "General";
    private bool isReadOnly;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => name;
        set => SetField(ref name, value);
    }

    public string Category
    {
        get => category;
        set => SetField(ref category, value);
    }

    public Type ValueType { get; set; } = typeof(string);

    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetField(ref isReadOnly, value);
    }

    public string? Description { get; set; }

    public object? Value
    {
        get => value;
        set => SetField(ref this.value, value);
    }

    private void SetField<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return;
        field = newValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
