using SkyUI.Samples.CrudListDetail.Models;

namespace SkyUI.Samples.CrudListDetail.Services;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> customers = CreateSeedData();
    private readonly Lock sync = new();

    public Task<IReadOnlyList<Customer>> ListAsync(string? searchQuery, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            IEnumerable<Customer> query = customers;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim();
                query = query.Where(customer =>
                    customer.Company.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || customer.ContactName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            IReadOnlyList<Customer> snapshot = query
                .OrderBy(customer => customer.Company)
                .ToList();

            return Task.FromResult(snapshot);
        }
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            return Task.FromResult(customers.FirstOrDefault(customer => customer.Id == id));
        }
    }

    public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            customers.Add(customer);
            return Task.FromResult(customer);
        }
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            var index = customers.FindIndex(existing => existing.Id == customer.Id);
            if (index >= 0)
                customers[index] = customer;

            return Task.CompletedTask;
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            customers.RemoveAll(customer => customer.Id == id);
            return Task.CompletedTask;
        }
    }

    private static List<Customer> CreateSeedData()
    {
        var statuses = new[] { CustomerStatus.Active, CustomerStatus.Prospect, CustomerStatus.Inactive };
        var companies = new[]
        {
            "Northwind Traders", "Contoso Ltd", "Fabrikam Inc", "Adventure Works", "Tailspin Toys",
            "Blue Yonder Airlines", "Litware Inc", "Wingtip Toys", "Proseware Inc", "Margie's Travel",
        };

        var list = new List<Customer>(250);
        for (var index = 0; index < 250; index++)
        {
            var company = companies[index % companies.Length];
            list.Add(new Customer
            {
                Company = $"{company} #{index + 1}",
                ContactName = $"Contact {index + 1}",
                Email = $"contact{index + 1}@example.com",
                Status = statuses[index % statuses.Length],
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-index),
            });
        }

        return list;
    }
}
