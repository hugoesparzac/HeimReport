using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailVerificationTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameOrEmailAsync(
        string normalizedInput,
        CancellationToken cancellationToken = default);
}
