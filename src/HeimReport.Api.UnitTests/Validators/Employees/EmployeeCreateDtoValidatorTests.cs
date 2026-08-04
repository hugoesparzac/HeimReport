using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Validators.Employees;
using FluentValidation.TestHelper;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Employees;

public class EmployeeCreateDtoValidatorTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly Mock<IPositionRepository> _positionRepository = new();

    private readonly EmployeeCreateDtoValidator _sut;

    public EmployeeCreateDtoValidatorTests()
    {
        _countryRepository.Setup(r => r.ExistsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _departmentRepository.Setup(r => r.ExistsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _positionRepository.Setup(r => r.ExistsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _employeeRepository.Setup(r => r.ExistsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _employeeRepository
            .Setup(r => r.ExistsByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _employeeRepository
            .Setup(r => r.ExistsByNationalIdAndCountryAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new EmployeeCreateDtoValidator(
            _employeeRepository.Object, _countryRepository.Object, _departmentRepository.Object, _positionRepository.Object);
    }

    private static EmployeeCreateDto ValidDto(
        string email = "jane.doe@heimreport-demo.com",
        string nationalId = "SEED-00001",
        DateTime? birthDate = null,
        DateTime? hireDate = null,
        DateTime? contractEndDate = null,
        decimal currentSalary = 20000,
        int countryId = 1,
        int departmentId = 1,
        int positionId = 1,
        int? managerId = null) => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = email,
        NationalId = nationalId,
        BirthDate = birthDate ?? DateTime.UtcNow.AddYears(-30),
        HireDate = hireDate ?? DateTime.UtcNow.AddDays(-10),
        ContractType = ContractType.Permanent,
        ContractEndDate = contractEndDate,
        CurrentSalary = currentSalary,
        CountryId = countryId,
        DepartmentId = departmentId,
        PositionId = positionId,
        ManagerId = managerId
    };

    // ===================== BASIC FIELD RULES =====================

    [Fact]
    public async Task ShouldHaveError_WhenFirstNameIsEmpty()
    {
        // Arrange
        var dto = ValidDto() with { FirstName = "" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task ShouldHaveError_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var dto = ValidDto() with { Email = "not-an-email" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task ShouldHaveError_WhenEmployeeIsYoungerThan15()
    {
        // Arrange
        var dto = ValidDto(birthDate: DateTime.UtcNow.AddYears(-10));

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorMessage("Employee must be at least 15 years old");
    }

    [Fact]
    public async Task ShouldHaveError_WhenContractEndDateIsBeforeHireDate()
    {
        // Arrange
        var hireDate = DateTime.UtcNow;
        var dto = ValidDto(hireDate: hireDate, contractEndDate: hireDate.AddDays(-1));

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContractEndDate)
            .WithErrorMessage("Contract End Date must be after Hire Date");
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenContractEndDateIsNull()
    {
        // Arrange
        var dto = ValidDto(contractEndDate: null);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContractEndDate);
    }

    [Fact]
    public async Task ShouldHaveError_WhenCurrentSalaryIsNegative()
    {
        // Arrange
        var dto = ValidDto(currentSalary: -1);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentSalary)
            .WithErrorMessage("Salary cannot be negative");
    }

    // ===================== EMAIL / NATIONALID UNIQUENESS =====================

    [Fact]
    public async Task ShouldHaveError_WhenEmailAlreadyExists()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.ExistsByNormalizedEmailAsync("JANE.DOE@HEIMREPORT-DEMO.COM", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = ValidDto();

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("An employee with this email already exists");
    }

    [Fact]
    public async Task ShouldHaveError_WhenNationalIdAlreadyExistsInSameCountry()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.ExistsByNationalIdAndCountryAsync("SEED-00001", 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = ValidDto(nationalId: "SEED-00001", countryId: 1);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor("NationalId")
            .WithErrorMessage("An employee with this National ID already exists in this country");
    }

    // ===================== CATALOG EXISTENCE / ACTIVE =====================

    [Fact]
    public async Task ShouldHaveError_WhenCountryDoesNotExistOrIsNotActive()
    {
        // Arrange
        _countryRepository.Setup(r => r.ExistsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var dto = ValidDto(countryId: 1);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CountryId)
            .WithErrorMessage("Country does not exist or is not active");
    }

    [Fact]
    public async Task ShouldHaveError_WhenDepartmentDoesNotExistOrIsNotActive()
    {
        // Arrange
        _departmentRepository.Setup(r => r.ExistsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var dto = ValidDto(departmentId: 1);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("Department does not exist or is not active");
    }

    [Fact]
    public async Task ShouldHaveError_WhenPositionDoesNotExistOrIsNotActive()
    {
        // Arrange
        _positionRepository.Setup(r => r.ExistsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var dto = ValidDto(positionId: 1);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionId)
            .WithErrorMessage("Position does not exist or is not active");
    }

    // ===================== MANAGER =====================

    [Fact]
    public async Task ShouldHaveError_WhenManagerDoesNotExistOrIsNotActive()
    {
        // Arrange
        _employeeRepository.Setup(r => r.ExistsActiveAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var dto = ValidDto(managerId: 99);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("Manager does not exist or is not active");
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenManagerIdIsNull()
    {
        // Arrange
        var dto = ValidDto(managerId: null);

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManagerId);
    }

    // ===================== HAPPY PATH =====================

    [Fact]
    public async Task ShouldNotHaveAnyErrors_WhenDtoIsValid()
    {
        // Arrange
        var dto = ValidDto();

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}