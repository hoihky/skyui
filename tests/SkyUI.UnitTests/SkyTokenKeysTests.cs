using System.Reflection;
using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyTokenKeysTests
{
    [Theory]
    [MemberData(nameof(AllBrushKeys))]
    public void Brush_keys_are_non_empty(string key) =>
        Assert.False(string.IsNullOrWhiteSpace(key));

    public static IEnumerable<object[]> AllBrushKeys() =>
        typeof(SkyTokenKeys.Brush)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, FieldType: var t } && t == typeof(string))
            .Select(f => new object[] { (string)f.GetRawConstantValue()! });
}
