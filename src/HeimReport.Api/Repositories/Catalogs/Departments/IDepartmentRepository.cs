using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Catalogs.Departments;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default);
}