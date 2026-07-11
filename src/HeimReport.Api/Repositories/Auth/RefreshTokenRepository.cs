using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Auth;

public class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .ThenInclude(user => user!.Employee)
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    public void Revoke(RefreshToken token)
    {
        token.RevokedAt = DateTime.UtcNow;
    }
}