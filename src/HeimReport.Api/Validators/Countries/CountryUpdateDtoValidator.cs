using FluentValidation;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Repositories.Countries;

namespace HeimReport.Api.Validators.Countries;

public class CountryUpdateDtoValidator : AbstractValidator<CountryUpdateDto>
{
    public CountryUpdateDtoValidator(ICountryRepository repository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid Country Id");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(60).WithMessage("Name cannot exceed 60 characters")
            .MustAsync(async (dto, name, ct) =>
                !await repository.ExistsByNameAsync(name, excludeId: dto.Id, ct))
            .WithMessage("A country with this name already exists");
    }
}
