namespace HeimReport.Api.DTOs.Auth;
public record ResendEmailVerificationDto
{
    public required string Email { get; init; }
}