using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Departments;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default);
}