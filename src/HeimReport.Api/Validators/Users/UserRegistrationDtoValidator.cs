using FluentValidation;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Users;

namespace HeimReport.Api.Validators.Users;

public class UserRegistrationDtoValidator : AbstractValidator<UserRegistrationDto>
{
    public UserRegistrationDtoValidator(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email is too long")
            .MustAsync(async (email, ct) =>
                await employeeRepository.ExistsActiveByNormalizedEmailAsync(email.ToUpperInvariant(), ct))
            .WithMessage("No active employee record matches this email")
            .MustAsync(async (email, ct) =>
                !await userRepository.ExistsByEmployeeEmailAsync(email.ToUpperInvariant(), ct))
            .WithMessage("An account already exists for this employee");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_.-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores and hyphens")
            .MustAsync(async (username, ct) =>
                !await userRepository.ExistsByNormalizedUsernameAsync(username.ToUpperInvariant(), excludeId: null, ct))
            .WithMessage("Username is already taken");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(100).WithMessage("Password is too long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.PreferredLanguage)
            .IsInEnum().WithMessage("Invalid preferred language");
    }
}