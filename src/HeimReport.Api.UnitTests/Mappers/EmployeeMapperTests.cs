using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class EmployeeMapperTests
{
    private static Employee BuildFullyLoadedEmployee(bool withManager = false)
    {
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@heimreport-demo.com",
            NormalizedEmail = "JANE.DOE@HEIMREPORT-DEMO.COM",
            NationalId = "SEED-00001",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            HireDate = DateTime.UtcNow.AddYears(-2),
            ContractType = ContractType.Permanent,
            Status = EmployeeStatus.Active,
            CurrentSalary = 25000,
            CountryId = 1,
            Country = new Country { Id = 1, Name = "Mexico", IsActive = true },
            DepartmentId = 2,
            Department = new Department { Id = 2, Name = "Technology", IsActive = true },
            PositionId = 3,
            Position = new Position { Id = 3, Title = "Analyst", CareerLevel = CareerLevel.Professional, IsCritical = false, IsActive = true },
            CreatedAt = DateTime.UtcNow
        };

        if (withManager)
        {
            employee.ManagerId = 99;
            employee.Manager = new Employee
            {
                Id = 99,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@heimreport-demo.com",
                NormalizedEmail = "JOHN.SMITH@HEIMREPORT-DEMO.COM",
                NationalId = "SEED-00099",
                BirthDate = DateTime.UtcNow.AddYears(-45),
                HireDate = DateTime.UtcNow.AddYears(-10),
                ContractType = ContractType.Permanent,
                Status = EmployeeStatus.Active,
                CurrentSalary = 60000,
                CountryId = 1,
                DepartmentId = 2,
                PositionId = 5,
                CreatedAt = DateTime.UtcNow
            };
        }

        return employee;
    }

    // ===================== HAPPY PATH =====================

    [Fact]
    public void ToResponseDto_ShouldMapAllFields_WhenNavigationPropertiesAreLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();

        // Act
        var result = employee.ToResponseDto();

        // Assert
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal(employee.CountryId, result.CountryId);
        Assert.Equal("Mexico", result.CountryName);
        Assert.Equal("Technology", result.DepartmentName);
        Assert.Equal("Analyst", result.PositionTitle);
        Assert.Null(result.ManagerId);
        Assert.Null(result.ManagerFullName);
    }

    [Fact]
    public void ToResponseDto_ShouldMapManagerFullName_WhenManagerIsLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee(withManager: true);

        // Act
        var result = employee.ToResponseDto();

        // Assert
        Assert.Equal(99, result.ManagerId);
        Assert.Equal("John Smith", result.ManagerFullName);
    }

    [Fact]
    public void ToResponseDto_ShouldReturnNullManagerFullName_WhenEmployeeHasNoManager()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();
        employee.ManagerId = null;
        employee.Manager = null;

        // Act
        var result = employee.ToResponseDto();

        // Assert
        Assert.Null(result.ManagerId);
        Assert.Null(result.ManagerFullName);
    }

    // ===================== DEFENSIVE CHECKS: INCLUDE MISSING =====================

    [Fact]
    public void ToResponseDto_ShouldThrow_WhenCountryIsNotLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();
        employee.Country = null;

        // Act
        void Act() => employee.ToResponseDto();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(Act);
        Assert.Contains("Country/Department/Position", exception.Message);
    }

    [Fact]
    public void ToResponseDto_ShouldThrow_WhenDepartmentIsNotLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();
        employee.Department = null;

        // Act
        void Act() => employee.ToResponseDto();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void ToResponseDto_ShouldThrow_WhenPositionIsNotLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();
        employee.Position = null;

        // Act
        void Act() => employee.ToResponseDto();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void ToResponseDto_ShouldThrow_WhenManagerIdIsSetButManagerNavigationIsNotLoaded()
    {
        // Arrange
        var employee = BuildFullyLoadedEmployee();
        employee.ManagerId = 99;
        employee.Manager = null;

        // Act
        void Act() => employee.ToResponseDto();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(Act);
        Assert.Contains("Manager navigation was not loaded", exception.Message);
    }

    // ===================== TO ENTITY / UPDATE ENTITY =====================

    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields_AndDefaultStatusToActive()
    {
        // Arrange
        var dto = new EmployeeCreateDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@heimreport-demo.com",
            NationalId = "SEED-00001",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            HireDate = DateTime.UtcNow,
            ContractType = ContractType.Permanent,
            CurrentSalary = 25000,
            CountryId = 1,
            DepartmentId = 2,
            PositionId = 3
        };

        // Act
        var entity = dto.ToEntity();

        // Assert
        Assert.Equal(dto.FirstName, entity.FirstName);
        Assert.Equal(dto.Email, entity.Email);
        Assert.Equal(dto.Email.ToUpperInvariant(), entity.NormalizedEmail);
        Assert.Equal(EmployeeStatus.Active, entity.Status);
    }

    [Fact]
    public void UpdateEntity_ShouldMutateExistingInstance()
    {
        // Arrange
        var entity = BuildFullyLoadedEmployee();
        var dto = new EmployeeUpdateDto
        {
            FirstName = "UpdatedFirstName",
            LastName = entity.LastName,
            Email = "new.email@heimreport-demo.com",
            NationalId = entity.NationalId,
            BirthDate = entity.BirthDate,
            ContractType = entity.ContractType,
            Status = EmployeeStatus.Active,
            CurrentSalary = 30000,
            CountryId = entity.CountryId,
            DepartmentId = entity.DepartmentId,
            PositionId = entity.PositionId,
            ManagerId = null
        };

        // Act
        dto.UpdateEntity(entity);

        // Assert
        Assert.Equal("UpdatedFirstName", entity.FirstName);
        Assert.Equal("new.email@heimreport-demo.com", entity.Email);
        Assert.Equal("NEW.EMAIL@HEIMREPORT-DEMO.COM", entity.NormalizedEmail);
        Assert.Equal(30000, entity.CurrentSalary);
    }
}