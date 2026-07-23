using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Users;

public record UserResponseDto
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required SystemRole Role { get; init; }
    public required bool IsEmailVerified { get; init; }
    public required bool IsActive { get; init; }
    public required Language PreferredLanguage { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
