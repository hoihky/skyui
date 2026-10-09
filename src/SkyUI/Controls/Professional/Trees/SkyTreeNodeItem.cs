using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Professional;

public sealed class SkyTreeNodeItem : INotifyPropertyChanged
{
    private bool isExpanded;
    private bool isSelected;
    private string header = "";

    public SkyTreeNodeItem() => Children = new ObservableCollection<SkyTreeNodeItem>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Header
    {
        get => header;
        set => SetField(ref header, value);
    }

    public object? Tag { get; set; }

    public ObservableCollection<SkyTreeNodeItem> Children { get; }

    public bool IsExpanded
    {
        get => isExpanded;
        set => SetField(ref isExpanded, value);
    }

    public bool IsSelected
    {
        get => isSelected;
        set => SetField(ref isSelected, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
