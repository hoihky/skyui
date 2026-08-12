namespace SkyUI.Controls;

/// <summary>Option in <see cref="SkyRadioGroup"/>.</summary>
public class SkyRadioGroupItem
{
    public string? Label { get; set; }

    public object? Value { get; set; }

    public bool IsEnabled { get; set; } = true;
}
