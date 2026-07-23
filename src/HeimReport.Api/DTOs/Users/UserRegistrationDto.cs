using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Users;
public record UserRegistrationDto
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
    public required Language PreferredLanguage { get; init; }
}
