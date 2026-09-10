namespace SkyUI.Samples.SettingsApp.Models;

/// <summary>Aggregate settings persisted by the sample application.</summary>
public sealed class SettingsDocument
{
    public ProfileSettings Profile { get; set; } = new();

    public AppearanceSettings Appearance { get; set; } = new();

    public NotificationSettings Notifications { get; set; } = new();
}

public sealed class ProfileSettings
{
    public string DisplayName { get; set; } = "SkyUI User";

    public string Email { get; set; } = "user@example.com";

    public string Bio { get; set; } = string.Empty;
}

public sealed class AppearanceSettings
{
    public string ThemeVariant { get; set; } = "Dark";

    public string Density { get; set; } = "Comfortable";

    public string AccentHex { get; set; } = "#1ED760";
}

public sealed class NotificationSettings
{
    public bool ProductUpdates { get; set; } = true;

    public bool SecurityAlerts { get; set; } = true;

    public bool WeeklyDigest { get; set; }
}
