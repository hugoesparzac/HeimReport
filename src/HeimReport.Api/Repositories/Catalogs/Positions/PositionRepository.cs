using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Repositories.Catalogs.Positions;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Catalogs;

public class PositionRepository : Repository<Position>, IPositionRepository
{
    private readonly ApplicationDbContext _context;

    public PositionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Position>()
            .AnyAsync(p => p.Title.ToLower() == title.ToLower(), cancellationToken);
    }

    public async Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Employee>()
            .AnyAsync(e => e.PositionId == id, cancellationToken);
    }
}