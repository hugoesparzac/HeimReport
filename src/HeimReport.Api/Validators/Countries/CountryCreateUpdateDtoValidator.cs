using FluentValidation;
using HeimReport.Api.DTOs.Countries;

namespace HeimReport.Api.Validators.Countries;

public class CountryCreateUpdateDtoValidator : AbstractValidator<CountryCreateUpdateDto>
{
    public CountryCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(60).WithMessage("Name cannot exceed 60 characters");
    }
}