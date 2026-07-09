using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required User User { get; set; }
    public required string TokenHash { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}