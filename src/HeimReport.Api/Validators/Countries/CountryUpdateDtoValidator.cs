using FluentValidation;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Repositories.Countries;

namespace HeimReport.Api.Validators.Countries;

public class CountryUpdateDtoValidator : AbstractValidator<CountryUpdateDto>
{
    public CountryUpdateDtoValidator(ICountryRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(60).WithMessage("Name cannot exceed 60 characters")
            .MustAsync(async (_, name, context, ct) =>
                !await repository.ExistsByNameAsync(name, GetCurrentId(context), ct))
            .WithMessage("A country with this name already exists");
    }

    private static int GetCurrentId(ValidationContext<CountryUpdateDto> context)
    {
        if (context.RootContextData.TryGetValue("CountryId", out var value) && value is int id)
        {
            return id;
        }

        throw new InvalidOperationException(
            "CountryId must be set in RootContextData before validating CountryUpdateDto.");
    }
}