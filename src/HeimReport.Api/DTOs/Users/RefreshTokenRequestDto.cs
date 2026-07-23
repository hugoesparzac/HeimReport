namespace HeimReport.Api.DTOs.Users;
public record RefreshTokenRequestDto
{
    public required string RefreshToken { get; init; }
}