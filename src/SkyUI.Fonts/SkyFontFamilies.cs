namespace SkyUI.Fonts;

/// <summary>Font family URIs and system fallbacks (see <c>DESIGN.md</c> § Typography).</summary>
public static class SkyFontFamilies
{
    /// <summary>Inter (Latin) from <c>Avalonia.Fonts.Inter</c>.</summary>
    public const string Inter = "avares://Avalonia.Fonts.Inter/Assets#Inter";

    /// <summary>Noto Sans SC via <see cref="SkyNotoFontCollection"/>.</summary>
    public const string NotoSansSc = "fonts:SkyNoto#Noto Sans SC";

    /// <summary>Global script fallbacks aligned with DESIGN.md.</summary>
    public const string SystemFallbacks =
        "Helvetica Neue, Helvetica, Arial, PingFang SC, Hiragino Sans GB, Hiragino Kaku Gothic ProN, "
        + "Microsoft YaHei, Meiryo, MS Gothic, ui-sans-serif, sans-serif";

    /// <summary>UI / body stack: Inter → Noto SC → system.</summary>
    public const string Ui = Inter + ", " + NotoSansSc + ", " + SystemFallbacks;

    /// <summary>Title stack (Inter bold roles; same family chain as UI).</summary>
    public const string Title = Inter + ", " + NotoSansSc + ", " + SystemFallbacks;
}
