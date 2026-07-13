using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using HeimReport.Api.Repositories.Catalogs.Countries;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Catalogs;

public class CountryRepository : Repository<Country>, ICountryRepository
{
    private readonly ApplicationDbContext _context;

    public CountryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Country>()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Employee>()
            .AnyAsync(e => e.CountryId == id, cancellationToken);
    }
}