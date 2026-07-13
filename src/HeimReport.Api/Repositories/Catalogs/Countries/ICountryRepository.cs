using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Catalogs.Countries;

public interface ICountryRepository : IRepository<Country>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default);
}