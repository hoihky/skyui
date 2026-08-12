using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for the Forms demo page.</summary>
public sealed class FormsDemoViewModel : INotifyPropertyChanged
{
    private string _email = string.Empty;
    private string? _emailError;
    private string _searchQuery = string.Empty;
    private string _password = string.Empty;
    private object? _theme = "dark";
    private double _volume = 60;

    public FormsDemoViewModel()
    {
        ValidateEmailCommand = new RelayCommand(ValidateEmail);
        ClearErrorsCommand = new RelayCommand(ClearErrors);
        EmailValidator = new CompositeSkyValidator(
            SkyValidators.Required("Email is required."),
            SkyValidators.Email());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Email
    {
        get => _email;
        set => SetField(ref _email, value);
    }

    public string? EmailError
    {
        get => _emailError;
        set => SetField(ref _emailError, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetField(ref _searchQuery, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public object? Theme
    {
        get => _theme;
        set => SetField(ref _theme, value);
    }

    public double Volume
    {
        get => _volume;
        set => SetField(ref _volume, value);
    }

    public ISkyValidator EmailValidator { get; }

    public ICommand ValidateEmailCommand { get; }

    public ICommand ClearErrorsCommand { get; }

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
}
