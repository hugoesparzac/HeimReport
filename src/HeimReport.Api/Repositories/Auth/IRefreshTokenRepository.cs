using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Auth;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    void Revoke(RefreshToken token);
}