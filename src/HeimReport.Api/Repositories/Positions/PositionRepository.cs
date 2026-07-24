using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Positions;

public class PositionRepository(ApplicationDbContext context)
    : Repository<Position>(context), IPositionRepository
{
    public Task<bool> ExistsByTitleAsync(string title, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Position>()
            .AnyAsync(
                p => EF.Functions.ILike(p.Title, title) && (excludeId == null || p.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.PositionId == id && e.Status == EmployeeStatus.Active, cancellationToken);
    }
}