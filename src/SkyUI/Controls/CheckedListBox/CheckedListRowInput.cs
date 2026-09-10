using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

internal static class CheckedListRowInput
{
    public static bool IsInteractiveSource(object? source, Visual rowBorder)
    {
        if (source is not Visual visual)
            return false;

        for (var current = visual; current is not null && !ReferenceEquals(current, rowBorder); current = current.GetVisualParent())
        {
            if (current is CheckBox or Button or TextBox)
                return true;
        }

        return false;
    }
}
