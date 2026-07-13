using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Repositories.Catalogs.Departments;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Catalogs;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Department>()
            .AnyAsync(d => d.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Employee>()
            .AnyAsync(e => e.DepartmentId == id, cancellationToken);
    }
}