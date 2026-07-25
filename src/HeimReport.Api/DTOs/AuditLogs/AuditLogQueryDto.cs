using HeimReport.Api.DTOs.Common;

namespace HeimReport.Api.DTOs.AuditLogs;

public record AuditLogQueryDto : PaginationQueryDto
{
    public int? UserId { get; init; }
    public string? Action { get; init; }
    public string? EntityName { get; init; }
    public int? EntityId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}