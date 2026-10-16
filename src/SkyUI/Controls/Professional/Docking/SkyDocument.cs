using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyUI.Controls.Professional;

/// <summary>Default MVVM-friendly document model for <see cref="SkyDocumentWell"/>.</summary>
public sealed class SkyDocument : ISkyDocument, INotifyPropertyChanged
{
    private string title = "Untitled";
    private bool isDirty;
    private object? content;
    private bool canClose = true;
    private SkyDocumentShellPlacement shellPlacement = SkyDocumentShellPlacement.Central;
    private SkyDocumentShellPlacement shellDockBack = SkyDocumentShellPlacement.Central;
    private double shellFloatLeft = double.NaN;
    private double shellFloatTop = double.NaN;

    public SkyDocument() => Id = Guid.NewGuid();

    public SkyDocument(string title, object? content = null)
        : this()
    {
        this.title = title;
        this.content = content;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; }

    public string Title
    {
        get => title;
        set => SetField(ref title, value);
    }

    public bool IsDirty
    {
        get => isDirty;
        set => SetField(ref isDirty, value);
    }

    public object? Content
    {
        get => content;
        set => SetField(ref content, value);
    }

    public bool CanClose
    {
        get => canClose;
        set => SetField(ref canClose, value);
    }

    public SkyDocumentShellPlacement ShellPlacement
    {
        get => shellPlacement;
        set => SetField(ref shellPlacement, value);
    }

    public SkyDocumentShellPlacement ShellDockBack
    {
        get => shellDockBack;
        set => SetField(ref shellDockBack, value);
    }

    public double ShellFloatLeft
    {
        get => shellFloatLeft;
        set => SetField(ref shellFloatLeft, value);
    }

    public double ShellFloatTop
    {
        get => shellFloatTop;
        set => SetField(ref shellFloatTop, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
