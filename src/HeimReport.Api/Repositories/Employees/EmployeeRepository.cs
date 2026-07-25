using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Employees;

public class EmployeeRepository(ApplicationDbContext context)
    : Repository<Employee>(context), IEmployeeRepository
{
    public Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public IQueryable<Employee> QueryWithDetails()
    {
        return Context.Set<Employee>()
            .Include(e => e.Country)
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager);
    }

    public Task<Employee?> GetActiveByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.NormalizedEmail == normalizedEmail && e.Status == Enums.EmployeeStatus.Active, cancellationToken);
    }

    public Task<bool> ExistsActiveAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.Id == employeeId && e.Status == Enums.EmployeeStatus.Active, cancellationToken);
    }

    public Task<bool> ExistsActiveByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.NormalizedEmail == normalizedEmail && e.Status == Enums.EmployeeStatus.Active, cancellationToken);
    }

    public Task<bool> ExistsByNormalizedEmailAsync(
        string normalizedEmail, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(
                e => e.NormalizedEmail == normalizedEmail && (excludeId == null || e.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> ExistsByNationalIdAndCountryAsync(
        string nationalId, int countryId, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(
                e => e.NationalId == nationalId
                    && e.CountryId == countryId
                    && (excludeId == null || e.Id != excludeId),
                cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetManagerChainAsync(
        int startEmployeeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH RECURSIVE manager_chain AS (
                SELECT "Id", "ManagerId", 1 AS depth
                FROM "Employees"
                WHERE "Id" = {0}

                UNION ALL

                SELECT e."Id", e."ManagerId", mc.depth + 1
                FROM "Employees" e
                INNER JOIN manager_chain mc ON e."Id" = mc."ManagerId"
                WHERE mc.depth < 20 -- margen amplio sobre los 6 CareerLevel reales; corta rápido si hay datos corruptos
            )
            SELECT "Id" FROM manager_chain
            """;

        return await Context.Database
            .SqlQueryRaw<int>(sql, startEmployeeId)
            .ToListAsync(cancellationToken);
    }
}