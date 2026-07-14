using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Catalogs;

public class PositionRepository(ApplicationDbContext context)
    : Repository<Position>(context), IPositionRepository
{
    public async Task<bool> ExistsByNameAsync(string title, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Position>()
            .AnyAsync(p => p.Title.ToLower() == title.ToLower(), cancellationToken);
    }

    public async Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Employee>()
            .AnyAsync(e => e.PositionId == id, cancellationToken);
    }
}