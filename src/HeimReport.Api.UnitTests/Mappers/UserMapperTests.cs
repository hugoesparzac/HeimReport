using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class UserMapperTests
{
    private static User BuildUserWithEmployee() => new()
    {
        Id = 1,
        EmployeeId = 10,
        Employee = new Employee
        {
            Id = 10,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@heimreport-demo.com",
            NormalizedEmail = "JANE.DOE@HEIMREPORT-DEMO.COM",
            NationalId = "SEED-00001",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            HireDate = DateTime.UtcNow.AddYears(-1),
            ContractType = ContractType.Permanent,
            Status = EmployeeStatus.Active,
            CurrentSalary = 25000,
            CountryId = 1,
            DepartmentId = 2,
            PositionId = 3,
            CreatedAt = DateTime.UtcNow
        },
        Username = "jane.doe",
        NormalizedUsername = "JANE.DOE",
        PasswordHash = "hashed-password",
        Role = SystemRole.Employee,
        IsEmailVerified = true,
        IsActive = true,
        PreferredLanguage = Language.English,
        CreatedAt = DateTime.UtcNow
    };

    // ===================== HAPPY PATH =====================

    [Fact]
    public void ToResponseDto_ShouldMapAllFields_WhenEmployeeIsLoaded()
    {
        // Arrange
        var user = BuildUserWithEmployee();

        // Act
        var result = user.ToResponseDto();

        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.EmployeeId, result.EmployeeId);
        Assert.Equal("Jane Doe", result.EmployeeFullName);
        Assert.Equal(user.Role, result.Role);
        Assert.Equal(user.IsActive, result.IsActive);
    }

    [Fact]
    public void ToResponseDto_ShouldNeverExposePasswordHash()
    {
        var user = BuildUserWithEmployee();

        // Act
        var result = user.ToResponseDto();

        // Assert
        var dtoProperties = typeof(UserResponseDto).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("PasswordHash", dtoProperties);
    }

    // ===================== DEFENSIVE CHECK: INCLUDE FALTANTE =====================

    [Fact]
    public void ToResponseDto_ShouldThrow_WhenEmployeeIsNotLoaded()
    {
        // Arrange
        var user = BuildUserWithEmployee();
        user.Employee = null;

        // Act
        void Act() => user.ToResponseDto();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(Act);
        Assert.Contains("Employee", exception.Message);
    }

    // ===================== TO ENTITY (REGISTRATION / PROVISION) =====================

    [Fact]
    public void ToEntity_FromRegistrationDto_ShouldForceEmployeeRole_AndUnverifiedEmail()
    {
        // Arrange
        var dto = new UserRegistrationDto
        {
            Email = "jane.doe@heimreport-demo.com",
            Username = "jane.doe",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            PreferredLanguage = Language.Spanish
        };

        // Act
        var entity = dto.ToEntity(employeeId: 10, passwordHash: "hashed-password");

        // Assert
        Assert.Equal(10, entity.EmployeeId);
        Assert.Equal("hashed-password", entity.PasswordHash);
        Assert.Equal(SystemRole.Employee, entity.Role);
        Assert.False(entity.IsEmailVerified);
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void ToEntity_FromProvisionDto_ShouldRespectRequestedRole()
    {
        // Arrange
        var dto = new UserProvisionDto
        {
            EmployeeId = 20,
            Username = "hr.user",
            Role = SystemRole.HR,
            PreferredLanguage = Language.English
        };

        // Act
        var entity = dto.ToEntity(passwordHash: "hashed-temp-password");

        // Assert
        Assert.Equal(20, entity.EmployeeId);
        Assert.Equal(SystemRole.HR, entity.Role);
        Assert.Equal("hashed-temp-password", entity.PasswordHash);
        Assert.False(entity.IsEmailVerified);
    }

    // ===================== UPDATE ENTITY =====================

    [Fact]
    public void UpdateEntity_ShouldMutateExistingInstance_WithoutTouchingPasswordHash()
    {
        // Arrange
        var entity = BuildUserWithEmployee();
        var originalPasswordHash = entity.PasswordHash;

        var dto = new UserUpdateDto
        {
            Role = SystemRole.Admin,
            IsActive = false,
            PreferredLanguage = Language.Spanish
        };

        // Act
        dto.UpdateEntity(entity);

        // Assert
        Assert.Equal(SystemRole.Admin, entity.Role);
        Assert.False(entity.IsActive);
        Assert.Equal(Language.Spanish, entity.PreferredLanguage);
        Assert.Equal(originalPasswordHash, entity.PasswordHash);
    }
}