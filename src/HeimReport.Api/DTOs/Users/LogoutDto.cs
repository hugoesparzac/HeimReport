namespace HeimReport.Api.DTOs.Users;
public record LogoutDto
{
    public required string RefreshToken { get; init; }
}