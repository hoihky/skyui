using Avalonia.Input;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyMenuGesturesTests
{
    [Theory]
    [InlineData(nameof(SkyMenuGestures.New), Key.N, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Open), Key.O, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Save), Key.S, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Copy), Key.C, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Paste), Key.V, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Undo), Key.Z, KeyModifiers.Control)]
    [InlineData(nameof(SkyMenuGestures.Delete), Key.Delete, KeyModifiers.None)]
    public void Standard_gestures_use_expected_keys(string propertyName, Key key, KeyModifiers modifiers)
    {
        var gesture = (KeyGesture)typeof(SkyMenuGestures).GetProperty(propertyName)!.GetValue(null)!;
        Assert.Equal(key, gesture.Key);
        Assert.Equal(modifiers, gesture.KeyModifiers);
    }
}
