using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Mappers;

public static class UserMapper
{
    public static UserResponseDto ToResponseDto(this User user)
    {
        if (user.Employee is null)
        {
            throw new InvalidOperationException(
                $"User with Id {user.Id} was loaded without its related Employee. " +
                "Ensure the query includes .Include(u => u.Employee).");
        }

        return new UserResponseDto
        {
            Id = user.Id,
            EmployeeId = user.EmployeeId,
            EmployeeFullName = $"{user.Employee.FirstName} {user.Employee.LastName}",
            Username = user.Username,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            PreferredLanguage = user.PreferredLanguage,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public static User ToEntity(this UserRegistrationDto dto, int employeeId, string passwordHash)
    {
        return new User
        {
            EmployeeId = employeeId,
            Username = dto.Username,
            NormalizedUsername = dto.Username.ToUpperInvariant(),
            PasswordHash = passwordHash,
            Role = SystemRole.Employee,
            IsEmailVerified = false,
            IsActive = true,
            PreferredLanguage = dto.PreferredLanguage
        };
    }

    public static User ToEntity(this UserProvisionDto dto, string passwordHash)
    {
        return new User
        {
            EmployeeId = dto.EmployeeId,
            Username = dto.Username,
            NormalizedUsername = dto.Username.ToUpperInvariant(),
            PasswordHash = passwordHash,
            Role = dto.Role,
            IsEmailVerified = false,
            IsActive = true,
            PreferredLanguage = dto.PreferredLanguage
        };
    }

    public static void UpdateEntity(this UserUpdateDto dto, User user)
    {
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        user.PreferredLanguage = dto.PreferredLanguage;
    }
}