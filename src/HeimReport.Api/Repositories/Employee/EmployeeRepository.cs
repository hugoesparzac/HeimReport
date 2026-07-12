using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Employees;

public class EmployeeRepository(ApplicationDbContext context) : Repository<Employee>(context), IEmployeeRepository
{
    public async Task<Employee?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        return await DbSet.FirstOrDefaultAsync(e => e.NormalizedEmail == normalizedEmail && e.Status == EmployeeStatus.Active, cancellationToken);
    }

    public async Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Country)
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetAllWithFiltersAsync(
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