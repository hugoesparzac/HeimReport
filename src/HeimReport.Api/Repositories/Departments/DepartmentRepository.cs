using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Departments;

public class DepartmentRepository(ApplicationDbContext context)
    : Repository<Department>(context), IDepartmentRepository
{
    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Department>()
            .AnyAsync(
                d => EF.Functions.ILike(d.Name, name) && (excludeId == null || d.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.DepartmentId == id && e.Status == EmployeeStatus.Active, cancellationToken);
    }
}