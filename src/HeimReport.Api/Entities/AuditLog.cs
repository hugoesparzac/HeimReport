namespace HeimReport.Api.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public required string Action { get; set; }
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public required DateTime Timestamp { get; set; }
}