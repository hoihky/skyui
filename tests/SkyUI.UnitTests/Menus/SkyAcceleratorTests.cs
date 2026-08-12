using Avalonia.Input;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyAcceleratorTests
{
    [Fact]
    public void Format_returns_empty_for_null() =>
        Assert.Equal(string.Empty, SkyAccelerator.Format(null));

    [Fact]
    public void Format_includes_key_name()
    {
        var text = SkyAccelerator.Format(SkyMenuGestures.Save);
        Assert.Contains("S", text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Ctrl+S", Key.S, KeyModifiers.Control)]
    [InlineData("Alt+F4", Key.F4, KeyModifiers.Alt)]
    [InlineData("Ctrl+Shift+Z", Key.Z, KeyModifiers.Control | KeyModifiers.Shift)]
    [InlineData("Delete", Key.Delete, KeyModifiers.None)]
    public void Parse_roundtrips_common_gestures(string text, Key key, KeyModifiers modifiers)
    {
        var gesture = SkyAccelerator.Parse(text);
        Assert.NotNull(gesture);
        Assert.Equal(key, gesture.Key);
        Assert.Equal(modifiers, gesture.KeyModifiers);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("NotAValidKey")]
    public void Parse_returns_null_for_invalid_input(string? text) =>
        Assert.Null(SkyAccelerator.Parse(text));

    [Fact]
    public void Equals_compares_key_and_modifiers()
    {
        var a = new KeyGesture(Key.C, KeyModifiers.Control);
        var b = new KeyGesture(Key.C, KeyModifiers.Control);
        var c = new KeyGesture(Key.V, KeyModifiers.Control);

        Assert.True(SkyAccelerator.Equals(a, b));
        Assert.False(SkyAccelerator.Equals(a, c));
        Assert.True(SkyAccelerator.Equals(null, null));
    }
}
