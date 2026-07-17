using FluentValidation;
using HeimReport.Api.DTOs.Auth;

namespace HeimReport.Api.Validators.Auth;

public class LogoutDtoValidator : AbstractValidator<LogoutDto>
{
    public LogoutDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.")
            .MaximumLength(255).WithMessage("RefreshToken must not exceed 255 characters.");
    }
}