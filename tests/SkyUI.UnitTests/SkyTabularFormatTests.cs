using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyTabularFormatTests
{
    [Fact]
    public void ToTsv_formats_rows_with_tabs()
    {
        var text = SkyTabularFormat.ToTsv(
        [
            ["Index", "Label"],
            ["0", "Row 0"],
        ]);

        Assert.Equal($"Index\tLabel{Environment.NewLine}0\tRow 0", text);
    }

    [Fact]
    public void ToTsv_escapes_tabs_and_quotes()
    {
        var text = SkyTabularFormat.ToTsv([["a\tb", "say \"hi\""]]);
        Assert.Equal("\"a\tb\"\t\"say \"\"hi\"\"\"", text);
    }

    [Fact]
    public void TryParseTsv_round_trips_simple_grid()
    {
        IReadOnlyList<IReadOnlyList<string?>> original =
        [
            new string?[] { "Index", "Label" },
            new string?[] { "1", "Row 1" },
        ];

        var parsed = SkyTabularFormat.TryParseTsv(SkyTabularFormat.ToTsv(original));
        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Count);
        Assert.Equal(["Index", "Label"], parsed[0]);
        Assert.Equal(["1", "Row 1"], parsed[1]);
    }
}
