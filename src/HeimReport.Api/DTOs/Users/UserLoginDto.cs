namespace HeimReport.Api.DTOs.Users;
public record UserLoginDto
{
    public required string UsernameOrEmail { get; init; }
    public required string Password { get; init; }
}