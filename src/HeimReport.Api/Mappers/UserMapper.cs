using HeimReport.Api.DTOs.Auth;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
namespace HeimReport.Api.Mappers;
public static class UserMapper
{
    public static User ToEntity(this UserRegistrationDto dto, int employeeId)
    {
        return new User
        {
            EmployeeId = employeeId,
            Username = dto.Username,
            NormalizedUsername = dto.Username.Trim().ToUpperInvariant(),
            PasswordHash = string.Empty, // se asigna en el Service con IPasswordHasher
            Role = SystemRole.Employee,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}