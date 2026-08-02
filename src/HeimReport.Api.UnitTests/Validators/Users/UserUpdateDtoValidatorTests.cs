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
}
