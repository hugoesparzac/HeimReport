namespace HeimReport.Api.DTOs.Auth;
public record RefreshTokenRequestDto
{
    public required string RefreshToken { get; init; }
}