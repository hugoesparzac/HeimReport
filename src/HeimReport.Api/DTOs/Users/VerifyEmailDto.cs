namespace HeimReport.Api.DTOs.Users;

public record VerifyEmailDto
{
    public required string Token { get; init; }
}