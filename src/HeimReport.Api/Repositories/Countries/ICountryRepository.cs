using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Countries;

public interface ICountryRepository : IRepository<Country>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default);
}