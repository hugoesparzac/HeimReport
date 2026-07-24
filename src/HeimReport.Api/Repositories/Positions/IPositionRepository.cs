using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Positions;

public interface IPositionRepository : IRepository<Position>
{
    Task<bool> ExistsByTitleAsync(string title, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByActiveEmployeeAsync(int id, CancellationToken cancellationToken = default);
}