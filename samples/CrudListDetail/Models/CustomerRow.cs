namespace SkyUI.Samples.CrudListDetail.Models;

/// <summary>Grid row projection (adapter DTO for virtual grid binding).</summary>
public sealed class CustomerRow
{
    public Guid Id { get; init; }

    public string Company { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public static CustomerRow FromCustomer(Customer customer) => new()
    {
        Id = customer.Id,
        Company = customer.Company,
        ContactName = customer.ContactName,
        Email = customer.Email,
        Status = customer.Status.ToString(),
    };
}
