using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Departments;

public class DepartmentRepository(ApplicationDbContext context)
    : Repository<Department>(context), IDepartmentRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Context.Set<Department>()
            .AnyAsync(
                d => EF.Functions.ILike(d.Name, name),
                cancellationToken);
    }

    public Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.DepartmentId == id, cancellationToken);
    }
}