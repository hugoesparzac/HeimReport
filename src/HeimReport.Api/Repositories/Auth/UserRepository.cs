using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Auth;

public class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(
                user => user.NormalizedUsername == normalizedUsername,
                cancellationToken);
    }

    public async Task<User?> GetByEmailVerificationTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(user => user.Employee)
            .FirstOrDefaultAsync(
                user => user.EmailVerificationTokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<User?> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(user => user.EmployeeId == employeeId, cancellationToken);
    }
}
