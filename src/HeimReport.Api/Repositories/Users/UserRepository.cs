using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Users;

public class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public Task<User?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public IQueryable<User> QueryWithDetails()
    {
        return Context.Users.Include(u => u.Employee);
    }

    public Task<User?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(
                user => user.NormalizedUsername == normalizedUsername,
                cancellationToken);
    }

    public Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(
                user => user.EmailVerificationTokenHash == tokenHash,
                cancellationToken);
    }

    public Task<User?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .FirstOrDefaultAsync(user => user.EmployeeId == employeeId, cancellationToken);
    }

    public Task<User?> GetByUsernameOrEmailAsync(string normalizedInput, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(
                user => user.NormalizedUsername == normalizedInput
                    || user.Employee!.NormalizedEmail == normalizedInput, cancellationToken);
    }

    public Task<bool> ExistsByNormalizedUsernameAsync(
        string normalizedUsername, int? excludeId, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .AnyAsync(
                user => user.NormalizedUsername == normalizedUsername
                    && (excludeId == null || user.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> ExistsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .AnyAsync(user => user.EmployeeId == employeeId, cancellationToken);
    }

    public Task<bool> ExistsByEmployeeEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .AnyAsync(user => user.Employee!.NormalizedEmail == normalizedEmail, cancellationToken);
    }
}