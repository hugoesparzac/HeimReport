using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Countries;

public class CountryRepository(ApplicationDbContext context)
    : Repository<Country>(context), ICountryRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Context.Set<Country>()
            .AnyAsync(
                c => EF.Functions.ILike(c.Name, name),
                cancellationToken);
    }

    public Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<Employee>()
            .AnyAsync(e => e.CountryId == id, cancellationToken);
    }
}