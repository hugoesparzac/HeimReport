using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class AuditLogMapperTests
{
    [Fact]
    public void ToResponseDto_ShouldMapUserFullName_WhenUserAndEmployeeAreLoaded()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = 1,
            UserId = 5,
            User = new User
            {
                Id = 5,
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
                    HireDate = DateTime.UtcNow,
                    ContractType = ContractType.Permanent,
                    CurrentSalary = 25000,
                    CountryId = 1,
                    DepartmentId = 1,
                    PositionId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                Username = "jane.doe",
                NormalizedUsername = "JANE.DOE",
                PasswordHash = "hashed",
                Role = SystemRole.Admin,
                IsEmailVerified = true,
                IsActive = true,
                PreferredLanguage = Language.English,
                CreatedAt = DateTime.UtcNow
            },
            Action = "UPDATE_EMPLOYEE",
            EntityName = nameof(Employee),
            EntityId = 10,
            OldValues = """{"Status":"Active"}""",
            NewValues = """{"Status":"InvoluntaryTermination"}""",
            IpAddress = "127.0.0.1",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = log.ToResponseDto();

        // Assert
        Assert.Equal(log.Id, result.Id);
        Assert.Equal("Jane Doe", result.UserFullName);
        Assert.Equal(log.Action, result.Action);
        Assert.Equal(log.OldValues, result.OldValues);
        Assert.Equal(log.NewValues, result.NewValues);
    }

    [Fact]
    public void ToResponseDto_ShouldReturnNullUserFullName_WhenUserIsNull()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = 1,
            UserId = null,
            User = null,
            Action = "SYSTEM_ACTION",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = log.ToResponseDto();

        // Assert
        Assert.Null(result.UserId);
        Assert.Null(result.UserFullName);
    }

    [Fact]
    public void ToResponseDto_ShouldReturnNullUserFullName_WhenUserIsLoadedButEmployeeIsNot()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = 1,
            UserId = 5,
            User = new User
            {
                Id = 5,
                EmployeeId = 10,
                Employee = null,
                Username = "jane.doe",
                NormalizedUsername = "JANE.DOE",
                PasswordHash = "hashed",
                Role = SystemRole.Admin,
                IsEmailVerified = true,
                IsActive = true,
                PreferredLanguage = Language.English,
                CreatedAt = DateTime.UtcNow
            },
            Action = "SOME_ACTION",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = log.ToResponseDto();

        // Assert
        Assert.Null(result.UserFullName);
    }
}