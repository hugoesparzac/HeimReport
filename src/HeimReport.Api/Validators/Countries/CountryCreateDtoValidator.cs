using FluentValidation;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Repositories.Countries;

namespace HeimReport.Api.Validators.Countries;

public class CountryCreateDtoValidator : AbstractValidator<CountryCreateDto>
{
    public CountryCreateDtoValidator(ICountryRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(60).WithMessage("Name cannot exceed 60 characters")
            .MustAsync(async (name, ct) =>
                !await repository.ExistsByNameAsync(name, excludeId: null, ct))
            .WithMessage("A country with this name already exists");
    }
}