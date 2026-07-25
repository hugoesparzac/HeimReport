using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<User?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(string normalizedInput, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedUsernameAsync(string normalizedUsername, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmployeeEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    IQueryable<User> QueryWithDetails();
}