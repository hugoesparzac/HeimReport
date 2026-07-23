using FluentValidation;
using HeimReport.Api.DTOs.Users;

namespace HeimReport.Api.Validators.Users;

public class LogoutDtoValidator : AbstractValidator<LogoutDto>
{
    public LogoutDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.")
            .MaximumLength(255).WithMessage("RefreshToken must not exceed 255 characters.");
    }
}