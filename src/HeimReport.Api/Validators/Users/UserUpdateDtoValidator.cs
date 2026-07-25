using FluentValidation;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Enums;
using HeimReport.Api.Security;

namespace HeimReport.Api.Validators.Users;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role")
            .Must((_, role, context) => BeAllowedRoleForRequester(role, context))
            .WithMessage("You are not allowed to assign this role");

        RuleFor(x => x.PreferredLanguage)
            .IsInEnum().WithMessage("Invalid preferred language");
    }

    private static bool BeAllowedRoleForRequester(SystemRole targetRole, ValidationContext<UserUpdateDto> context)
    {
        if (!context.RootContextData.TryGetValue("RequesterRole", out var requesterRoleObj)
            || requesterRoleObj is not SystemRole requesterRole)
        {
            return false;
        }

        return RoleAssignmentPolicy.CanAssign(requesterRole, targetRole);
    }
}