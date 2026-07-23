using FluentValidation;
using HeimReport.Api.DTOs.Users;

namespace HeimReport.Api.Validators.Users;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public  RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken cannot be empty")
            .MaximumLength(255).WithMessage("RefreshToken must not exceed 255 characters.");
    }
}