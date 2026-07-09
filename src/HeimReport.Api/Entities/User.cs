using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class User
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public required Employee Employee { get; set; }
    public required string Username { get; set; }
    public required string NormalizedUsername { get; set; }
    public required string PasswordHash { get; set; }
    public required SystemRole Role { get; set; }
    public required bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}