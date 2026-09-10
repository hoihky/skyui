using SkyUI.Samples.Infrastructure.Mvvm;
using SkyUI.Samples.Infrastructure.Navigation;
using SkyUI.Samples.SettingsApp.Models;

namespace SkyUI.Samples.SettingsApp.ViewModels;

public sealed class NotificationSettingsViewModel : ViewModelBase, INavigationPage
{
    private bool productUpdates;
    private bool securityAlerts;
    private bool weeklyDigest;

    public string Title => "Notifications";

    public bool ProductUpdates
    {
        get => productUpdates;
        set => SetProperty(ref productUpdates, value);
    }

    public bool SecurityAlerts
    {
        get => securityAlerts;
        set => SetProperty(ref securityAlerts, value);
    }

    public bool WeeklyDigest
    {
        get => weeklyDigest;
        set => SetProperty(ref weeklyDigest, value);
    }

    public void LoadFrom(NotificationSettings settings)
    {
        ProductUpdates = settings.ProductUpdates;
        SecurityAlerts = settings.SecurityAlerts;
        WeeklyDigest = settings.WeeklyDigest;
    }

    public void ApplyTo(NotificationSettings settings)
    {
        settings.ProductUpdates = ProductUpdates;
        settings.SecurityAlerts = SecurityAlerts;
        settings.WeeklyDigest = WeeklyDigest;
    }
}
