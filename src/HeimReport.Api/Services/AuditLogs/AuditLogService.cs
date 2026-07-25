using System.Text.Json;
using HeimReport.Api.DTOs.AuditLogs;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.Entities;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.AuditLogs;

public class AuditLogService(
    IAuditLogRepository auditLogRepository,
    IHttpContextAccessor httpContextAccessor) : IAuditLogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public async Task LogAsync(
        string action,
        string? entityName = null,
        int? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = GetCurrentUserId(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues is not null ? JsonSerializer.Serialize(oldValues, SerializerOptions) : null,
            NewValues = newValues is not null ? JsonSerializer.Serialize(newValues, SerializerOptions) : null,
            IpAddress = GetClientIpAddress(),
            Timestamp = DateTime.UtcNow
        };

        await auditLogRepository.AddAsync(log, cancellationToken);
        await auditLogRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultDto<AuditLogResponseDto>> GetPagedAsync(
        AuditLogQueryDto query, CancellationToken cancellationToken = default)
    {
        var baseQuery = auditLogRepository.QueryWithDetails();

        if (query.UserId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            baseQuery = baseQuery.Where(a => a.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            baseQuery = baseQuery.Where(a => a.EntityName == query.EntityName);
        }

        if (query.EntityId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.EntityId == query.EntityId.Value);
        }

        if (query.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Timestamp >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Timestamp <= query.ToDate.Value);
        }

        baseQuery = baseQuery.OrderByDescending(a => a.Timestamp);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AuditLogResponseDto>
        {
            Items = [.. entities.Select(a => a.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    private int? GetCurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var userIdString = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(userIdString, out var userId) ? userId : null;
    }

    private string? GetClientIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}