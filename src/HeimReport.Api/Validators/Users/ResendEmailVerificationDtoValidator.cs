using FluentValidation;
using HeimReport.Api.DTOs.Users;

namespace HeimReport.Api.Validators.Users;

public class ResendEmailVerificationDtoValidator : AbstractValidator<ResendEmailVerificationDto>
{
    public  ResendEmailVerificationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Invalid email address.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");
    }
}