using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for the Forms demo page.</summary>
public sealed class FormsDemoViewModel : INotifyPropertyChanged
{
    private string email = string.Empty;
    private string? emailError;
    private string searchQuery = string.Empty;
    private string password = string.Empty;
    private object? theme = "dark";
    private double volume = 60;
    private string? cityText;
    private object? selectedCity;
    private object? selectedRole;
    private decimal quantity = 1;
    private DateTime? rangeStart;
    private DateTime? rangeEnd;
    private SkyDateRangePreset rangePreset;
    private string phoneRaw = string.Empty;
    private bool showEmptyState = true;

    public FormsDemoViewModel()
    {
        ValidateEmailCommand = new RelayCommand(ValidateEmail);
        ClearErrorsCommand = new RelayCommand(ClearErrors);
        ToggleEmptyStateCommand = new RelayCommand(() => ShowEmptyState = !ShowEmptyState);
        CreateItemCommand = new RelayCommand(() => ShowEmptyState = false);

        EmailValidator = new CompositeSkyValidator(
            SkyValidators.Required("Email is required."),
            SkyValidators.Email());

        CitySuggestions = new ObservableCollection<string>(
            ["Seattle", "San Francisco", "Austin", "Boston", "Chicago"]);
        RoleOptions = new ObservableCollection<string>(["Viewer", "Editor", "Admin"]);
        CityProvider = new CityAutocompleteProvider(CitySuggestions);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Email
    {
        get => email;
        set => SetField(ref email, value);
    }

    public string? EmailError
    {
        get => emailError;
        set => SetField(ref emailError, value);
    }

    public string SearchQuery
    {
        get => searchQuery;
        set => SetField(ref searchQuery, value);
    }

    public string Password
    {
        get => password;
        set => SetField(ref password, value);
    }

    public object? Theme
    {
        get => theme;
        set => SetField(ref theme, value);
    }

    public double Volume
    {
        get => volume;
        set => SetField(ref volume, value);
    }

    public string? CityText
    {
        get => cityText;
        set => SetField(ref cityText, value);
    }

    public object? SelectedCity
    {
        get => selectedCity;
        set => SetField(ref selectedCity, value);
    }

    public object? SelectedRole
    {
        get => selectedRole;
        set => SetField(ref selectedRole, value);
    }

    public decimal Quantity
    {
        get => quantity;
        set => SetField(ref quantity, value);
    }

    public DateTime? RangeStart
    {
        get => rangeStart;
        set => SetField(ref rangeStart, value);
    }

    public DateTime? RangeEnd
    {
        get => rangeEnd;
        set => SetField(ref rangeEnd, value);
    }

    public SkyDateRangePreset RangePreset
    {
        get => rangePreset;
        set => SetField(ref rangePreset, value);
    }

    public string PhoneRaw
    {
        get => phoneRaw;
        set => SetField(ref phoneRaw, value);
    }

    public bool ShowEmptyState
    {
        get => showEmptyState;
        set => SetField(ref showEmptyState, value);
    }

    public ObservableCollection<string> CitySuggestions { get; }

    public ObservableCollection<string> RoleOptions { get; }

    public ISkyAutocompleteProvider CityProvider { get; }

    public ISkyValidator EmailValidator { get; }

    public ICommand ValidateEmailCommand { get; }

    public ICommand ClearErrorsCommand { get; }

    public ICommand ToggleEmptyStateCommand { get; }

    public ICommand CreateItemCommand { get; }

    private void ValidateEmail()
    {
        var result = EmailValidator.Validate(Email);
        EmailError = result.IsValid ? null : result.ErrorMessage;
    }

    private void ClearErrors() => EmailError = null;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }

    private sealed class CityAutocompleteProvider(IEnumerable<string> cities) : ISkyAutocompleteProvider
    {
        public Task<IReadOnlyList<SkyAutocompleteItem>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var results = cities
                .Where(city => city.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(city => new SkyAutocompleteItem(city, city))
                .ToList();

            return Task.FromResult<IReadOnlyList<SkyAutocompleteItem>>(results);
        }
    }
}
