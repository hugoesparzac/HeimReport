using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Auth;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailVerificationTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}