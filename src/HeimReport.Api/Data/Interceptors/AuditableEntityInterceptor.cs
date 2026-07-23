using System.Security.Claims;
using HeimReport.Api.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace HeimReport.Api.Data.Interceptors;

public sealed class AuditableEntityInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker.Entries<IAuditableEntity>();
        var currentUserId = GetCurrentUserId();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                if (currentUserId.HasValue)
                {
                    entry.Entity.CreatedBy = currentUserId.Value;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                if (currentUserId.HasValue)
                {
                    entry.Entity.UpdatedBy = currentUserId.Value;
                }
            }
        }
    }

    private int? GetCurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (int.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        return null;
    }
}