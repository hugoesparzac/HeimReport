using FluentValidation;
using HeimReport.Api.DTOs.Users;
namespace HeimReport.Api.Validators.Users;
public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required.")
            .MaximumLength(100).WithMessage("Username or email must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(60).WithMessage("Password must not exceed 60 characters.");
    }
}