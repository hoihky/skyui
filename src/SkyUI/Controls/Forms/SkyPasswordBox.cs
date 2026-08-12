using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Password text field with masking and optional reveal.</summary>
public class SkyPasswordBox : TextBox
{
    static SkyPasswordBox()
    {
        PasswordCharProperty.OverrideDefaultValue<SkyPasswordBox>('•');
    }

    public SkyPasswordBox()
    {
        Classes.Add("sky");
        Classes.Add("sky-password");
    }
}
