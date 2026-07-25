namespace HeimReport.Api.DTOs.AuditLogs;

public record AuditLogResponseDto
{
    public int Id { get; init; }
    public int? UserId { get; init; }
    public string? UserFullName { get; init; }
    public required string Action { get; init; }
    public string? EntityName { get; init; }
    public int? EntityId { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
    public required DateTime Timestamp { get; init; }
}