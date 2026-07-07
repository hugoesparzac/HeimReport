using FluentValidation;
using HeimReport.Api.DTOs.Positions;

namespace HeimReport.Api.Validators.Positions;

public class PositionCreateUpdateDtoValidator : AbstractValidator<PositionCreateUpdateDto>
{
    public PositionCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

        RuleFor(x => x.CareerLevel)
            .NotEmpty().WithMessage("CareerLevel is required");

        RuleFor(x => x.IsCritical)
            .NotEmpty().WithMessage("IsCritical flag is required");
    }
}