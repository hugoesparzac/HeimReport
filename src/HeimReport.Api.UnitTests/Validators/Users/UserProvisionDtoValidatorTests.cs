using FluentValidation;
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

    private static ValidationContext<UserProvisionDto> BuilContext(UserProvisionDto dto, SystemRole? requesterRole = SystemRole.Admin)
    {
        var context = new ValidationContext<UserProvisionDto>(dto);

        if (requesterRole.HasValue)
        {
            context.RootContextData["RequesterRole"] = requesterRole.Value;
        }

        return context;
    }

}