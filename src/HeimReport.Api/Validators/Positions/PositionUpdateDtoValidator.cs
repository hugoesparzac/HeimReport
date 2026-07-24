using FluentValidation;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Repositories.Positions;

namespace HeimReport.Api.Validators.Positions;

public class PositionUpdateDtoValidator : AbstractValidator<PositionUpdateDto>
{
    public PositionUpdateDtoValidator(IPositionRepository repository)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters")
            .MustAsync(async (_, name, context, ct) =>
                !await repository.ExistsByTitleAsync(name, GetCurrentId(context), ct))
            .WithMessage("A position with this title already exists");

        RuleFor(x => x.CareerLevel)
            .IsInEnum().WithMessage("Invalid Career Level");
    }

    private static int GetCurrentId(ValidationContext<PositionUpdateDto> context)
    {
        if (context.RootContextData.TryGetValue("PositionId", out var value) && value is int id)
        {
            return id;
        }

        throw new InvalidOperationException(
            "PositionId must be set in RootContextData before validating PositionUpdateDto.");
    }
}