using HeimReport.Api.Enums;
using HeimReport.Api.Repositories;
using EmployeeEntity = HeimReport.Api.Entities.Employee;

namespace HeimReport.Api.Repositories.Employee;

public interface IEmployeeRepository : IRepository<EmployeeEntity>
{
    Task<EmployeeEntity?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<EmployeeEntity?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<EmployeeEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<EmployeeEntity> Items, int TotalCount)> GetAllWithFiltersAsync(
        EmployeeStatus? status,
        int? departmentId,
        int? positionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNationalIdAndCountryAsync(string nationalId, int countryId, CancellationToken cancellationToken = default);
}