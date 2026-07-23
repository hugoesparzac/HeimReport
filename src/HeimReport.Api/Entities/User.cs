using HeimReport.Api.Entities.Interfaces;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class User : IAuditableEntity
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public required string Username { get; set; }
    public required string NormalizedUsername { get; set; }
    public required string PasswordHash { get; set; }
    public required SystemRole Role { get; set; }
    public required bool IsEmailVerified { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public required bool IsActive { get; set; }
    public required Language PreferredLanguage { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }
    public int? UpdatedBy { get; set; }
    public User? UpdatedByUser { get; set; }
}
