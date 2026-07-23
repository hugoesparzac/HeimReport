using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Users;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<List<RefreshToken>> GetActiveByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    void Revoke(RefreshToken token);
}
