using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Enums;
using HeimReport.Api.Validators.Users;

namespace HeimReport.Api.UnitTests.Validators.Users;

public class UserUpdateDtoValidatorTests
{
    private readonly UserUpdateDtoValidator _sut = new();

    private static UserUpdateDto ValidDto(SystemRole role = SystemRole.Employee, bool isActive = true) => new()
    {
        Role = role,
        IsActive = isActive,
        PreferredLanguage = Language.English
    };

    private static ValidationContext<UserUpdateDto> BuildContext(
        UserUpdateDto dto, SystemRole? requesterRole = SystemRole.Admin)
    {
        var context = new ValidationContext<UserUpdateDto>(dto);

        if (requesterRole.HasValue)
        {
            context.RootContextData["RequesterRole"] = requesterRole.Value;
        }

        return context;
    }

    // ===================== ROLE ASSIGNMENT POLICY =====================

    [Fact]
    public void ShouldNotHaveError_WhenAdminAssignsAdminRole()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Admin);
        var context = BuildContext(dto, requesterRole: SystemRole.Admin);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void ShouldHaveError_WhenHRAttemptsToAssignAdminRole()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Admin);
        var context = BuildContext(dto, requesterRole: SystemRole.HR);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("You are not allowed to assign this role");
    }

    [Theory]
    [InlineData(SystemRole.Employee)]
    [InlineData(SystemRole.HR)]
    public void ShouldNotHaveError_WhenHRAssignsEmployeeOrHRRole(SystemRole targetRole)
    {
        // Arrange
        var dto = ValidDto(role: targetRole);
        var context = BuildContext(dto, requesterRole: SystemRole.HR);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void ShouldHaveError_WhenEmployeeAttemptsToAssignAnyRole()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Employee);
        var context = BuildContext(dto, requesterRole: SystemRole.Employee);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("You are not allowed to assign this role");
    }

    [Fact]
    public void ShouldHaveError_WhenRequesterRoleIsMissingFromRootContextData()
    {
        // Arrange
        var dto = ValidDto(role: SystemRole.Employee);
        var context = BuildContext(dto, requesterRole: null);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("You are not allowed to assign this role");
    }

    [Fact]
    public void ShouldHaveError_WhenRoleIsInvalidEnumValue()
    {
        // Arrange
        var dto = ValidDto(role: (SystemRole)999);
        var context = BuildContext(dto);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Invalid role");
    }

    // ===================== PREFERRED LANGUAGE =====================

    [Fact]
    public void ShouldHaveError_WhenPreferredLanguageIsInvalidEnumValue()
    {
        // Arrange
        var dto = new UserUpdateDto { Role = SystemRole.Employee, IsActive = true, PreferredLanguage = (Language)999 };
        var context = BuildContext(dto);

        // Act
        var result = _sut.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PreferredLanguage)
            .WithErrorMessage("Invalid preferred language");
    }
}
