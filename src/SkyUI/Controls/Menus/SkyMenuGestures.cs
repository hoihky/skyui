using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>Standard edit and file menu accelerators for MVVM menus and documentation.</summary>
public static class SkyMenuGestures
{
    public static KeyGesture New { get; } = new(Key.N, KeyModifiers.Control);
    public static KeyGesture Open { get; } = new(Key.O, KeyModifiers.Control);
    public static KeyGesture Save { get; } = new(Key.S, KeyModifiers.Control);
    public static KeyGesture SaveAs { get; } = new(Key.S, KeyModifiers.Control | KeyModifiers.Shift);
    public static KeyGesture Close { get; } = new(Key.W, KeyModifiers.Control);
    public static KeyGesture Quit { get; } = new(Key.Q, KeyModifiers.Control);

    public static KeyGesture Undo { get; } = new(Key.Z, KeyModifiers.Control);
    public static KeyGesture Redo { get; } = new(Key.Y, KeyModifiers.Control);
    public static KeyGesture Cut { get; } = new(Key.X, KeyModifiers.Control);
    public static KeyGesture Copy { get; } = new(Key.C, KeyModifiers.Control);
    public static KeyGesture Paste { get; } = new(Key.V, KeyModifiers.Control);
    public static KeyGesture SelectAll { get; } = new(Key.A, KeyModifiers.Control);
    public static KeyGesture Delete { get; } = new(Key.Delete);
    public static KeyGesture Find { get; } = new(Key.F, KeyModifiers.Control);
    public static KeyGesture ZoomIn { get; } = new(Key.OemPlus, KeyModifiers.Control);
    public static KeyGesture ZoomOut { get; } = new(Key.OemMinus, KeyModifiers.Control);
}
