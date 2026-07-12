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
            PasswordHash = string.Empty,
            Role = SystemRole.Employee,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserResponseDto ToResponseDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public static void RecordLogin(this User user)
    {
        user.LastLoginAt = DateTime.UtcNow;
    }

    public static void ConfirmEmailVerification(this User user)
    {
        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
    }

    public static void SetEmailVerificationToken(this User user, string tokenHash, DateTime expiresAt)
    {
        user.EmailVerificationTokenHash = tokenHash;
        user.EmailVerificationTokenExpiresAt = expiresAt;
    }
}