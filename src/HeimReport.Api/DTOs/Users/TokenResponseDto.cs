namespace HeimReport.Api.DTOs.Users;
public record TokenResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime AccessTokenExpiresAt { get; init; }
}