using System.Windows.Input;
using SkyUI.Controls;
using SkyUI.Samples.CrudListDetail.Models;
using SkyUI.Samples.CrudListDetail.Services;
using SkyUI.Samples.Infrastructure.Mvvm;

namespace SkyUI.Samples.CrudListDetail.ViewModels;

public sealed class CustomerDetailViewModel : ViewModelBase
{
    private readonly ICustomerRepository repository;
    private readonly ISnackbarNotifier snackbarNotifier;
    private Guid? customerId;
    private string company = string.Empty;
    private string contactName = string.Empty;
    private string email = string.Empty;
    private CustomerStatus status = CustomerStatus.Active;
    private bool isNew;
    private string? emailError;

    public CustomerDetailViewModel(ICustomerRepository repository, ISnackbarNotifier snackbarNotifier)
    {
        this.repository = repository;
        this.snackbarNotifier = snackbarNotifier;

        EmailValidator = new CompositeSkyValidator(
            SkyValidators.Required("Email is required."),
            SkyValidators.Email());

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => HasSelection || IsNew);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => HasSelection && !IsNew);
        NewCommand = new RelayCommand(BeginNew);
    }

    public bool HasSelection => customerId.HasValue;

    public bool IsNew
    {
        get => isNew;
        private set
        {
            if (!SetProperty(ref isNew, value))
                return;

            RaiseCanExecuteChanged();
        }
    }

    public string Company
    {
        get => company;
        set => SetProperty(ref company, value);
    }

    public string ContactName
    {
        get => contactName;
        set => SetProperty(ref contactName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public CustomerStatus Status
    {
        get => status;
        set => SetProperty(ref status, value);
    }

    public string? EmailError
    {
        get => emailError;
        set => SetProperty(ref emailError, value);
    }

    public ISkyValidator EmailValidator { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand NewCommand { get; }

    public event EventHandler? CustomerChanged;

    public async Task LoadCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(true);
        if (customer is null)
        {
            Clear();
            return;
        }

        customerId = customer.Id;
        IsNew = false;
        Company = customer.Company;
        ContactName = customer.ContactName;
        Email = customer.Email;
        Status = customer.Status;
        EmailError = null;
        RaisePropertyChanged(nameof(HasSelection));
        RaiseCanExecuteChanged();
    }

    public void Clear()
    {
        customerId = null;
        IsNew = false;
        Company = string.Empty;
        ContactName = string.Empty;
        Email = string.Empty;
        Status = CustomerStatus.Active;
        EmailError = null;
        RaisePropertyChanged(nameof(HasSelection));
        RaiseCanExecuteChanged();
    }

    private void BeginNew()
    {
        customerId = null;
        IsNew = true;
        Company = string.Empty;
        ContactName = string.Empty;
        Email = string.Empty;
        Status = CustomerStatus.Prospect;
        EmailError = null;
        RaisePropertyChanged(nameof(HasSelection));
        RaiseCanExecuteChanged();
    }

    private bool Validate()
    {
        var result = EmailValidator.Validate(Email);
        EmailError = result.IsValid ? null : result.ErrorMessage;
        return result.IsValid && !string.IsNullOrWhiteSpace(Company);
    }

    private async Task SaveAsync()
    {
        if (!Validate())
        {
            snackbarNotifier.Show("Fix validation errors before saving.", SkyFeedbackVariant.Warning);
            return;
        }

        if (IsNew)
        {
            var draft = new Customer
            {
                Company = Company.Trim(),
                ContactName = ContactName.Trim(),
                Email = Email.Trim(),
                Status = Status,
            };

            var created = await repository.AddAsync(draft).ConfigureAwait(true);
            customerId = created.Id;
            IsNew = false;
            RaisePropertyChanged(nameof(HasSelection));
            snackbarNotifier.Show("Customer created.", SkyFeedbackVariant.Success);
            CustomerChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (customerId.HasValue)
        {
            var existing = await repository.GetByIdAsync(customerId.Value).ConfigureAwait(true);
            if (existing is null)
                return;

            existing.Company = Company.Trim();
            existing.ContactName = ContactName.Trim();
            existing.Email = Email.Trim();
            existing.Status = Status;

            await repository.UpdateAsync(existing).ConfigureAwait(true);
            snackbarNotifier.Show("Customer saved.", SkyFeedbackVariant.Success);
            CustomerChanged?.Invoke(this, EventArgs.Empty);
        }

        RaiseCanExecuteChanged();
    }

    private async Task DeleteAsync()
    {
        if (!customerId.HasValue)
            return;

        await repository.DeleteAsync(customerId.Value).ConfigureAwait(true);
        snackbarNotifier.Show("Customer deleted.", SkyFeedbackVariant.Neutral);
        CustomerChanged?.Invoke(this, EventArgs.Empty);
        Clear();
    }

    private void RaiseCanExecuteChanged()
    {
        (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
