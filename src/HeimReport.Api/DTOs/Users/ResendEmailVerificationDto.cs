namespace HeimReport.Api.DTOs.Users;
public record ResendEmailVerificationDto
{
    public required string Email { get; init; }
}