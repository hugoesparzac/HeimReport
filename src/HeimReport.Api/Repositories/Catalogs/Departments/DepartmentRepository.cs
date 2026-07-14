using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Catalogs;

public class DepartmentRepository(ApplicationDbContext context)
    : Repository<Department>(context), IDepartmentRepository
{
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Department>()
            .AnyAsync(d => d.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Employee>()
            .AnyAsync(e => e.DepartmentId == id, cancellationToken);
    }
}