using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Countries;

public class CountryRepository(ApplicationDbContext context)
    : Repository<Country>(context), ICountryRepository
{
    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<Country>()
            .AnyAsync(
                c => EF.Functions.ILike(c.Name, name) && (excludeId == null || c.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.CountryId == id && e.Status == EmployeeStatus.Active, cancellationToken);
    }
}