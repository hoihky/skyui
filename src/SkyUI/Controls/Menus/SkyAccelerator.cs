using Avalonia.Controls.Converters;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>
/// Cross-platform keyboard accelerator formatting and parsing for menus and commands.
/// </summary>
public static class SkyAccelerator
{
    /// <summary>Formats a gesture for display (e.g. Ctrl+S on Windows, ⌘S on macOS).</summary>
    public static string Format(KeyGesture? gesture) =>
        gesture is null ? string.Empty : PlatformKeyGestureConverter.ToPlatformString(gesture);

    /// <summary>
    /// Parses a gesture string such as <c>Ctrl+S</c>, <c>Alt+F4</c>, or <c>Shift+Delete</c>.
    /// </summary>
    public static KeyGesture? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return null;

        var modifiers = KeyModifiers.None;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            modifiers |= parts[i].ToUpperInvariant() switch
            {
                "CTRL" or "CONTROL" => KeyModifiers.Control,
                "ALT" => KeyModifiers.Alt,
                "SHIFT" => KeyModifiers.Shift,
                "CMD" or "META" or "COMMAND" => KeyModifiers.Meta,
                _ => KeyModifiers.None
            };
        }

        var keyToken = parts[^1];
        if (!Enum.TryParse(keyToken, ignoreCase: true, out Key key))
            return null;

        return new KeyGesture(key, modifiers);
    }

    /// <summary>Returns true when both gestures represent the same key and modifiers.</summary>
    public static bool Equals(KeyGesture? left, KeyGesture? right) =>
        left is null ? right is null : right is not null && left.Key == right.Key && left.KeyModifiers == right.KeyModifiers;
}
