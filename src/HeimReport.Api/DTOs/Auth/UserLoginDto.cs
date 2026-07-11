namespace HeimReport.Api.DTOs.Auth;
public record UserLoginDto
{
    public required string UsernameOrEmail { get; init; }
    public required string Password { get; init; }
}