using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Users;

public record UserProvisionDto
{
    public required int EmployeeId { get; init; }
    public required string Username { get; init; }
    public required SystemRole Role { get; init; }
    public required Language PreferredLanguage { get; init; }
}