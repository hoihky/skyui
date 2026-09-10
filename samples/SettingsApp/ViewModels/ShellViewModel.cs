using System.Collections.ObjectModel;
using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Samples.Infrastructure.Mvvm;
using SkyUI.Samples.Infrastructure.Navigation;
using SkyUI.Samples.SettingsApp.Models;
using SkyUI.Samples.SettingsApp.Services;

namespace SkyUI.Samples.SettingsApp.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly ISettingsStore settingsStore;
    private readonly ISnackbarNotifier snackbarNotifier;
    private readonly ProfileSettingsViewModel profilePage;
    private readonly AppearanceSettingsViewModel appearancePage;
    private readonly NotificationSettingsViewModel notificationPage;
    private SettingsDocument document = new();
    private INavigationPage? currentPage;
    private int selectedNavigationIndex;

    public ShellViewModel(
        ISettingsStore settingsStore,
        ISnackbarNotifier snackbarNotifier,
        ProfileSettingsViewModel profilePage,
        AppearanceSettingsViewModel appearancePage,
        NotificationSettingsViewModel notificationPage)
    {
        this.settingsStore = settingsStore;
        this.snackbarNotifier = snackbarNotifier;
        this.profilePage = profilePage;
        this.appearancePage = appearancePage;
        this.notificationPage = notificationPage;

        NavigationPages = new ReadOnlyCollection<INavigationPage>(
            [profilePage, appearancePage, notificationPage]);

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ResetCommand = new AsyncRelayCommand(ResetAsync);
    }

    public ReadOnlyCollection<INavigationPage> NavigationPages { get; }

    public INavigationPage? CurrentPage
    {
        get => currentPage;
        private set => SetProperty(ref currentPage, value);
    }

    public int SelectedNavigationIndex
    {
        get => selectedNavigationIndex;
        set
        {
            if (!SetProperty(ref selectedNavigationIndex, value))
                return;

            CurrentPage = value >= 0 && value < NavigationPages.Count
                ? NavigationPages[value]
                : null;
        }
    }

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    public async void Initialize()
    {
        document = await settingsStore.LoadAsync().ConfigureAwait(true);
        ApplyDocumentToPages();
        SelectedNavigationIndex = 0;
    }

    private async Task SaveAsync()
    {
        if (!profilePage.Validate())
        {
            SelectedNavigationIndex = 0;
            snackbarNotifier.Show("Fix validation errors before saving.", SkyFeedbackVariant.Warning);
            return;
        }

        profilePage.ApplyTo(document.Profile);
        appearancePage.ApplyTo(document.Appearance);
        notificationPage.ApplyTo(document.Notifications);

        await settingsStore.SaveAsync(document).ConfigureAwait(true);
        snackbarNotifier.Show("Settings saved.", SkyFeedbackVariant.Success);
    }

    private async Task ResetAsync()
    {
        document = new SettingsDocument();
        ApplyDocumentToPages();
        await settingsStore.SaveAsync(document).ConfigureAwait(true);
        snackbarNotifier.Show("Settings reset to defaults.", SkyFeedbackVariant.Neutral);
    }

    private void ApplyDocumentToPages()
    {
        profilePage.LoadFrom(document.Profile);
        appearancePage.LoadFrom(document.Appearance);
        notificationPage.LoadFrom(document.Notifications);
    }
}
