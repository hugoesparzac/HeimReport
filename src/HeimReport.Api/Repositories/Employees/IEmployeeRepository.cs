using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Repositories.Employees;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Employee> Items, int TotalCount)> GetAllWithFiltersAsync(
        EmployeeStatus? status,
        int? departmentId,
        int? positionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNationalIdAndCountryAsync(string nationalId, int countryId, CancellationToken cancellationToken = default);
}