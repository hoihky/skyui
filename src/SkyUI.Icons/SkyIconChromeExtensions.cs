namespace SkyUI.Icons;

public static class SkyIconChromeExtensions
{
    public static SkyIconSize ToIconSize(this SkyIconChrome chrome) =>
        chrome switch
        {
            SkyIconChrome.Button => SkyIconSize.Small,
            SkyIconChrome.Navigation => SkyIconSize.Medium,
            SkyIconChrome.ListItem => SkyIconSize.Medium,
            _ => SkyIconSize.Medium,
        };
}
