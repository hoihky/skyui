namespace SkyUI.Samples.CrudListDetail.Models;

public sealed class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Company { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum CustomerStatus
{
    Active,
    Prospect,
    Inactive,
}
