using HeimReport.Api.DTOs.AuditLogs;
using HeimReport.Api.DTOs.Common;

namespace HeimReport.Api.Services.AuditLogs;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string? entityName = null,
        int? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AuditLogResponseDto>> GetPagedAsync(
        AuditLogQueryDto query, CancellationToken cancellationToken = default);
}