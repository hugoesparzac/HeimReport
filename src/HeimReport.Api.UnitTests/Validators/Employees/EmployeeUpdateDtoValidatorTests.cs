using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Validators.Employees;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Employees;

public class EmployeeUpdateDtoValidatorTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly Mock<IPositionRepository> _positionRepository = new();

    private readonly EmployeeUpdateDtoValidator _sut;

    private const int EmployeeId = 10;

    public EmployeeUpdateDtoValidatorTests()
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

        _employeeRepository
            .Setup(r => r.GetManagerChainAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new EmployeeUpdateDtoValidator(
            _employeeRepository.Object,
            _countryRepository.Object,
            _departmentRepository.Object,
            _positionRepository.Object);
    }

    // ===================== HELPERS =====================

    private static EmployeeUpdateDto ValidDto(
        EmployeeStatus status = EmployeeStatus.Active,
        int departmentId = 1,
        int positionId = 1,
        int? managerId = null,
        decimal currentSalary = 20000,
        JobChangeReason? changeReason = null,
        string? otherReasonDetail = null,
        DateTime? terminationDate = null) => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane.doe@heimreport-demo.com",
        NationalId = "SEED-00001",
        BirthDate = DateTime.UtcNow.AddYears(-30),
        ContractType = ContractType.Permanent,
        Status = status,
        TerminationDate = terminationDate,
        CurrentSalary = currentSalary,
        CountryId = 1,
        DepartmentId = departmentId,
        PositionId = positionId,
        ManagerId = managerId,
        ChangeReason = changeReason,
        OtherReasonDetail = otherReasonDetail
    };

    private static EmployeeSnapshot DefaultSnapshot(
        int departmentId = 1,
        int positionId = 1,
        int? managerId = null,
        decimal currentSalary = 20000,
        EmployeeStatus status = EmployeeStatus.Active) =>
        new(departmentId, positionId, managerId, currentSalary, status);

    private static ValidationContext<EmployeeUpdateDto> BuildContext(
        EmployeeUpdateDto dto, EmployeeSnapshot? snapshot = null, int employeeId = EmployeeId)
    {
        var context = new ValidationContext<EmployeeUpdateDto>(dto);
        context.RootContextData["EmployeeId"] = employeeId;

        if (snapshot is not null)
        {
            context.RootContextData["CurrentSnapshot"] = snapshot;
        }

        return context;
    }

    // ===================== MANAGER: SELF-REFERENCE =====================

    [Fact]
    public async Task ShouldHaveError_WhenManagerIsSelf()
    {
        // Arrange
        var dto = ValidDto(managerId: EmployeeId);
        var context = BuildContext(dto, DefaultSnapshot());

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("An employee cannot be their own manager");
    }

    // ===================== MANAGER: INVENTORY / ACTIVE =====================

    [Fact]
    public async Task ShouldHaveError_WhenManagerDoesNotExistOrIsNotActive()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.ExistsActiveAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = ValidDto(managerId: 99);
        var context = BuildContext(dto, DefaultSnapshot());

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("Manager does not exist or is not active");
    }

    // ===================== MANAGER: CYCLE DETECTION =====================

    [Fact]
    public async Task ShouldHaveError_WhenAssigningManagerWouldCreateCycle()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.GetManagerChainAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([5, 7, EmployeeId]);

        var dto = ValidDto(managerId: 5);
        var context = BuildContext(dto, DefaultSnapshot());

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("This assignment would create a circular reporting relationship");
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenAssigningManagerWithNoCycle()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.GetManagerChainAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([5, 7, 8]);

        var dto = ValidDto(managerId: 5);
        var context = BuildContext(dto, DefaultSnapshot());

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManagerId);
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenManagerIdIsNull()
    {
        // Arrange
        var dto = ValidDto(managerId: null);
        var context = BuildContext(dto, DefaultSnapshot());

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManagerId);
        _employeeRepository.Verify(
            r => r.GetManagerChainAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== CHANGEREASON: REQUIRED BASED ON CHANGES =====================

    [Fact]
    public async Task ShouldHaveError_WhenDepartmentChangesButChangeReasonIsMissing()
    {
        // Arrange
        var snapshot = DefaultSnapshot(departmentId: 1);
        var dto = ValidDto(departmentId: 2, changeReason: null);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChangeReason)
            .WithErrorMessage("A change reason is required when Department, Position, Manager or Salary changes");
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenNothingRelevantChanges()
    {
        // Arrange
        var snapshot = DefaultSnapshot(departmentId: 1, positionId: 1, managerId: null, currentSalary: 20000);
        var dto = ValidDto(departmentId: 1, positionId: 1, managerId: null, currentSalary: 20000, changeReason: null);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChangeReason);
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenSalaryChanges_AndChangeReasonIsProvided()
    {
        // Arrange
        var snapshot = DefaultSnapshot(currentSalary: 20000);
        var dto = ValidDto(currentSalary: 25000, changeReason: JobChangeReason.SalaryAdjustment);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChangeReason);
    }

    // ===================== CHANGEREASON: NOT REQUIRED FOR CANCELLATION / REACTIVATION =====================

    [Fact]
    public async Task ShouldNotRequireChangeReason_WhenEmployeeIsGoingOnLeave_EvenIfJobFieldsChanged()
    {
        // Arrange
        var snapshot = DefaultSnapshot(departmentId: 1, status: EmployeeStatus.Active);

        var dto = ValidDto(
            status: EmployeeStatus.VoluntaryResignation,
            departmentId: 2,
            changeReason: null,
            terminationDate: DateTime.UtcNow);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChangeReason);
    }

    [Fact]
    public async Task ShouldNotRequireChangeReason_WhenEmployeeIsReactivated()
    {
        // Arrange
        var snapshot = DefaultSnapshot(departmentId: 1, status: EmployeeStatus.VoluntaryResignation);

        var dto = ValidDto(status: EmployeeStatus.Active, departmentId: 2, changeReason: null);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChangeReason);
    }

    // ===================== CHANGEREASON: VALUES RESERVED FOR THE SYSTEM =====================

    [Theory]
    [InlineData(JobChangeReason.Termination)]
    [InlineData(JobChangeReason.Reactivation)]
    public async Task ShouldHaveError_WhenClientSubmitsSystemReservedChangeReason(JobChangeReason reservedReason)
    {
        // Arrange
        var snapshot = DefaultSnapshot();
        var dto = ValidDto(departmentId: 2, changeReason: reservedReason);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChangeReason);
    }

    [Fact]
    public async Task ShouldHaveError_WhenChangeReasonIsInvalidEnumValue()
    {
        // Arrange
        var snapshot = DefaultSnapshot();
        var dto = ValidDto(departmentId: 2, changeReason: (JobChangeReason)999);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChangeReason)
            .WithErrorMessage("Invalid change reason");
    }

    // ===================== OTHER REASON DETAIL =====================

    [Fact]
    public async Task ShouldHaveError_WhenChangeReasonIsOther_AndOtherReasonDetailIsMissing()
    {
        // Arrange
        var snapshot = DefaultSnapshot();
        var dto = ValidDto(departmentId: 2, changeReason: JobChangeReason.Other, otherReasonDetail: null);

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OtherReasonDetail)
            .WithErrorMessage("Please specify the reason when selecting 'Other'");
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenChangeReasonIsOther_AndOtherReasonDetailIsProvided()
    {
        // Arrange
        var snapshot = DefaultSnapshot();
        var dto = ValidDto(departmentId: 2, changeReason: JobChangeReason.Other, otherReasonDetail: "Special case approved by HR");

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OtherReasonDetail);
    }

    [Fact]
    public async Task ShouldHaveError_WhenOtherReasonDetailIsProvided_ButChangeReasonIsNotOther()
    {
        // Arrange
        var snapshot = DefaultSnapshot();
        var dto = ValidDto(
            departmentId: 2,
            changeReason: JobChangeReason.Promotion,
            otherReasonDetail: "This should not be here");

        var context = BuildContext(dto, snapshot);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OtherReasonDetail)
            .WithErrorMessage("Reason detail should only be provided when ChangeReason is 'Other'");
    }

    // ===================== NO ROOTCONTEXTDATA: MUST FAIL CLOSED =====================

    [Fact]
    public async Task ShouldThrow_WhenEmployeeIdIsMissingFromRootContextData()
    {
        // Arrange
        var dto = ValidDto(departmentId: 2, changeReason: JobChangeReason.Promotion);
        var context = new ValidationContext<EmployeeUpdateDto>(dto); // sin RootContextData

        // Act
        Task Act() => _sut.ValidateAsync(context);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }
}