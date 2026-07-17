using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Positions;

public interface IPositionRepository : IRepository<Position>
{
    Task<bool> ExistsByNameAsync(string title, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByEmployeeAsync(int id, CancellationToken cancellationToken = default);
}