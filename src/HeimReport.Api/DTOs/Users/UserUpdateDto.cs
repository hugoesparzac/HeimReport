using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Users;

public record UserUpdateDto
{
    public required SystemRole Role { get; init; }
    public required bool IsActive { get; init; }
    public required Language PreferredLanguage { get; init; }
}