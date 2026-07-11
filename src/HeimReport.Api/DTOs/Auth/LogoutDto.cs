namespace HeimReport.Api.DTOs.Auth;
public record LogoutDto
{
    public required string RefreshToken { get; init; }
}