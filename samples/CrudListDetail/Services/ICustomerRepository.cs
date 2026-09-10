using SkyUI.Samples.CrudListDetail.Models;

namespace SkyUI.Samples.CrudListDetail.Services;

/// <summary>Customer persistence abstraction (repository pattern).</summary>
public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> ListAsync(string? searchQuery, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
