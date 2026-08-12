using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Search text field with leading icon styling.</summary>
public class SkySearchBox : TextBox
{
    public SkySearchBox()
    {
        Classes.Add("sky");
        Classes.Add("sky-search");
    }
}
