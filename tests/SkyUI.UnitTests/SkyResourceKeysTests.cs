using SkyUI.Core;

namespace SkyUI.UnitTests;

public class SkyResourceKeysTests
{
    [Fact]
    public void Accent_key_is_non_empty() =>
        Assert.False(string.IsNullOrWhiteSpace(SkyResourceKeys.Accent));
}
