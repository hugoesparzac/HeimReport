using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Positions;

public class PositionRepository(ApplicationDbContext context) : Repository<Position>(context), IPositionRepository
{
    public Task<bool> ExistsByNameAsync(string title, CancellationToken cancellationToken = default)
    {
        return Context.Set<Position>()
            .AnyAsync(
                p => EF.Functions.ILike(p.Title, title),
                cancellationToken);
    }

    public Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.PositionId == id, cancellationToken);
    }
}