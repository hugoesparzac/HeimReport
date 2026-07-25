using FluentValidation;
using HeimReport.Api.DTOs.Users;

namespace HeimReport.Api.Validators.Users;

public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}