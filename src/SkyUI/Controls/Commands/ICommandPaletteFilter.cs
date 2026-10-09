namespace SkyUI.Controls;

/// <summary>Strategy for matching palette items against user search text.</summary>
public interface ICommandPaletteFilter
{
    bool Matches(SkyCommandPaletteItem item, string? searchText);
}
