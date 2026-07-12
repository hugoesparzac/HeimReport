using HeimReport.Api.Data;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using EmployeeEntity = HeimReport.Api.Entities.Employee;

namespace HeimReport.Api.Repositories.Employee;

public class EmployeeRepository(ApplicationDbContext context) : Repository<EmployeeEntity>(context), IEmployeeRepository
{
    public async Task<EmployeeEntity?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<EmployeeEntity?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Email == email && e.Status == EmployeeStatus.Active, cancellationToken);
    }

    public async Task<EmployeeEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Country)
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<EmployeeEntity> Items, int TotalCount)> GetAllWithFiltersAsync(
        EmployeeStatus? status,
        int? departmentId,
        int? positionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (positionId.HasValue)
            query = query.Where(e => e.PositionId == positionId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByNationalIdAndCountryAsync(string nationalId, int countryId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.NationalId == nationalId && e.CountryId == countryId, cancellationToken);
    }
}