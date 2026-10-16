using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Professional;

/// <summary>MVVM-friendly tool or auxiliary pane hosted by <see cref="SkyDockManager"/>.</summary>
public sealed class SkyDockPane : INotifyPropertyChanged
{
    private string title = "Tool";
    private SkyDockZone zone = SkyDockZone.Left;
    private SkyDockPaneKind kind = SkyDockPaneKind.Tool;
    private object? content;
    private bool isVisible = true;
    private bool isSelected;
    private double preferredLength = 260;
    private int order;
    private SkyDockZone dockBackZone = SkyDockZone.Left;
    private double floatLeft = double.NaN;
    private double floatTop = double.NaN;
    private double floatWidth = 320;
    private double floatHeight = 220;

    public SkyDockPane() => Id = Guid.NewGuid();

    public SkyDockPane(Guid id) => Id = id;

    public SkyDockPane(string title, object? content, SkyDockZone zone)
        : this()
    {
        this.title = title;
        this.content = content;
        this.zone = zone;
        kind = zone == SkyDockZone.Float ? SkyDockPaneKind.Float : SkyDockPaneKind.Tool;
    }

    public SkyDockPane(Guid id, string title, object? content, SkyDockZone zone)
        : this(id)
    {
        this.title = title;
        this.content = content;
        this.zone = zone;
        kind = zone == SkyDockZone.Float ? SkyDockPaneKind.Float : SkyDockPaneKind.Tool;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; }

    public string Title
    {
        get => title;
        set => SetField(ref title, value);
    }

    public SkyDockZone Zone
    {
        get => zone;
        set => SetField(ref zone, value);
    }

    public SkyDockPaneKind Kind
    {
        get => kind;
        set => SetField(ref kind, value);
    }

    public object? Content
    {
        get => content;
        set => SetField(ref content, value);
    }

    public bool IsVisible
    {
        get => isVisible;
        set => SetField(ref isVisible, value);
    }

    /// <summary>Active tab within the pane's <see cref="Zone"/> group.</summary>
    public bool IsSelected
    {
        get => isSelected;
        set => SetField(ref isSelected, value);
    }

    /// <summary>Default width (left/right) or height (bottom) for the zone chrome.</summary>
    public double PreferredLength
    {
        get => preferredLength;
        set => SetField(ref preferredLength, value);
    }

    public int Order
    {
        get => order;
        set => SetField(ref order, value);
    }

    /// <summary>Target zone for <see cref="SkyDockFloatWindow"/> dock-back affordance.</summary>
    public SkyDockZone DockBackZone
    {
        get => dockBackZone;
        set => SetField(ref dockBackZone, value);
    }

    public double FloatLeft
    {
        get => floatLeft;
        set => SetField(ref floatLeft, value);
    }

    public double FloatTop
    {
        get => floatTop;
        set => SetField(ref floatTop, value);
    }

    public double FloatWidth
    {
        get => floatWidth;
        set => SetField(ref floatWidth, value);
    }

    public double FloatHeight
    {
        get => floatHeight;
        set => SetField(ref floatHeight, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
