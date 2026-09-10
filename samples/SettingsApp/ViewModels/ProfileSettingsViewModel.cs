using SkyUI.Controls;
using SkyUI.Samples.Infrastructure.Mvvm;
using SkyUI.Samples.Infrastructure.Navigation;
using SkyUI.Samples.SettingsApp.Models;

namespace SkyUI.Samples.SettingsApp.ViewModels;

public sealed class ProfileSettingsViewModel : ViewModelBase, INavigationPage
{
    private string displayName = string.Empty;
    private string email = string.Empty;
    private string bio = string.Empty;
    private string? emailError;

    public ProfileSettingsViewModel()
    {
        EmailValidator = new CompositeSkyValidator(
            SkyValidators.Required("Email is required."),
            SkyValidators.Email());
    }

    public string Title => "Profile";

    public string DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string Bio
    {
        get => bio;
        set => SetProperty(ref bio, value);
    }

    public string? EmailError
    {
        get => emailError;
        set => SetProperty(ref emailError, value);
    }

    public ISkyValidator EmailValidator { get; }

    public void LoadFrom(ProfileSettings settings)
    {
        DisplayName = settings.DisplayName;
        Email = settings.Email;
        Bio = settings.Bio;
        EmailError = null;
    }

    public void ApplyTo(ProfileSettings settings)
    {
        settings.DisplayName = DisplayName;
        settings.Email = Email;
        settings.Bio = Bio;
    }

    public bool Validate()
    {
        var result = EmailValidator.Validate(Email);
        EmailError = result.IsValid ? null : result.ErrorMessage;
        return result.IsValid;
    }
}
