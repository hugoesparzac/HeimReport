using FluentValidation;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Users;
using HeimReport.Api.Security;

namespace HeimReport.Api.Validators.Users;

public class UserProvisionDtoValidator : AbstractValidator<UserProvisionDto>
{
    public UserProvisionDtoValidator(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository)
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId is required")
            .MustAsync(async (id, ct) => await employeeRepository.ExistsActiveAsync(id, ct))
            .WithMessage("Employee does not exist or is not active");

        RuleFor(x => x.EmployeeId)
            .MustAsync(async (id, ct) => !await userRepository.ExistsByEmployeeIdAsync(id, ct))
            .WithMessage("This employee already has a user account")
            .When(x => x.EmployeeId > 0);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_.-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores and hyphens")
            .MustAsync(async (username, ct) =>
                !await userRepository.ExistsByNormalizedUsernameAsync(username.ToUpperInvariant(), excludeId: null, ct))
            .WithMessage("Username is already taken");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role")
            .Must((_, role, context) => BeAllowedRoleForRequester(role, context))
            .WithMessage("You are not allowed to assign this role");

        RuleFor(x => x.PreferredLanguage)
            .IsInEnum().WithMessage("Invalid preferred language");
    }

    private static bool BeAllowedRoleForRequester(SystemRole targetRole, ValidationContext<UserProvisionDto> context)
    {
        if (!context.RootContextData.TryGetValue("RequesterRole", out var requesterRoleObj)
            || requesterRoleObj is not SystemRole requesterRole)
        {
            return false;
        }

        return RoleAssignmentPolicy.CanAssign(requesterRole, targetRole);
    }
}