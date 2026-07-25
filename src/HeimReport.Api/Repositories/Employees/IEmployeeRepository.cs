using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Employees;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<Employee?> GetActiveByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedEmailAsync(
        string normalizedEmail, int? excludeId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNationalIdAndCountryAsync(
        string nationalId, int countryId, int? excludeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetManagerChainAsync(int startEmployeeId, CancellationToken cancellationToken = default);

    IQueryable<Employee> QueryWithDetails();
}