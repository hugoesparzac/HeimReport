using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Users;
using HeimReport.Api.Validators.Users;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Users;

public class UserProvisionDtoValidatorTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly UserProvisionDtoValidator _sut;

    public UserProvisionDtoValidatorTests()
    {
        _employeeRepository
            .Setup(r => r.ExistsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepository
            .Setup(r => r.ExistsByEmployeeIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepository
            .Setup(r => r.ExistsByNormalizedUsernameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new UserProvisionDtoValidator(_employeeRepository.Object, _userRepository.Object);
    }

    private static UserProvisionDto ValidDto(
        int employeeId = 1,
        string username = "jane.doe",
        SystemRole role = SystemRole.Employee) => new()
    {
        EmployeeId = employeeId,
        Username = username,
        Role = role,
        PreferredLanguage = Language.English
    };

    private static ValidationContext<UserProvisionDto> BuildContext(UserProvisionDto dto, SystemRole? requesterRole = SystemRole.Admin)
    {
        var context = new ValidationContext<UserProvisionDto>(dto);

        if (requesterRole.HasValue)
        {
            context.RootContextData["RequesterRole"] = requesterRole.Value;
        }

        return context;
    }

    // ===================== EMPLOYEE EXISTENCE / ACCOUNT UNIQUENESS =====================

    [Fact]
    public async Task ShouldHaveError_WhenEmployeeDoesNotExistOrIsNotActive()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.ExistsActiveAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = ValidDto(employeeId: 1);
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("Employee does not exist or is not active");
    }

    [Fact]
    public async Task ShouldHaveError_WhenEmployeeAlreadyHasAnAccount()
    {
        // Arrange
        _userRepository
            .Setup(r => r.ExistsByEmployeeIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = ValidDto(employeeId: 1);
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("This employee already has a user account");
    }

    // ===================== USERNAME =====================

    [Fact]
    public async Task ShouldHaveError_WhenUsernameIsAlreadyTaken()
    {
        // Arrange
        _userRepository
            .Setup(r => r.ExistsByNormalizedUsernameAsync("JANE.DOE", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = ValidDto(username: "jane.doe");
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is already taken");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("invalid username")]
    [InlineData("invalid$user")]
    public async Task ShouldHaveError_WhenUsernameFormatIsInvalid(string username)
    {
        // Arrange
        var dto = ValidDto(username: username);
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ===================== ROLE ASSIGNMENT POLICY =====================

    [Fact]
    public async Task ShouldNotHaveError_WhenAdminAssignsAdminRole()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Admin);
        var context = BuildContext(dto, requesterRole: SystemRole.Admin);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public async Task ShouldHaveError_WhenHRAttemptsToAssignAdminRole()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Admin);
        var context = BuildContext(dto, requesterRole: SystemRole.HR);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("You are not allowed to assign this role");
    }

    [Theory]
    [InlineData(SystemRole.Employee)]
    [InlineData(SystemRole.HR)]
    public async Task ShouldNotHaveError_WhenHRAssignsEmployeeOrHRRole(SystemRole targetRole)
    {
        // Arrange
        var dto = ValidDto(role: targetRole);
        var context = BuildContext(dto, requesterRole: SystemRole.HR);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public async Task ShouldHaveError_WhenRequesterRoleIsMissingFromRootContextData()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Employee);
        var context = BuildContext(dto, requesterRole: null);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("You are not allowed to assign this role");
    }

    [Fact]
    public async Task ShouldHaveError_WhenRoleIsInvalidEnumValue()
    {
        // Arrange
        var dto = ValidDto(role: (SystemRole)999);
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Invalid role");
    }
}