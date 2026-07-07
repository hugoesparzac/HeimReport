using FluentValidation;
using HeimReport.Api.DTOs.Auth;

namespace HeimReport.Api.Validators.Auth;

public class EmployeeRegistrationDtoValidator : AbstractValidator<EmployeeRegistrationDto>
{
    public EmployeeRegistrationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Email is required")
            .MaximumLength(100).WithMessage("Email is too long");
        
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username is too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password is too short");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}